namespace ServerApps.WebApp.ViewModels
{
    public class EventLogListViewModel
    {
        public DateTime? TimeCreated { get; set; }
        public string Level { get; set; }
        public string Source { get; set; }
        public string Id { get; set; }
        public string TaskCategory { get; set; }
        public string Message { get; set; }
    }
}
