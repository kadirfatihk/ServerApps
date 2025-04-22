namespace ServerApps.WebAPI.Models
{
    public class GetServerAppModel      // controllerda dto'dan gelen veriyi dönüştürmek için kullanılır
    {
        public string ApplicationName { get; set; }
        public string Ip { get; set; }  // örnek: 127.0.0.1
        public int Port { get; set; }   // örnek: 5050, 8080
        public string Status { get; set; }  // çalışıyor, durduruldu...
    }
}