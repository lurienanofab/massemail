using LNF;
using LNF.Data;
using System.Web.Mvc;

namespace MassEmail.Controllers
{
    public abstract class MassEmailController : Controller
    {
        public IProvider Provider { get; }
        public IClient CurrentUser { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            CurrentUser = Provider.Data.Client.GetClient(HttpContext.User.Identity.Name);
        }

        protected MassEmailController(IProvider provider)
        {
            Provider = provider;
        }
    }
}