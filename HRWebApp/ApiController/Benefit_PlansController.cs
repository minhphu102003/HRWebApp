using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HRWebApp.Models;

namespace HRWebApp.ApiController
{
    public class Benefit_PlansController : System.Web.Http.ApiController
    {
        private HRDB db = new HRDB(); // Đảm bảo rằng bạn đã import namespace cho lớp HRDB

        // GET: api/Benefit_Plans
        public IEnumerable<Benefit_Plans> Get()
        {
            return db.Benefit_Plans.ToList();
        }
    }
}
