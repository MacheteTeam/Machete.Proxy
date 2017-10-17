using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public class ProxyInterfaceBase
    {
        protected string _proxyType;

        public IInvocation Invocation { set; get; }
    }
}
