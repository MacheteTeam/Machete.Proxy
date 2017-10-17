using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public class ProxyTypeFatory<T>
    {
        private ProxyTypeGenerator _proxyTypeGenerator;

        private Type _cachedType;


        public ProxyTypeFatory()
        {
            _proxyTypeGenerator = new ProxyTypeGenerator();
        }

        public T Build(T proxyObject, IIntercept intercept)
        {
            string key = typeof(T).FullName;
            ProxyTypeCache typeCache = new ProxyTypeCache();
            if (typeCache.Contains(key))
            {
                _cachedType = typeCache.Get(key);
            }
            else
            {
                _cachedType = _proxyTypeGenerator.Build<T>();
                typeCache.Store(key, _cachedType);
            }

            var proxy = (T)Activator.CreateInstance(_cachedType, proxyObject);

            ProxyTypeBase<T> proxyBase = proxy as ProxyTypeBase<T>;
            proxyBase.Intercept = intercept;

            return proxy;
        }
    }
}
