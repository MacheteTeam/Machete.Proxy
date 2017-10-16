using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    /// <summary>
    /// Cache
    /// </summary>
    public class ProxyCache
    {
        private static readonly Dictionary<string, Type> _cache =
            new Dictionary<string, Type>();

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            Type type = null;
            return _cache.TryGetValue(key, out type);
        }

        /// <summary>
        /// Get By Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Type Get(string key)
        {
            Type type = null;
            _cache.TryGetValue(key, out type);
            return type;
        }


        /// <summary>
        /// Store
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        public void Store(string key, Type type)
        {
            _cache.Add(key, type);
        }


    }
}
