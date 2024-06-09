using Confluent.Kafka;
using HRWebApp.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Threading;


namespace HRWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Task.Run(async () =>
            {
                var consumerService = new ConsumerService("localhost:9092", "hr", "middlewaretest");
                await consumerService.StartListening(CancellationToken.None);
            });

            Task.Run(async () =>
            {
                var consumer = new ConsumerSip("localhost:9092", "hr-sip", "siptest");
                await consumer.StartListening(CancellationToken.None);
            });

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

        }
    }
}
