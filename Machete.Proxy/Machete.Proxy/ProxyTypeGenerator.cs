using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public class ProxyTypeGenerator
    {

        /// <summary>
        /// build dynamic assembly
        /// </summary>
        /// <returns></returns>
        private AssemblyBuilder GetAssemblyBuilder()
        {
            string assemblyName = $"smallcode.{Guid.NewGuid().ToString().Replace("-", "")}";
            AssemblyName name = new AssemblyName(assemblyName);
            var builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            return builder;
        }

        /// <summary>
        /// build module
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private ModuleBuilder BuildModule(AssemblyBuilder builder)
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
        private TypeBuilder BuildType<T>(ModuleBuilder builder)
        {
            Type baseType = typeof(T);
            Type parentType = typeof(ProxyTypeBase<T>);
            string typeName = $"smallcode.{baseType.Name}";
            Type[] interfaces = new Type[] { baseType };
            TypeBuilder typeBuilder = builder.DefineType(typeName,
                TypeAttributes.Public | TypeAttributes.Class,
                parentType, interfaces);
            return typeBuilder;
        }

        void ImplementCtor<T>(TypeBuilder builder)
        {
            var baseType = typeof(T);
            Type parentType = typeof(ProxyTypeBase<T>);
            var _proxyObjectField = parentType
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == "_proxyObject");

            var cbuilder = builder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard | CallingConventions.HasThis
                , new Type[] { baseType });

            var gen = cbuilder.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0); //load load this
            gen.Emit(OpCodes.Ldarg_1); //load baseType object
            gen.Emit(OpCodes.Stfld, _proxyObjectField);
            gen.Emit(OpCodes.Ret);

        }

        /// <summary>
        /// if all methods implement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        bool ImplementMethods<T>(TypeBuilder builder)
        {
            var baseType = typeof(T);
            Type parentType = typeof(ProxyTypeBase<T>);
            var methods = baseType.GetMethods();
            foreach (var method in methods)
            {
                var methodBuilder = DefineMethod(builder, method);
                ProxyMethod(methodBuilder, method, parentType);
                builder.DefineMethodOverride(methodBuilder, method);
            }
            return true;
        }

        private MethodBuilder DefineMethod(TypeBuilder builder, MethodInfo info)
        {
            var returnType = info.ReturnType;
            var paramInfoList = info.GetParameters();
            var paramTypeList = paramInfoList.Select(p => p.ParameterType).ToArray();

            var methodBuilder = builder.DefineMethod(info.Name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                info.CallingConvention, returnType, paramTypeList);
            return methodBuilder;
        }

        private void ProxyMethod(MethodBuilder method, MethodInfo proxyMethod, Type parentType)
        {
            var proxyObjectField = parentType
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == "_proxyObject");

            // parentType intercept  property get method
            var getInterceptMethod = parentType.GetMethods()
                .FirstOrDefault(x => x.Name == "get_Intercept");

            var gen = method.GetILGenerator();

            //invoke  intercept  Invoke
            MethodInfo beginInvoke =
                typeof(IIntercept).GetMethod("BeginInvoke", new Type[] { typeof(string), typeof(object[]) });
            MethodInfo endInvoke = typeof(IIntercept).GetMethod("EndInvoke", new Type[] { typeof(object) });
            MethodInfo exceptionInvoke = typeof(IIntercept).GetMethod("OnException", new Type[] { typeof(Exception) });

            ParameterInfo[] args = proxyMethod.GetParameters();


            var paramVar = gen.DeclareLocal(typeof(object[]));
            var methodVar = gen.DeclareLocal(typeof(string));
            var retVar = gen.DeclareLocal(typeof(object));
            var localException = gen.DeclareLocal(typeof(Exception));

            //define parameters
            gen.Emit(OpCodes.Ldc_I4, 2);
            gen.Emit(OpCodes.Newarr, typeof(object));
            gen.Emit(OpCodes.Stloc_0, paramVar);

            for (int i = 0; i < args.Length; i++)
            {
                gen.Emit(OpCodes.Ldloc_0, paramVar);
                gen.Emit(OpCodes.Ldc_I4, i);
                gen.Emit(OpCodes.Ldarg, (1 + i));
                gen.Emit(OpCodes.Box, args[i].ParameterType);
                gen.Emit(OpCodes.Stelem_Ref);
            }

            //define method name
            gen.Emit(OpCodes.Ldstr, proxyMethod.Name);
            gen.Emit(OpCodes.Stloc_1, methodVar);

            //invoke beginInvoke
            gen.Emit(OpCodes.Ldarg_0); //load "this" onto stack
            gen.Emit(OpCodes.Call, getInterceptMethod); // this.get_Intercept;
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ldloc_0);
            gen.EmitCall(OpCodes.Callvirt, beginInvoke, null); // this.get_Intercept.Invoke();

            gen.BeginExceptionBlock(); //try{

            //invoke proxyMethod
            gen.Emit(OpCodes.Ldarg_0); //load "this" onto stack
            gen.Emit(OpCodes.Ldfld, proxyObjectField); // this._proxtObject
            var parms = proxyMethod.GetParameters();
            int pIndex = 1;
            foreach (var parm in parms)
            {
                gen.Emit(OpCodes.Ldarg, pIndex++);
            }
            gen.Emit(OpCodes.Callvirt, proxyMethod); //_proxtObject.proxyMethod()


            // if has return value set Stloc_2
            if (proxyMethod.ReturnType != typeof(void))
            {
                gen.Emit(OpCodes.Stloc_2, retVar);
            }

            gen.BeginCatchBlock(typeof(Exception)); // } catch{

            //invoke onexception
            gen.Emit(OpCodes.Stloc_3, localException);
            gen.Emit(OpCodes.Ldarg_0); //load "this" onto stack
            gen.Emit(OpCodes.Call, getInterceptMethod); // this.get_Intercept;
            gen.Emit(OpCodes.Ldloc_3);
            gen.EmitCall(OpCodes.Callvirt, exceptionInvoke, null);

            gen.EndExceptionBlock();

            if (proxyMethod.ReturnType == typeof(void))
            {
                //no return value 
                // invoke endinvoke
                gen.Emit(OpCodes.Ldarg_0); //load "this" onto stack
                gen.Emit(OpCodes.Call, getInterceptMethod); // this.get_Intercept;
                gen.Emit(OpCodes.Ldnull);
                gen.EmitCall(OpCodes.Callvirt, endInvoke, null); // this.get_Intercept.Invoke();
                gen.Emit(OpCodes.Ret);
            }
            else
            {
                // invoke endinvoke
                gen.Emit(OpCodes.Ldarg_0); //load "this" onto stack
                gen.Emit(OpCodes.Call, getInterceptMethod); // this.get_Intercept;
                gen.Emit(OpCodes.Ldloc_2);
                gen.Emit(OpCodes.Box, proxyMethod.ReturnType);
                gen.EmitCall(OpCodes.Callvirt, endInvoke, null); // this.get_Intercept.Invoke();

                //return 
                gen.Emit(OpCodes.Ldloc_2);
                gen.Emit(OpCodes.Ret);
            }

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

