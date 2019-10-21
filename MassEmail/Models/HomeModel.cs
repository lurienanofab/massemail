namespace MassEmail.Models
{
    public class HomeModel : MassEmailModel
    {
        public string From { get; set; }
        public string CC { get; set; }
        public string DisplayName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SelectedValues { get; set; }
    }
}