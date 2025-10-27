using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine.Events;

namespace COM3D2.SimpleUI.Extensions
{
    public static class IControlValueExtensions
    {
        public static R Bind<R, S, T>(this R obj, S target, Expression<Func<S, T>> propExpr) where R : IControlValue<T>
        {
            var expr = (MemberExpression)propExpr.Body;
            var prop = (PropertyInfo)expr.Member;

            obj.AddChangeCallback(value =>
            {
                prop.SetValue(target, value, null);
            });

            return obj;
        }

        public static R Bind<R, S, T>(this R obj, S target, string propName) where R : IControlValue<T>
        {
            var prop = target.GetType().GetProperty(propName);
            if (prop.PropertyType.IsAssignableFrom(typeof(T)))
            {
                throw new Exception($"Unassignable type {typeof(T)} => {prop.PropertyType}");
            }

            obj.AddChangeCallback(value =>
            {
                prop.SetValue(target, value, null);
            });

            return obj;
        }

        public static R OnChange<R>(this R obj, UnityAction<bool> action)
            where R : IBoolControlValue
        {
            obj.AddChangeCallback(action);
            return obj;
        }

        public static R OnChange<R>(this R obj, UnityAction<string> action)
            where R : IStringControlValue
        {
            obj.AddChangeCallback(action);
            return obj;
        }

        public static R OnChange<R>(this R obj, UnityAction<float> action)
            where R : IFloatControlValue
        {
            obj.AddChangeCallback(action);
            return obj;
        }

        public static R OnChange<R>(this R obj, UnityAction<int> action)
            where R : IIntControlValue
        {
            obj.AddChangeCallback(action);
            return obj;
        }
    }
}