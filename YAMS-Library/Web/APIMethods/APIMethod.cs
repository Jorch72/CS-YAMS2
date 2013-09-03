using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace YAMS.Web.APIMethods
{
    class APIMethod
    {
        public readonly bool RequiresAuth = false;

        public NameValueCollection _qs;

        public APIMethod(ref NameValueCollection QS)
        {
            this._qs = QS;
        }

        public virtual string DoRequest() {
            throw new NotImplementedException();
        }
    }
}
