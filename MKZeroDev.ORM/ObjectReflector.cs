using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MKZeroDev.ORM
{
    public static class ObjectReflector
    {
        public static object? CreateInstance(Type type, object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        public static object? CreateGenericInstance(Type type, Type[] typeArgs, object[] args)
        {
            var genericType = type.MakeGenericType(typeArgs);
            return Activator.CreateInstance(genericType, args);
        }

        public static object? CallGenericMethod(Type type, object obj, string methodName, Type[] typeArgs, Type[] paramTypeArgs, object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, paramTypeArgs, null);
            var genericMethod = method?.MakeGenericMethod(typeArgs);
            return genericMethod?.Invoke(obj, parameters);
        }

        public static object? CallGenericStaticMethod(Type type, string methodName, Type[] typeArgs, Type[] paramTypeArgs, object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, paramTypeArgs, null);
            var genericMethod = method?.MakeGenericMethod(typeArgs);
            return genericMethod?.Invoke(null, parameters);
        }

        public static object? CallMethod(Type type, object obj, string methodName, Type[] paramTypeArgs, object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, paramTypeArgs, null);
            return method?.Invoke(obj, parameters);
        }

        public static object? CallStaticMethod(Type type, string methodName, Type[] paramTypeArgs, object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, paramTypeArgs, null);
            return method?.Invoke(null, parameters);
        }
    }
}
