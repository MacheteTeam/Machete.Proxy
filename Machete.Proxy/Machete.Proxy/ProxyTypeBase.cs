using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public class ProxyTypeBase<T>
    {
        protected T _proxyObject;

        public IIntercept Intercept { set; get; }

        //public ProxyBase(T proxyObject)
        //{

        //    _proxyObject = proxyObject;
        //    // _proxyObject = (T)Activator.CreateInstance<T>();
        //}
    }
}
