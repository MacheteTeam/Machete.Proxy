using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public class ProxyInterfaceFatory<T>
    {
        private ProxyInterfaceGenerator _proxyInterfaceGenerator;

        private Type _cachedType;


        public ProxyInterfaceFatory()
        {
            _proxyInterfaceGenerator = new ProxyInterfaceGenerator();
        }

        public T Build(IInvocation invocation)
        {
            string key = typeof(T).FullName;
            ProxyTypeCache typeCache = new ProxyTypeCache();
            if (typeCache.Contains(key))
            {
                _cachedType = typeCache.Get(key);
            }
            else
            {
                _cachedType = _proxyInterfaceGenerator.Build<T>();
                typeCache.Store(key, _cachedType);
            }

            var proxy = (T)Activator.CreateInstance(_cachedType, invocation);

            ProxyInterfaceBase proxyBase = proxy as ProxyInterfaceBase;
            proxyBase.Invocation = invocation;
            return proxy;
        }
    }
}
