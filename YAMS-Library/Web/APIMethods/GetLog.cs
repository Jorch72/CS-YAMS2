using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace YAMS.Web.APIMethods
{
    class GetLog : APIMethod
    {
        public readonly bool RequiresAuth = true;

        public GetLog(ref NameValueCollection QS) : base(ref QS) { }

        public override string DoRequest()
        {
            return "{\"response\": \"GetLog\" }";
        }
    }
}
