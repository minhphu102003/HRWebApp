using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HRWebApp.Models;

namespace HRWebApp.ApiController
{
    public class Job_HistoryController : System.Web.Http.ApiController
    {
        private HRDB db = new HRDB();
        public IEnumerable<Job_History> Get()
        {
            return db.Job_History.ToList();
        }
    }
}
