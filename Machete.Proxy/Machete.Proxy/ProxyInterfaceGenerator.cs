using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    public class ProxyInterfaceGenerator : ProxyGeneratorBase
    {
        /// <summary>
        /// build type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected override TypeBuilder BuildType<T>(ModuleBuilder builder)
        {
            Type baseType = typeof(T);
            Type parentType = typeof(ProxyInterfaceBase);
            string typeName = $"smallcode.{baseType.Name}";
            Type[] interfaces = new Type[] { baseType };
            TypeBuilder typeBuilder = builder.DefineType(typeName,
                TypeAttributes.Public | TypeAttributes.Class,
                parentType, interfaces);
            return typeBuilder;
        }

        protected override void ImplementCtor<T>(TypeBuilder builder)
        {

            var baseType = typeof(T);
            Type parentType = typeof(ProxyInterfaceBase);

            var proxyTypeField = parentType
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == "_proxyType");

            var cbuilder = builder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard | CallingConventions.HasThis
                , new Type[] { typeof(IInvocation) });

            var gen = cbuilder.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0); //load this for base type constructor
            gen.Emit(OpCodes.Call, typeof(object).GetConstructors().Single());

            gen.Emit(OpCodes.Ldarg_0); //load baseType object
            gen.Emit(OpCodes.Ldstr, baseType.FullName);
            gen.Emit(OpCodes.Stfld, proxyTypeField);

            gen.Emit(OpCodes.Ret);

        }

        protected override void ProxyMethod(MethodBuilder method, MethodInfo proxyMethod, Type parentType)
        {

            var getInvocationMethod = parentType.GetMethods()
                .FirstOrDefault(x => x.Name == "get_Invocation");

            var proxyType = parentType
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == "_proxyType");

            var gen = method.GetILGenerator();

            //invoke  intercept  Invoke
            MethodInfo invoke =
                typeof(IInvocation).GetMethod("Invoke", new Type[] { typeof(string), typeof(string), typeof(object[]) });


            ParameterInfo[] args = proxyMethod.GetParameters();


            var paramVar = gen.DeclareLocal(typeof(object[]));
            var methodVar = gen.DeclareLocal(typeof(string));
            var typeVar = gen.DeclareLocal(typeof(string));


            //define parameters
            gen.Emit(OpCodes.Ldc_I4, args.Length);
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


            gen.Emit(OpCodes.Ldarg_0); //load "this" onto stack
            gen.Emit(OpCodes.Ldfld, proxyType); // this._proxtType
            gen.Emit(OpCodes.Stloc_2, typeVar);

            gen.Emit(OpCodes.Ldarg_0); //load "this" onto stack
            gen.Emit(OpCodes.Call, getInvocationMethod); // this.get_Invocation;

            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ldloc_0);
            gen.EmitCall(OpCodes.Callvirt, invoke, null); // this.get_Intercept.Invoke();

            if (proxyMethod.ReturnType == typeof(void))
            {
                gen.Emit(OpCodes.Pop);
            }
            else
            {
                gen.Emit(OpCodes.Unbox_Any, proxyMethod.ReturnType);
                gen.Emit(OpCodes.Stloc_1);
                gen.Emit(OpCodes.Ldloc_1);
            }
            gen.Emit(OpCodes.Ret);
        }


        protected override bool ImplementMethods<T>(TypeBuilder builder)
        {
            var baseType = typeof(T);
            Type parentType = typeof(ProxyInterfaceBase);
            var methods = baseType.GetMethods();
            foreach (var method in methods)
            {
                var methodBuilder = DefineMethod(builder, method);
                ProxyMethod(methodBuilder, method, parentType);
                builder.DefineMethodOverride(methodBuilder, method);
            }
            return true;
        }
    }
}
