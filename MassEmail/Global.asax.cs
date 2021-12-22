using LNF;
using MassEmail.Models;
using OnlineServices.Api;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace MassEmail
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray();

            var container = new Container();
            container.ConfigureDependencies();
            container.RegisterMvcControllers(assemblies);
            container.Verify();

            ServiceProvider.Setup(container.GetInstance<IProvider>());
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Simple Injector set up
            //var container = new Container();
            //container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            //// Register your stuff here
            //IOC.Configure(container);
            //container.Register<HomeModel>();

            //container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            //container.RegisterMvcIntegratedFilterProvider();
            //container.Verify();

            //DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }
    }
}