using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public interface IInvocation
    {
        object Invoke(string type, string method, params object[] args);
    }
}
