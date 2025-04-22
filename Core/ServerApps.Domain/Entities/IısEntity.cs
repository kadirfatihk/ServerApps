using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Domain.Entities
{
    public class IısEntity      // uygulamanın iis üzerinde çalıştığı bilgiler
    {
        public string ApplicationName { get; set; } 
        public string Ip { get; set; }      // örnek: 127.0.0.1
        public int Port { get; set; }    // örnek: 5050, 8080
        public string Status { get; set; }      // çalışıyor, durduruldu...
    }
}