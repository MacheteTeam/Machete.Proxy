using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy.Test
{
    public class UserIntercept : IIntercept
    {
        public void BeginInvoke(string method, params object[] parameters)
        {
            Console.WriteLine("BeginInvoke method:" + method);
        }

        public void EndInvoke(object retVal)
        {
            if (retVal != null)
            {
                Console.WriteLine("EndInvoke  return value:" + retVal.ToString());
            }
            else
            {
                Console.WriteLine("no return value");
            }
        }

        public void OnException(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
}
