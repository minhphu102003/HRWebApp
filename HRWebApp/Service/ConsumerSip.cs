using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using HRWebApp.ModelService;
using HRWebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HRWebApp.Service
{
    // File code dùng để đọc message từ topic Sip làm producer (Dashboard cũng có vai trò producer trong topic này (sửa, xóa dữ liệu))
    public class ConsumerSip
    {
        private HRDB db = new HRDB();
        private readonly IConsumer<Ignore, string> _consumer;
        private static readonly Random random = new Random();

        public ConsumerSip(string bootstrapServers, string groupId, string topic)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest // Đặt lại offset để nhận tất cả các message từ đầu
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            _consumer.Subscribe(topic); // Đăng ký consumer cho topic cụ thể
        }

        // Một hàm xử lý chuỗi tương tự 
        private EmployeeSip ParseEmployeeData(string employeeData)
        {
            // Loại bỏ dấu ngoặc kép ở hai đầu xâu
            employeeData = employeeData.Substring(1, employeeData.Length - 2);

            // Tạo một đối tượng EmployeeMiddleware mới
            EmployeeSip employeeObject = new EmployeeSip();

            // Tách dữ liệu thành các cặp key-value và gán giá trị cho các thuộc tính của đối tượng
            string[] splitData = employeeData.Split(',');
            foreach (string item in splitData)
            {
                string[] keyValue = item.Split(':');
                string key = keyValue[0].Trim();
                key = key.Trim('"');
                string value = keyValue[1].Trim();
                Trace.WriteLine(key);
                Trace.WriteLine(value);
                switch (key)
                {
                    case "_id":
                        employeeObject._id = value.Trim('"');
                        break;
                    case "firstName":
                        employeeObject.firstName = value.Trim('"');
                        break;
                    case "lastName":
                        employeeObject.lastName = value.Trim('"');
                        break;
                    case "vacationDays":
                        if (int.TryParse(value, out int vacationDays))
                        {
                            employeeObject.vacationDays = vacationDays;
                        }
                        else
                        {
                            employeeObject.vacationDays = 0;
                        }
                        break;
                    case "payRate":
                        if (int.TryParse(value, out int payRate))
                        {
                            employeeObject.payRate = payRate;
                        }
                        else
                        {
                            employeeObject.payRate = 0;
                        }
                        break;
                    case "payRateId":
                        if (int.TryParse(value, out int payRateId))
                        {
                            employeeObject.payRateId = payRateId;
                        }
                        else
                        {
                            employeeObject.payRateId = 0;
                        }
                        break;
                    case "paidToDate":
                        if (int.TryParse(value, out int parsedPaidToDate))
                        {
                            employeeObject.paidToDate = parsedPaidToDate;
                        }
                        else
                        {
                            employeeObject.paidToDate = 0;
                        }
                        break;
                    case "paidLastYear":
                        if (int.TryParse(value, out int parsedPaidLastYear))
                        {
                            employeeObject.paidLastYear = parsedPaidLastYear;
                        }
                        else
                        {
                            employeeObject.paidLastYear = 0;
                        }
                        break;
                    case "employeeId":
                        {
                            value = value.Trim('"');
                            if (int.TryParse(value, out int employeeId))
                            {
                                employeeObject.employeeId = employeeId;
                            }
                            else
                            {
                                int min = 1000000;
                                int max = 9999999;
                                employeeObject.employeeId = random.Next(min, max);
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            return employeeObject;
        }

        private DeleteEmployeeMiddleware ParseDeleteEmployee(string employeeData)
        {
            employeeData = employeeData.Substring(1, employeeData.Length - 2);
            DeleteEmployeeMiddleware em = new DeleteEmployeeMiddleware();
            string[] splitData = employeeData.Split(',');
            foreach (string item in splitData)
            {
                string[] keyValue = item.Split(':');
                string key = keyValue[0].Trim();
                key = key.Trim('"');
                string value = keyValue[1].Trim();
                switch (key)
                {
                    case "employeeId":
                        if (decimal.TryParse(value, out decimal parsedPaidToDate))
                        {
                            em.employeeId = parsedPaidToDate;
                        }
                        else
                        {
                            em.employeeId = 0;
                        }
                        break;
                    case "id":
                        em.id = value.Trim('"');
                        break;
                    default:
                        break;
                }
            }
            return em;
        }

        public async Task StartListening(CancellationToken cancellationToken)
        {
            Trace.WriteLine("Comsumer is starting to listen for topic sip.");
            // Thêm listener để ghi log vào tệp tin đã cấu hình

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    var jsonData = consumeResult.Message.Value;
                    var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonData);
                    string eventType = jsonObject["EventType"].ToString();
                    string dataObject = jsonObject["Data"].ToString();
                    if (jsonObject != null)
                    {
                        if (eventType == "create")
                        {
                            EmployeeSip em = ParseEmployeeData(dataObject);
                            Personal personal = new Personal();
                            personal.Employee_ID = em.employeeId;
                            personal.First_Name = em.firstName;
                            personal.Last_Name = em.lastName;
                            personal.Gender = false;
                            db.Personals.Add(personal);
                            db.SaveChanges();
                            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            string logMessage = $"[{currentTime}] {eventType} {em.ToString()}";
                            Trace.WriteLine(logMessage);
                        }
                        else if (eventType == "updateMiddleware")
                        {
                            EmployeeSip em = ParseEmployeeData(dataObject);
                            var existingPersonal = db.Personals.Find(em.employeeId);
                            Trace.WriteLine(em.ToString());
                            if (existingPersonal != null)
                            {
                                existingPersonal.First_Name = em.firstName;
                                existingPersonal.Last_Name = em.lastName;
                                db.SaveChanges();
                                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                string logMessage = $"[{currentTime}] {eventType} {em.ToString()}";
                                Trace.WriteLine(logMessage);
                            }
                        }
                        else if (eventType == "deleteMiddleware")
                        {
                            Trace.WriteLine(dataObject);
                            DeleteEmployeeMiddleware em = ParseDeleteEmployee(dataObject);
                            Personal existingPersonal = db.Personals.Find(em.employeeId);
                            if (existingPersonal != null)
                            {
                                // Delete có một lỗi em đã đề cập ở file ComsumerService.cs mà vẫn chưa có thời gian fix 
                                db.Personals.Remove(existingPersonal); // Đánh dấu đối tượng cần xóa
                                db.SaveChanges(); // Thực hiện xóa từ cơ sở dữ liệu
                                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                Trace.WriteLine($"[{currentTime}] {eventType} {em.employeeId}");
                            }
                            else
                            {
                                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                Trace.WriteLine($"[{currentTime}] Không tìm thấy personal {em.employeeId}");
                            }
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    Trace.WriteLine($"Error occurred: {e.Error.Reason}");
                }
            }
        }
        public void StopListening()
        {
            _consumer.Close();
        }

    }
}