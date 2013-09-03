using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YAMS.Functions;

namespace YAMS
{
    public static class Core
    {
        public static void StartUp()
        {          
            Web.Server _webServer = new Web.Server();
            _webServer.Start();
        }
    }
}
