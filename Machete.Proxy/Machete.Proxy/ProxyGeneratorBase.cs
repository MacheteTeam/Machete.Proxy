using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public class ProxyGeneratorBase
    {
        /// <summary>
        /// build dynamic assembly
        /// </summary>
        /// <returns></returns>
        protected AssemblyBuilder GetAssemblyBuilder()
        {
            string assemblyName = $"smallcode.{Guid.NewGuid().ToString().Replace("-", "")}";
            AssemblyName name = new AssemblyName(assemblyName);
            var builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            return builder;
        }

        /// <summary>
        /// build module
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected ModuleBuilder BuildModule(AssemblyBuilder builder)
        {
            string moduleName = $"smallcode.{Guid.NewGuid().ToString().Replace("-", "")}";
            var module = builder.DefineDynamicModule(moduleName);
            return module;
        }

        /// <summary>
        /// build type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected virtual TypeBuilder BuildType<T>(ModuleBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected virtual void ImplementCtor<T>(TypeBuilder builder)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// if all methods implement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected virtual bool ImplementMethods<T>(TypeBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected MethodBuilder DefineMethod(TypeBuilder builder, MethodInfo info)
        {
            var returnType = info.ReturnType;
            var paramInfoList = info.GetParameters();
            var paramTypeList = paramInfoList.Select(p => p.ParameterType).ToArray();

            var methodBuilder = builder.DefineMethod(info.Name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                info.CallingConvention, returnType, paramTypeList);
            return methodBuilder;
        }

        /// <summary>
        /// generate proxy method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="proxyMethod"></param>
        /// <param name="parentType"></param>
        protected virtual void ProxyMethod(MethodBuilder method, MethodInfo proxyMethod, Type parentType)
        {

        }



        public Type Build<T>()
        {
            var builder = GetAssemblyBuilder();
            var module = BuildModule(builder);
            var typeBuilder = BuildType<T>(module);

            ImplementCtor<T>(typeBuilder);

            if (!ImplementMethods<T>(typeBuilder))
            {
                throw new InvalidOperationException("Can't proxy methods");
            }
            return typeBuilder.CreateType();
        }
    }
}
