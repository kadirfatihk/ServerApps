namespace ServerApps.Business.Dtos.IisDtos
{
    public class GetServerAppDto
    {
        public string ApplicationName { get; set; }
        public string Ip { get; set; }
        public string Status { get; set; }
        public int Port { get; set; }
    }
}
