using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerBiApplication.PowerBI
{
    public class AppSettings
    {
        public string AuthorityUri { get; set; }
        public string ResourceUrl { get; set; }
        public string RedirectUrl { get; set; }
        public string ApiUrl { get; set; }
        public string ApplicationId { get; set; }
        public Guid GroupId { get; set; }
        public string ReportId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
