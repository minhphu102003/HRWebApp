using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Confluent.Kafka;
using HRWebApp.Models;
using HRWebApp.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HRWebApp.Controllers
{
    public class PersonalsController : Controller
    {
        private HRDB db = new HRDB();
        private ProducerService _producerService;


        public PersonalsController()
        {
            _producerService = new ProducerService("tobi:9092"); 
        }
        // GET: Personals

        public ActionResult Index()
        {
            var personals = db.Personals.Include(p => p.Benefit_Plans1).Include(p => p.Emergency_Contacts).Include(p => p.Employment);
            return View(personals.ToList());
        }

        // GET: Personals/Details/5
        public ActionResult Details(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personal personal = db.Personals.Find(id);
            if (personal == null)
            {
                return HttpNotFound();
            }
            return View(personal);
        }

        // GET: Personals/Create
        public ActionResult Create()
        {
            ViewBag.Benefit_Plans = new SelectList(db.Benefit_Plans, "Benefit_Plan_ID", "Plan_Name");
            ViewBag.Employee_ID = new SelectList(db.Emergency_Contacts, "Employee_ID", "Emergency_Contact_Name");
            ViewBag.Employee_ID = new SelectList(db.Employments, "Employee_ID", "Employment_Status");
            return View();
        }

        // POST: Personals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Employee_ID,First_Name,Last_Name,Middle_Initial,Address1,Address2,City,State,Zip,Email,Phone_Number,Social_Security_Number,Drivers_License,Marital_Status,Gender,Shareholder_Status,Benefit_Plans,Ethnicity")] Personal personal)
        {
            if (ModelState.IsValid)
            {
                db.Personals.Add(personal);
                db.SaveChanges();
                var dataObject = new
                {
                    Employee_ID = personal.Employee_ID,
                    First_Name = personal.First_Name,
                    Last_Name = personal.Last_Name,
                    Middle_Initial = personal.Middle_Initial,
                    Address1 = personal.Address1,
                    Address2 = personal.Address2,
                    City = personal.City,
                    State = personal.State,
                    Zip = personal.Zip,
                    Email = personal.Email,
                    Phone_Number = personal.Phone_Number,
                    Social_Security_Number = personal.Social_Security_Number,
                    Drivers_License = personal.Drivers_License,
                    Marital_Status = personal.Marital_Status,
                    Gender = personal.Gender,
                    Shareholder_Status = personal.Shareholder_Status,
                    Benefit_Plans = personal.Benefit_Plans,
                    Ethnicity = personal.Ethnicity
                };

                string personalJson = JsonConvert.SerializeObject(dataObject);
                Task.Run(() => SendDataToKafka("create", personalJson));
                return RedirectToAction("Index");
            }

            ViewBag.Benefit_Plans = new SelectList(db.Benefit_Plans, "Benefit_Plan_ID", "Plan_Name", personal.Benefit_Plans);
            ViewBag.Employee_ID = new SelectList(db.Emergency_Contacts, "Employee_ID", "Emergency_Contact_Name", personal.Employee_ID);
            ViewBag.Employee_ID = new SelectList(db.Employments, "Employee_ID", "Employment_Status", personal.Employee_ID);

            return View(personal);
        }

        // GET: Personals/Edit/5
        public ActionResult Edit(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personal personal = db.Personals.Find(id);
            if (personal == null)
            {
                return HttpNotFound();
            }
            ViewBag.Benefit_Plans = new SelectList(db.Benefit_Plans, "Benefit_Plan_ID", "Plan_Name", personal.Benefit_Plans);
            ViewBag.Employee_ID = new SelectList(db.Emergency_Contacts, "Employee_ID", "Emergency_Contact_Name", personal.Employee_ID);
            ViewBag.Employee_ID = new SelectList(db.Employments, "Employee_ID", "Employment_Status", personal.Employee_ID);
            return View(personal);
        }

        // POST: Personals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Employee_ID,First_Name,Last_Name,Middle_Initial,Address1,Address2,City,State,Zip,Email,Phone_Number,Social_Security_Number,Drivers_License,Marital_Status,Gender,Shareholder_Status,Benefit_Plans,Ethnicity")] Personal personal)
        {
            if (ModelState.IsValid)
            {
                db.Entry(personal).State = EntityState.Modified;
                db.SaveChanges();
                var dataObject = new
                {
                    Employee_ID = personal.Employee_ID,
                    First_Name = personal.First_Name,
                    Last_Name = personal.Last_Name,
                    Middle_Initial = personal.Middle_Initial,
                    Address1 = personal.Address1,
                    Address2 = personal.Address2,
                    City = personal.City,
                    State = personal.State,
                    Zip = personal.Zip,
                    Email = personal.Email,
                    Phone_Number = personal.Phone_Number,
                    Social_Security_Number = personal.Social_Security_Number,
                    Drivers_License = personal.Drivers_License,
                    Marital_Status = personal.Marital_Status,
                    Gender = personal.Gender,
                    Shareholder_Status = personal.Shareholder_Status,
                    Benefit_Plans = personal.Benefit_Plans,
                    Ethnicity = personal.Ethnicity
                };
                string personalJson = JsonConvert.SerializeObject(dataObject);
                Task.Run(() => SendDataToKafkaUpdate("update", personalJson));
                return RedirectToAction("Index");
            }
            ViewBag.Benefit_Plans = new SelectList(db.Benefit_Plans, "Benefit_Plan_ID", "Plan_Name", personal.Benefit_Plans);
            ViewBag.Employee_ID = new SelectList(db.Emergency_Contacts, "Employee_ID", "Emergency_Contact_Name", personal.Employee_ID);
            ViewBag.Employee_ID = new SelectList(db.Employments, "Employee_ID", "Employment_Status", personal.Employee_ID);
            return View(personal);
        }

        // GET: Personals/Delete/5
        public ActionResult Delete(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personal personal = db.Personals.Find(id);
            if (personal == null)
            {
                return HttpNotFound();
            }
            return View(personal);
        }

        // POST: Personals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(decimal id)
        {
            Personal personal = db.Personals.Find(id);
            db.Personals.Remove(personal);
            db.SaveChanges();
            Task.Run(() => SendDeleteMessageToKafka(id));
            return RedirectToAction("Index");
        }

        public async Task SendDataToKafka(string eventType, string data)
        {
            var eventData = new EventData
            {
                EventType = eventType,
                Data = data
            };
            string json = JsonConvert.SerializeObject(eventData);
            await _producerService.ProduceAsync("hrtest1", json, 0);
        }

        public async Task SendDataToKafkaUpdate(string eventType, string data)
        {
            var eventData = new EventData
            {
                EventType = eventType,
                Data = data
            };

            string json = JsonConvert.SerializeObject(eventData);

            await _producerService.ProduceAsync("hrtest1", json, 0);
        }

        private async Task SendDeleteMessageToKafka(decimal id)
        {
            string jsonData = $"{{\"employeeId\": {id}}}";
            var eventData = new EventData
            {
                EventType = "delete",
                Data = jsonData
            };
            string json = JsonConvert.SerializeObject(eventData);
            // Gửi message lên Kafka thông qua ProducerService
            await _producerService.ProduceAsync("hrtest1", json, 0);
        }


        // Serialize dữ liệu personal thành message string (bạn có thể sử dụng JSON hoặc các phương pháp khác)
        private string SerializePersonal(Personal personal)
        {
            // Code serialize dữ liệu
            return "Serialized personal data";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
