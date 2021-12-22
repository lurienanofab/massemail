using LNF;
using LNF.Mail;
using System.Linq;
using System.Web.Mvc;

namespace MassEmail.Controllers
{
    public class AjaxController : MassEmailController
    {
        public AjaxController(IProvider provider) : base(provider) { }

        [HttpGet, Route("ajax")]
        public ActionResult Test()
        {
            return Json(new { test = "Hello World!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Route("ajax/recipients")]
        public ActionResult Recipients(MassEmailRecipientArgs args)
        {
            var result = Provider.Mail.MassEmail.GetRecipients(args).OrderBy(x => x.Email).ToList();
            return Json(result);
        }
    }
}