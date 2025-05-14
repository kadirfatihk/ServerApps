using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Dtos.IisDtos
{
    public class UpdatePortDto
    {
        public string Ip { get; set; }
        public string SiteName { get; set; }
        public int NewPort { get; set; }
    }
}
