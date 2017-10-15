using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy.Test
{
    public interface IUserDao
    {
        int Delete(int id);

        string Get(string name);

        void Show(string name);

        int Update(string name, int id);
    }
}
