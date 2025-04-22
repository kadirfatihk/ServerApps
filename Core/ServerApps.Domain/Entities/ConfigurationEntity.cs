using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Domain.Entities
{
    public class ConfigurationEntity    // kullanıcı için ayarlar... bağlantı(eşleşme) için gerekli...
    {
        public string Ip { get; set; }      
        public string? Username { get; set; }   
        public string? Password { get; set; }
        //public bool IsLocal { get; set; }
    }
}