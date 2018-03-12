using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using AutoMapper;
using IPS.Tracker.Data;
using SimpleInjector;
using SimpleInjector.Integration.Wcf;
using IPS.Tracker.Domain;

namespace IPS.Tracker.WCF
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WcfOperationLifestyle();

            // Register your types, for instance:
            container.Register<ITrackerRepository, OracleTrackerRepository>();

            // Register the container to the SimpleInjectorServiceHostFactory.
            SimpleInjectorServiceHostFactory.SetContainer(container);

            Mapper.Initialize(cfg => {
                cfg.CreateMap<Defect, DefectDTO>();
                cfg.CreateMap<WorkOrder, WorkOrderDTO>();
                cfg.CreateMap<Worker, WorkerDTO>();
                cfg.CreateMap<DefectComment, DefectCommentDTO>();
                cfg.CreateMap<Release, ReleaseDTO>();
            });
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}