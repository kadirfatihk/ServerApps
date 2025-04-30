namespace ServerApps.WebApp.ViewModels
{
    public class ServerAppViewModel
    {
        public string ApplicationName { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public string Status { get; set; }
        //public string ServerName { get; set; }  // configden gelen Ip'ye ait isim
    }
}
