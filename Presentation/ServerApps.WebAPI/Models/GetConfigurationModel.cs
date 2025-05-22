namespace ServerApps.WebAPI.Models
{
    public class GetConfigurationModel
    {
        public string ApplicationName { get; set; }
        public string Ip { get; set; }      // bağlanacak ip adresi
        public string? Username { get; set; }    // bağlanacak kullanıcı adı
        public string? Password { get; set; }    // şifresi
    }
}
