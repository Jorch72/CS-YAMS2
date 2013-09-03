using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace YAMS.Web
{
    class Session
    {
        public string AuthCode;
        public DateTime Expiry;
        public IPAddress IP;
        public string UserAgent;
        public User User;
    }
}
