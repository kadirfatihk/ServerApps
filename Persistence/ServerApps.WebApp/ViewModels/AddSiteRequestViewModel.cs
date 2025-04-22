namespace ServerApps.WebApp.Models
{
    public class AddSiteRequestViewModel
    {
        public string ApplicationName { get; set; }
        public string Ip { get; set; }
        public string Status { get; set; }
        public int Port { get; set; }
    }
}
