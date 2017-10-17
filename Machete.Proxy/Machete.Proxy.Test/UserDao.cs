using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy.Test
{
    public class UserDao : IUserDao
    {
        public int Delete(int id)
        {
            return id;
        }

        public string Get(string name)
        {
            return "name :" + name;
        }

        public void Show(string name)
        {
            Console.WriteLine("name :" + name);
        }

        public int Update(string name, int id)
        {
            throw new Exception("update exception");
        }
    }
}
