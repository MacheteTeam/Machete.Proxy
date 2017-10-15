using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public interface IIntercept
    {
        /// <summary>
        /// before method 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        void BeginInvoke(string method, params object[] parameters);

        /// <summary>
        /// after method
        /// </summary>
        /// <param name="retVal"></param>
        void EndInvoke(object retVal);

        /// <summary>
        /// occur excepption
        /// </summary>
        /// <param name="obj"></param>
        void OnException(Exception exception);
    }
}
