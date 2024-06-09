using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HRWebApp.Models;

namespace HRWebApp.ApiController
{
    public class EmploymentController : System.Web.Http.ApiController
    {
        private HRDB db = new HRDB(); // Đảm bảo rằng bạn đã import namespace cho lớp HRDB

        // GET: api/Benefit_Plans
        public IEnumerable<Employment> Get()
        {
            return db.Employments.ToList();
        }
    }
}
