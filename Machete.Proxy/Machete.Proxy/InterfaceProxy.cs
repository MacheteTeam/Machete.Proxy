using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Machete.Proxy
{
    /// <summary>
    /// 动态接口代理
    /// </summary>
    public partial class InterfaceProxy
    {
        private class Map
        {
            public Type New
            {
                get;
                set;
            }

            public Type Org
            {
                get;
                set;
            }
        }

        private static IList<Map> maps = null;

        public static T New<T>(IIntercept interceptor) where T : class
        {
            object value = New<T>(typeof(T), interceptor);
            if (value == null)
            {
                return null;
            }
            return (T)value;
        }

        public static object New<T>(Type clazz, IIntercept hanlder)
        {
            if (clazz == null || !clazz.IsInterface)
            {
                throw new ArgumentException("clazz");
            }
            if (hanlder == null)
            {
                throw new ArgumentException("hanlder");
            }
            lock (maps)
            {
                Type type = GetType(clazz);
                if (type == null)
                {
                    type = CreateType<T>(clazz);
                    maps.Add(new Map() { New = type, Org = clazz });
                }
                return Activator.CreateInstance(type, hanlder);
            }
        }
    }

    public partial class InterfaceProxy
    {
        private const MethodAttributes METHOD_ATTRIBUTES = MethodAttributes.Public | MethodAttributes.NewSlot |
                                                           MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig;

        private const TypeAttributes TYPE_ATTRIBUTES = TypeAttributes.Public | TypeAttributes.Sealed |
                                                       TypeAttributes.Serializable;

        private const FieldAttributes FIELD_ATTRIBUTES = FieldAttributes.Private;

        private const CallingConventions CALLING_CONVENTIONS = CallingConventions.HasThis;

        private const PropertyAttributes PROPERTY_ATTRIBUTES = PropertyAttributes.SpecialName;

        private static ModuleBuilder MODULE_BUILDER = null;

        static InterfaceProxy()
        {
            maps = new List<Map>();
            AssemblyName an = new AssemblyName("?");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
            MODULE_BUILDER = ab.DefineDynamicModule(an.Name);
        }

        private static Type GetType(Type clazz)
        {
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (map.Org == clazz)
                {
                    return map.New;
                }
            }
            return null;
        }

        private static void CreateConstructor(TypeBuilder tb, FieldBuilder fb)
        {
            Type[] args = new Type[] { typeof(IIntercept) };
            ConstructorBuilder ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, args);
            ILGenerator il = ctor.GetILGenerator();
            //
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fb);
            il.Emit(OpCodes.Ret);
        }

        private static FieldBuilder CreateField(TypeBuilder tb)
        {
            return tb.DefineField("handler", typeof(IIntercept), FIELD_ATTRIBUTES);
        }

        private static Type CreateType<T>(Type clazz)
        {
            TypeBuilder tb = MODULE_BUILDER.DefineType(string.Format("{0}.{1}", typeof(InterfaceProxy).FullName, clazz.Name));
            tb.AddInterfaceImplementation(clazz);
            //
            FieldBuilder fb = CreateField(tb);
            //
            CreateConstructor(tb, fb);
            CreateMethods<T>(clazz, tb, fb);
            CreateProperties<T>(clazz, tb, fb);
            //
            return tb.CreateType();
        }

        private static void CreateMethods<T>(Type clazz, TypeBuilder tb, FieldBuilder fb)
        {
            foreach (MethodInfo met in clazz.GetMethods())
            {
                CreateMethod<T>(met, tb, fb);
            }
        }

        private static Type[] GetParameters(ParameterInfo[] pis)
        {
            Type[] buffer = new Type[pis.Length];
            for (int i = 0; i < pis.Length; i++)
            {
                buffer[i] = pis[i].ParameterType;
            }
            return buffer;
        }

        private static MethodBuilder CreateMethod<T>(MethodInfo met, TypeBuilder tb, FieldBuilder fb)
        {
            ParameterInfo[] args = met.GetParameters();
            MethodBuilder mb = tb.DefineMethod(met.Name, InterfaceProxy.METHOD_ATTRIBUTES, met.ReturnType, GetParameters(args));
            ILGenerator il = mb.GetILGenerator();
            il.DeclareLocal(typeof(object[]));

            if (met.ReturnType != typeof(void))
            {
                il.DeclareLocal(met.ReturnType);
            }

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldc_I4, args.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc_0);

            for (int i = 0; i < args.Length; i++)
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, (1 + i));
                il.Emit(OpCodes.Box, args[i].ParameterType);
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fb);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, met.MetadataToken);
            il.Emit(OpCodes.Ldstr, met.DeclaringType?.FullName + "+" + met.Name);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Call, typeof(IIntercept).GetMethod("InvokeMember", BindingFlags.Instance | BindingFlags.Public));

            if (met.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Pop);
            }
            else
            {
                il.Emit(OpCodes.Unbox_Any, met.ReturnType);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldloc_1);
            }
            il.Emit(OpCodes.Ret);
            //
            return mb;
        }

        private static void CreateProperties<T>(Type clazz, TypeBuilder tb, FieldBuilder fb)
        {
            foreach (PropertyInfo prop in clazz.GetProperties())
            {
                PropertyBuilder pb = tb.DefineProperty(prop.Name, PROPERTY_ATTRIBUTES, prop.PropertyType, Type.EmptyTypes);
                MethodInfo met = prop.GetGetMethod();
                if (met != null)
                {
                    MethodBuilder mb = CreateMethod<T>(met, tb, fb);
                    pb.SetGetMethod(mb);
                }
                met = prop.GetSetMethod();
                if (met != null)
                {
                    MethodBuilder mb = CreateMethod<T>(met, tb, fb);
                    pb.SetSetMethod(mb);
                }
            }
        }
    }
}
