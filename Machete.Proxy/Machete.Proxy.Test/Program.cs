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
            TestProxy();
            //TestProxy1();
        }


        public static void TestProxy1()
        {

            DemoInvocation invocation = new DemoInvocation();
            ProxyInterfaceFatory<IUserDao> proxyInterfaceFatory = new ProxyInterfaceFatory<IUserDao>();
            IUserDao userDao = proxyInterfaceFatory.Build(invocation);

            //  int ret = userDao.Delete(1);
            userDao.Show("name");
            // userDao.Get("asd");
        }

        public static void TestProxy()
        {
            IIntercept intercept = new UserIntercept();
            var proxyObject = new UserDao();
            var factory = new ProxyTypeFatory<IUserDao>();
            IUserDao userDao = factory.Build(proxyObject, intercept);

            string ret1 = userDao.Get("张三");
            Console.WriteLine("------------------------------------------------------------");
            int ret2 = userDao.Delete(1);
            Console.WriteLine("------------------------------------------------------------");
            userDao.Show("张三");
            Console.WriteLine("------------------------------------------------------------");
            userDao.Update("张三", 2);
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


        protected void ImplementCtor<T>(TypeBuilder builder)
        {

            var baseType = typeof(T);
            Type parentType = typeof(ProxyInterfaceBase);
            var invocationField = parentType
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == "_invocation");

            //var proxyTypeField = parentType
            //    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            //    .FirstOrDefault(x => x.Name == "_proxyType");

            var cbuilder = builder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard | CallingConventions.HasThis
                , new Type[] { baseType });

            var gen = cbuilder.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0); //load this for base type constructor
            gen.Emit(OpCodes.Call, typeof(object).GetConstructors().Single());


            gen.Emit(OpCodes.Ldarg_0); //load load this
            gen.Emit(OpCodes.Ldarg_1); //load baseType object
            gen.Emit(OpCodes.Stfld, invocationField);

            //gen.Emit(OpCodes.Ldstr, baseType.FullName);
            //gen.Emit(OpCodes.Ldarg_1); //load baseType object
            //gen.Emit(OpCodes.Stfld, proxyTypeField);

            gen.Emit(OpCodes.Ret);

        }
    }
}
