using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public class AutoProxyFatory<T>
    {
        private ProxyTypeGenerator _proxyTypeGenerator;

        private Type _cachedType;


        public AutoProxyFatory()
        {
            _proxyTypeGenerator = new ProxyTypeGenerator();
        }

        public T Build(T proxyObject, IIntercept intercept)
        {
            if (_cachedType == null)
            {
                _cachedType = _proxyTypeGenerator.Build<T>();
            }
            var proxy = (T)Activator.CreateInstance(_cachedType, proxyObject);

            ProxyTypeBase<T> proxyBase = proxy as ProxyTypeBase<T>;

            proxyBase.Intercept = intercept;

            return proxy;
        }
    }
}
