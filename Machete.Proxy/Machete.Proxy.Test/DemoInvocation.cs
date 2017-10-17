using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy.Test
{
    public class DemoInvocation : IInvocation
    {
        public object Invoke(string type, string method, params object[] args)
        {
            Console.WriteLine($"type:{type};method:{method}");

            return default(object);
        }
    }
}
