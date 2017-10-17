using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy.Test
{
    public class UserIntercept : IIntercept
    {
        /// <summary>
        /// 方法执行之前
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        public void BeginInvoke(string method, params object[] parameters)
        {
            Console.WriteLine("BeginInvoke method:" + method);
        }

        /// <summary>
        /// 方法执行之后
        /// </summary>
        /// <param name="retVal"></param>
        public void EndInvoke(object retVal)
        {
            if (retVal != null)
            {
                Console.WriteLine("EndInvoke  return value:" + retVal);
            }
            else
            {
                Console.WriteLine("EndInvoke  no return value");
            }
        }

        /// <summary>
        /// 执行方法出异常
        /// </summary>
        /// <param name="exception"></param>
        public void OnException(Exception exception)
        {
            Console.WriteLine("OnException " + exception.Message);
        }
    }
}
