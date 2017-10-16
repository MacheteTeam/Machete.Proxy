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
            string key = typeof(T).FullName;
            ProxyCache cache = new ProxyCache();
            if (cache.Contains(key))
            {
                _cachedType = cache.Get(key);
            }
            else
            {
                _cachedType = _proxyTypeGenerator.Build<T>();
                cache.Store(key, _cachedType);
            }

            var proxy = (T)Activator.CreateInstance(_cachedType, proxyObject);

            ProxyTypeBase<T> proxyBase = proxy as ProxyTypeBase<T>;
            proxyBase.Intercept = intercept;

            return proxy;
        }
    }
}
