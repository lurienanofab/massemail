using LNF;
using LNF.Data;
using LNF.Mail;
using MassEmail.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MassEmail.Controllers
{
    [Authorize]
    public class HomeController : MassEmailController
    {
        private List<string> _errors;
        private List<Attachment> _attachments;

        public HomeController(IProvider provider) : base(provider) { }

        [HttpGet, Route("")]
        public ActionResult Index()
        {
            var selectedTab = GetSelectedTab();

            ViewBag.SelectedTab = selectedTab;
            ViewBag.Errors = new List<string>();
            ViewBag.Message = string.Empty;

            if (selectedTab == "admin")
            {
                return RedirectToAction("Admin");
            }
            else
            {
                var model = new HomeModel
                {
                    From = CurrentUser.Email,
                    DisplayName = CurrentUser.DisplayName,
                    SelectedValues = JsonConvert.SerializeObject(new int[0])
                };

                InitModel(model, selectedTab);

                return View(model);
            }
        }

        [HttpPost, Route("")]
        public ActionResult Index(HomeModel model)
        {
            InitModel(model, model.SelectedTab);
            ViewBag.SelectedTab = model.SelectedTab;

            ViewBag.Message = string.Empty;

            var group = model.SelectedTab;

            if (string.IsNullOrEmpty(group))
                throw new Exception("Cannot determine recipient group.");

            var values = JsonConvert.DeserializeObject<IEnumerable<int>>(model.SelectedValues);

            _errors = new List<string>();

            if (values.Count() == 0)
                _errors.Add("You have not selected any recipients.");

            if (string.IsNullOrEmpty(model.From))
                _errors.Add("From address is required.");

            if (string.IsNullOrEmpty(model.DisplayName))
                _errors.Add("Display name is required.");

            if (string.IsNullOrEmpty(model.Subject))
                _errors.Add("Subject is required.");

            if (string.IsNullOrEmpty(model.Body))
                _errors.Add("Body is required.");

            if (_errors.Count == 0)
            {
                var args = new MassEmailSendArgs
                {
                    ClientID = CurrentUser.ClientID,
                    Caller = "MassEmail.Controllers.HomeController.Index",
                    Group = group,
                    Values = values,
                    From = model.From,
                    CC = model.CC,
                    DisplayName = model.DisplayName,
                    Subject = model.Subject,
                    Body = model.Body
                };

                if (GetAttachments())
                {
                    if (_attachments.Count > 0)
                    {
                        var guid = Provider.Mail.Attachment.Attach(_attachments);
                        args.Attachments = guid;
                    }

                    var sent = Provider.Mail.SendMassEmail(args);

                    ViewBag.Message = $"Mail sent OK! Emails sent: {sent}";
                }
            }

            ViewBag.Errors = _errors ?? new List<string>(); // empty list if there were no errors

            return View(model);
        }

        [HttpGet, Route("admin")]
        public ActionResult Admin(bool active = true, int? emailId = null)
        {
            var invalid = GetInvalidEmails(active);

            var model = new AdminModel
            {
                IsAdmin = IsAdmin(),
                SelectedTab = "admin",
                InvalidEmails = invalid,
                ShowDeleted = !active,
                EditEmailID = 0
            };

            if (emailId.HasValue)
            {
                var modify = invalid.FirstOrDefault(x => x.EmailID == emailId.GetValueOrDefault());
                if (modify != null)
                {
                    model.EditEmailID = modify.EmailID;
                    model.EditDisplayName = modify.DisplayName;
                    model.EditEmailAddress = modify.EmailAddress;
                }
            }

            return View(model);
        }

        [HttpGet, Route("admin/invalid-email/{command}")]
        public ActionResult AdminInvalidEmailCommand(string command, int emailId)
        {
            bool value = command != "delete";
            Provider.Mail.MassEmail.SetInvalidEmailActive(emailId, value);
            return RedirectToAction("Admin");
        }

        [HttpPost, Route("admin")]
        public ActionResult Admin(AdminModel model)
        {
            IInvalidEmail invalidEmail;

            ViewBag.ErrorMessage = string.Empty;

            if (EmailIsValid(model.EditEmailAddress))
            {
                if (model.EditEmailID == 0)
                {
                    invalidEmail = new InvalidEmailItem
                    {
                        DisplayName = model.EditDisplayName,
                        EmailAddress = model.EditEmailAddress,
                        IsActive = true
                    };

                    Provider.Mail.MassEmail.AddInvalidEmail(invalidEmail);

                    return RedirectToAction("Admin");
                }
                else
                {
                    invalidEmail = new InvalidEmailItem
                    {
                        EmailID = model.EditEmailID,
                        DisplayName = model.EditDisplayName,
                        EmailAddress = model.EditEmailAddress,
                        IsActive = true
                    };

                    Provider.Mail.MassEmail.ModifyInvalidEmail(invalidEmail);

                    return RedirectToAction("Admin");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid email address.";

                model.IsAdmin = IsAdmin();
                model.SelectedTab = "admin";
                model.InvalidEmails = GetInvalidEmails(true);
                model.ShowDeleted = true;

                return View(model);
            }
        }

        [Route("nav/{tab}")]
        public ActionResult Navigate(string tab)
        {
            Session["SelectedTab"] = tab;
            return RedirectToAction("Index");
        }

        private bool IsAdmin()
        {
            return CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);
        }

        private string GetSelectedTab()
        {
            if (Session["SelectedTab"] == null)
                Session["SelectedTab"] = "privilege";

            var result = Convert.ToString(Session["SelectedTab"]);

            Session.Remove("SelectedTab");

            return result;
        }

        private void InitModel(MassEmailModel model, string selectedTab)
        {
            model.IsAdmin = IsAdmin();
            model.SelectedTab = selectedTab;
            model.Privs = Provider.Data.Client.GetPrivs();
            model.Communities = Provider.Data.Client.GetCommunities();
            model.Managers = Provider.Data.Client.AllActiveManagers();
            model.Tools = Provider.Scheduler.Resource.AllActiveResources();
            model.Areas = Provider.Data.Room.GetPassbackRooms();
        }

        private bool GetAttachments()
        {
            _attachments = new List<Attachment>();
            _errors = new List<string>();

            var result = true;

            for (var x = 0; x < Request.Files.Count; x++)
            {
                var file = Request.Files[x];

                if (file != null && !string.IsNullOrEmpty(file.FileName))
                {
                    if (file.ContentLength > 0)
                    {
                        if (IsAllowedFileType(file))
                        {
                            using (var ms = new MemoryStream(file.ContentLength))
                            {
                                file.InputStream.CopyTo(ms);
                                var a = new Attachment
                                {
                                    FileName = file.FileName,
                                    Data = ms.ToArray()
                                };
                                _attachments.Add(a);
                            }
                        }
                        else
                        {
                            _errors.Add($"File {file.FileName}: file type is not allowed.");
                            result = false;
                        }
                    }
                    else
                    {
                        _errors.Add($"File {file.FileName}: file is empty.");
                        result = false;
                    }
                }
            }

            return result;
        }

        private bool IsAllowedFileType(HttpPostedFileBase file)
        {
            var ext = Path.GetExtension(file.FileName);
            return GetAllowedFileTypes().Contains(ext);
        }

        private string[] GetAllowedFileTypes()
        {
            var setting = ConfigurationManager.AppSettings["AllowedFileTypes"];

            if (string.IsNullOrEmpty(setting))
                throw new Exception("Missing required appSetting: AllowedFileTypes");

            return setting.Split(',');
        }

        private bool EmailIsValid(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                var ma = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private IEnumerable<IInvalidEmail> GetInvalidEmails(bool active)
        {
            return Provider.Mail.MassEmail.GetInvalidEmails(active).OrderBy(x => x.DisplayName).ThenBy(x => x.EmailAddress).ToList();
        }
    }
}