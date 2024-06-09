using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using HRWebApp.ModelService;
using HRWebApp.Models;

namespace HRWebApp.Service
{
    // File code này dùng để dọc message do Dashboard làm producer
    // Các hàm xử lỹ chuỗi cũng được dành cho dữ liệu message Dashboard
    public class ConsumerService
    {
        private HRDB db = new HRDB();
        private readonly IConsumer<Ignore, string> _consumer;

        public ConsumerService(string bootstrapServers, string groupId, string topic)
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

        // Như một người coder tay ngang em gặp một số vấn đề về parseJson, cuống quá nên em viết luôn hàm sử lý chuỗi  
        private DeleteEmployeeMiddleware ParseDeleteEmployee(string employeeData)
        {
            employeeData = employeeData.Substring(1, employeeData.Length - 2);
            DeleteEmployeeMiddleware em = new DeleteEmployeeMiddleware();
            string[] splitData = employeeData.Split(',');
            foreach (string item in splitData)
            {
                string[] keyValue = item.Split(':');
                string key = keyValue[0].Trim('"');
                string value = keyValue[1].Trim('"');
                switch (key)
                {
                    case "employeeId":
                        // Thiếu try cacth
                        em.employeeId = decimal.Parse(value);
                        break;
                    case "id":
                        em.id = value;
                        break;
                    default:
                        break;
                }
            }
            return em;
        }
        // Tiếp tục là một hàm xử lý chuỗi 
        private EmployeeMiddleware ParseEmployeeData(string employeeData)
        {
            // Loại bỏ dấu ngoặc kép ở hai đầu xâu
            employeeData = employeeData.Substring(1, employeeData.Length - 2);

            // Tạo một đối tượng EmployeeMiddleware mới
            EmployeeMiddleware employeeObject = new EmployeeMiddleware();

            // Tách dữ liệu thành các cặp key-value và gán giá trị cho các thuộc tính của đối tượng
            string[] splitData = employeeData.Split(',');
            foreach (string item in splitData)
            {
                string[] keyValue = item.Split(':');
                string key = keyValue[0].Trim('"');
                string value = keyValue[1].Trim('"');
                switch (key)
                {
                    case "employeeId":
                        employeeObject.employeeId = decimal.Parse(value);
                        break;
                    case "firstName":
                        employeeObject.firstName = value;
                        break;
                    case "lastName":
                        employeeObject.lastName = value;
                        break;
                    case "shareHolder":
                        employeeObject.shareHolder = bool.Parse(value);
                        break;
                    case "gender":
                        employeeObject.gender = bool.Parse(value);
                        break;
                    case "ethnicity":
                        employeeObject.ethnicity = value;
                        break;
                    case "paidToDate":
                        if (float.TryParse(value, out float parsedPaidToDate))
                        {
                            employeeObject.paidToDate = parsedPaidToDate;
                        }
                        else
                        {
                            employeeObject.paidToDate = 0;
                        }
                        break;
                    case "paidLastYear":
                        if (float.TryParse(value, out float parsedPaidLastYear))
                        {
                            employeeObject.paidLastYear = parsedPaidLastYear;
                        }
                        else
                        {
                            employeeObject.paidLastYear = 0;
                        }
                        break;
                    case "employmentStatus":
                        employeeObject.employmentStatus = value;
                        break;
                    case "vacationDays":
                        if (int.TryParse(value, out int parsedVacationDays))
                        {
                            employeeObject.vacationDays = parsedVacationDays;
                        }
                        else
                        {
                            employeeObject.vacationDays = 0;
                        }
                        break;
                    case "benefitID":
                        if (int.TryParse(value, out int parsedBenefitID))
                        {
                            employeeObject.benefitID = parsedBenefitID;
                        }
                        else
                        {
                            employeeObject.benefitID = 0;
                        }
                        break;
                    case "idMongo":
                        employeeObject.idMongo = value;
                        break;
                    default:
                        break;
                }
            }

            return employeeObject;
        }


        public async Task StartListening(CancellationToken cancellationToken)
        {
            Trace.WriteLine("Comsumer is starting to listen for topic middleware.");
            // Thêm listener để ghi log vào tệp tin đã cấu hình
            // Sử dụng logfile để debug vì không biết ASP.NET web console ở đâu 

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    var jsonData = consumeResult.Message.Value;
                    jsonData = jsonData.Substring(1, jsonData.Length - 2);
                    var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
                    var messageObject = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageMiddleware>(jsonObject.ToString());
                    if (messageObject != null)
                    {
                        string employee = messageObject.Data;
                        if (messageObject.EventType == "create")
                        {
                            EmployeeMiddleware em = ParseEmployeeData(employee);
                            Personal personal = new Personal();
                            personal.Employee_ID = em.employeeId;
                            personal.First_Name = em.firstName;
                            personal.Last_Name = em.lastName;
                            personal.Gender = em.gender;
                            personal.Shareholder_Status = em.shareHolder;
                            personal.Ethnicity = em.ethnicity;
                            db.Personals.Add(personal);
                            db.SaveChanges();
                            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            // Ghi sự thay đổi dữ liệu vào logfile 
                            // Kết hợp với thông tin đối tượng
                            string logMessage = $"[{currentTime}] {messageObject.EventType} {em.ToString()}";
                            Trace.WriteLine(logMessage);
                        }
                        else if(messageObject.EventType == "update")
                        {
                            EmployeeMiddleware em = ParseEmployeeData(employee);
                            var existingPersonal = db.Personals.Find(em.employeeId);
                            Trace.WriteLine(em.ToString());
                            if (existingPersonal != null)
                            {
                                // Cập nhật thông tin của nhân viên
                                existingPersonal.First_Name = em.firstName;
                                existingPersonal.Last_Name = em.lastName;
                                existingPersonal.Gender = em.gender;
                                existingPersonal.Shareholder_Status = em.shareHolder;
                                existingPersonal.Ethnicity = em.ethnicity;

                                // Lưu các thay đổi vào cơ sở dữ liệu
                                db.SaveChanges();
                                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                // Kết hợp với thông tin đối tượng
                                string logMessage = $"[{currentTime}] {messageObject.EventType} {em.ToString()}";
                                Trace.WriteLine(logMessage);
                            }
                            else
                            {
                                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                Trace.WriteLine($"[{currentTime}] Không tìm thấy nhân viên có ID: {em.employeeId} để cập nhật.");
                            }
                        }
                        else if(messageObject.EventType == "delete")
                        {
                            // Chỉ delete với những nhân viên không có khóa ngoại ở những bảng khác 
                            Trace.WriteLine(employee);
                            DeleteEmployeeMiddleware em = ParseDeleteEmployee(employee);
                            Personal existingPersonal = db.Personals.Find(em.employeeId);
                            Trace.WriteLine(em.ToString());
                            if (existingPersonal != null)
                            {
                                db.Personals.Remove(existingPersonal); // Đánh dấu đối tượng cần xóa
                                // Ở đây đang xuất hiện một bug mà em chưa có thời gian fix 
                                // Bảng Personal có một số bảng khác đang tham chiếu khóa ngoại [Employment, Jobhistory,..]
                                // Nếu chọn đúng đối tượng có khóa chính là khóa ngoại đang được tham chiếu ở bảng khác thì sẽ không xóa được và 
                                // break vòng đọc message từ Kafka, khi start app lại thì vẫn tiếp tục đọc message đó và thoát vòng (có nghĩa message vẫn chưa được dọc bởi comsumer này) 

                                db.SaveChanges(); // Thực hiện xóa từ cơ sở dữ liệu
                                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                Trace.WriteLine($"[{currentTime}] {messageObject.EventType} {em.employeeId}");
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
