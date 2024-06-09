using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HRWebApp.Models;

namespace HRWebApp.ApiController
{
    public class Emergency_ContactsController : System.Web.Http.ApiController
    {
        private HRDB db = new HRDB();
        public IEnumerable<Benefit_Plans> Get()
        {
            return db.Benefit_Plans.ToList();
        }
    }
}
