using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using YAMS.Web.APIMethods;

namespace YAMS.Web
{
    class API
    {
        public NameValueCollection QS;
        public APIMethod Method;
        public HttpListenerRequest Request;
        public HttpListenerResponse Response;
        
        public API(ref HttpListenerContext context, Server myServer)
        {
            this.QS = context.Request.QueryString;
            this.Request = context.Request;
            this.Response = context.Response;
            this.Method = new APIMethod(ref this.QS);
        }

        public void ProcessRequest()
        {
            string strOutput;
            
            switch (this.QS["action"])
            {
                case "GetLog":
                    this.Method = new GetLog(ref this.QS);
                    break;
                default:
                    break;
            }

            strOutput = this.Method.DoRequest();


            byte[] msg = System.Text.Encoding.UTF8.GetBytes(strOutput);

            this.Response.StatusCode = (int)HttpStatusCode.OK;
            this.Response.ContentType = "application/json";
            this.Response.ContentLength64 = msg.Length;
            using (Stream s = this.Response.OutputStream)
                s.Write(msg, 0, msg.Length);

        }

    }
}
