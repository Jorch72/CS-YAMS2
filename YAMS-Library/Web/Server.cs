using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using YAMS.Functions;
using System.Text.RegularExpressions;

namespace YAMS.Web
{
    class Server
    {
        private HttpListener _listener = new HttpListener();
        private Thread _clientThread;

        private Dictionary<string, Session> _sessions = new Dictionary<string, Session>();

        private string _basePath = Environment.CurrentDirectory + @"\web\";

        public Server()
        {
        }

        public void Start()
        {
            this._listener.Start();
            this._listener.Prefixes.Add("http://127.0.0.1:8080/");
            this._clientThread = new Thread(new ThreadStart(this._clientListener));
            this._clientThread.Start();
        }

        public void Stop()
        {
            this._clientThread.Abort();
            this._listener.Stop();
        }

        private void _clientListener()
        {
            while (true)
            {
                try
                {
                    HttpListenerContext request = this._listener.GetContext();
                    ThreadPool.QueueUserWorkItem(this._processRequest, request);
                }
                catch (Exception e) { Log.Write(e.Message); }
            }
        }
        
        private void _processRequest(object listenerContext)
        {
            var context = (HttpListenerContext)listenerContext;
            HttpListenerResponse resp = context.Response;
            HttpListenerRequest req = context.Request;

            try
            {
                //Set defaults
                context.Response.Headers["Server"] = "YAMS Web Server - powered by ";
                
                //Get the URL the client asked for
                string strRequestedURL = req.Url.AbsolutePath;

                //Work out the class of the request
                Regex regRequest = new Regex(@"^/(.+?)/(.*)$", RegexOptions.IgnoreCase);
                Match matRequest = regRequest.Match(strRequestedURL);

                //Byte to hold the response
                byte[] msg;

                if (matRequest.Groups.Count == 1) throw new NotFoundException();

                //What is the first part of the address?  This tells us how to start handling the request
                switch (matRequest.Groups[1].Captures[0].Value)
                {
                    case "login":
                    case "api":
                        API _api = new API(ref context, this);
                        _api.ProcessRequest();
                        break;
                    default:
                        //Static asset, just serve file as is
                        string filename = this._basePath + strRequestedURL.Replace("/", "\\");
                        if (!File.Exists(filename))
                        {
                            throw new NotFoundException();
                        }
                        else
                        {
                            resp.StatusCode = (int)HttpStatusCode.OK;
                            resp.ContentType = this._getContentType(Path.GetExtension(filename));
                            msg = File.ReadAllBytes(filename);
                            
                            context.Response.ContentLength64 = msg.Length;
                            using (Stream s = context.Response.OutputStream)
                                s.Write(msg, 0, msg.Length);
                        }
                        break;
                }
            }
            catch (NotFoundException)
            {
                Log.Write("Requested path not found: " + context.Request.Url.AbsolutePath);
                this._sendError((int)HttpStatusCode.NotFound, ref context);
            }
            catch (NotAuthorisedException)
            {
                Log.Write("Requested not authorised: " + context.Request.Url.AbsolutePath);
                this._sendError((int)HttpStatusCode.Unauthorized, ref context);
            }
            catch (NotImplementedException)
            {
                Log.Write("Requested not implemented: " + context.Request.Url.AbsolutePath);
                this._sendError((int)HttpStatusCode.NotImplemented, ref context);
            }
            catch (Exception e)
            {
                Log.Write("Totally failed: " + context.Request.Url.AbsolutePath);
                this._sendError((int)HttpStatusCode.InternalServerError, ref context, "Internal Server Error", e.Message);
            }
            finally
            {
                context.Response.Close();
            }
        }

        private string _getContentType(string strExt)
        {
            switch (strExt)
            {
                case ".js": return "text/javascript"; // Not application/javascript because http://stackoverflow.com/a/4101763/443316
                case ".html": return "text/html";
                case ".json": return "application/json";
                case ".css": return "text/css";
                case ".png": return "image/png";
                default: return "application/octet-stream";
            }
        }

        private void _sendError(int intStatusCode, ref HttpListenerContext context, string strStatusText = null, string strMessage = null)
        {
            context.Response.StatusCode = intStatusCode;
            if (strStatusText != null)
            {
                context.Response.StatusDescription = strStatusText;
            }
            else
            {
                switch (intStatusCode)
                {
                    case 401:
                        context.Response.StatusDescription = "Unauthorized";
                        strMessage = File.ReadAllText(this._basePath + "\\errordocs\\401.html");
                        break;
                    case 404:
                        context.Response.StatusDescription = "Not Found";
                        strMessage = File.ReadAllText(this._basePath + "\\errordocs\\404.html");
                        break;
                    case 501:
                        context.Response.StatusDescription = "Not Implemented";
                        strMessage = File.ReadAllText(this._basePath + "\\errordocs\\501.html");
                        break;
                }
            }
            context.Response.ContentType = "text/html";
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(strMessage);
            context.Response.ContentLength64 = msg.Length;
            using (Stream s = context.Response.OutputStream)
                s.Write(msg, 0, msg.Length);
        }

        //Exceptions
        public class NotFoundException : System.Exception
        {
            public NotFoundException() {}
            public NotFoundException(string message) : base(message) {}
            public NotFoundException(string message, Exception innerException): base(message, innerException) {}
        }
        public class NotAuthorisedException : System.Exception
        {
            public NotAuthorisedException() { }
            public NotAuthorisedException(string message) : base(message) { }
            public NotAuthorisedException(string message, Exception innerException) : base(message, innerException) { }
        }
    }



}
