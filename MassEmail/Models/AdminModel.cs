using LNF.Mail;
using System.Collections.Generic;

namespace MassEmail.Models
{
    public class AdminModel : MassEmailModel
    {
        public IEnumerable<IInvalidEmail> InvalidEmails { get; set; }
        public bool ShowDeleted { get; set; }
        public int EditEmailID { get; set; }
        public string EditDisplayName { get; set; }
        public string EditEmailAddress { get; set; }
    }
}