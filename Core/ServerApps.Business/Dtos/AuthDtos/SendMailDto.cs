using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Dtos.AuthDtos
{
    public class SendMailDto
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<string> ToMailAdress { get; set; }
    }
}
