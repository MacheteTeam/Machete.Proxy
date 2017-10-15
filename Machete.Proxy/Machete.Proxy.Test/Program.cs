using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy.Test
{

    class Program
    {
        static void Main(string[] args)
        {
            IIntercept intercept = new UserIntercept();
            var proxyObject = new UserDao();
            var factory = new AutoProxyFatory<IUserDao>();
            IUserDao userDao = factory.Build(proxyObject, intercept);
            //plugin.TestMethod2("Zhangsan", 42);
            string ret1 = userDao.Get("nele");
            int ret2 = userDao.Delete(1);
            userDao.Show("nele");
            userDao.Update("nele", 2);
            // Test();
        }





        public static void Test()
        {
            AssemblyName assemblyName = new AssemblyName("Study");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("StudyModule", "StudyOpCodes.dll");
            TypeBuilder typeBuilder = moduleBuilder.DefineType("StudyOpCodes", TypeAttributes.Public);
            ConstructorBuilder ctorMethod = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(int), typeof(string) });

            ILGenerator il = ctorMethod.GetILGenerator();

            MethodInfo consoleWriteLineInfo = typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(string) });
            //il.Emit(OpCodes.Ldstr, "Proxy Hello");
            //il.Emit(OpCodes.Call, consoleWriteLineInfo);

            var localException = il.DeclareLocal(typeof(Exception));

            var retVar = il.DeclareLocal(typeof(object));

            il.BeginExceptionBlock(); //try{
            //非静态方法参数索引从1开始
            il.Emit(OpCodes.Ldstr, "姓名：{1} 年龄：{0}");
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(int), typeof(string) }));

            il.Emit(OpCodes.Stloc_0, retVar);

            il.Emit(OpCodes.Ldloc_0, retVar);
            il.Emit(OpCodes.Call, consoleWriteLineInfo);

            il.BeginCatchBlock(typeof(Exception)); // } catch{
            il.Emit(OpCodes.Stloc_1, localException);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Call, consoleWriteLineInfo);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ret);
            Type t = typeBuilder.CreateType();

            assemblyBuilder.Save("StudyOpCodes.dll");
        }
    }
}
