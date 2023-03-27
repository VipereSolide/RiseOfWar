using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RiseOfWar
{
    public static class ActorExtensions
    {
        public static T GetProperty<T>(this Actor _instance, string _propertyName)
        {
            return (T)Traverse.Create(_instance).Field(_propertyName).GetValue();
        }

        public static void SetProperty<T>(this Actor _instance, string _propertyName, T _value)
        {
            Traverse.Create(_instance).Field(_propertyName).SetValue(_value);
        }

        public static void CallPrivateMethod(this Actor _instance, string _methodName, object[] _arguments)
        {
            MethodInfo _privateMethod = _instance.GetType().GetMethod(_methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            _privateMethod.Invoke(_instance, _arguments);
        }

        public static void CallPrivateMethod(this Actor _instance, string _methodName)
        {
            CallPrivateMethod(_instance, _methodName, new object[] { });
        }

        private static readonly ConditionalWeakTable<Actor, ActorAdditionalData> _data = new ConditionalWeakTable<Actor, ActorAdditionalData>();

        public static ActorAdditionalData GetAdditionalData(this Actor _actor)
        {
            return _data.GetOrCreateValue(_actor);
        }

        public static void AddData(this Actor _actor, ActorAdditionalData _value)
        {
            try
            {
                _data.Add(_actor, _value);
            }
            catch (Exception)
            {
            }
        }
    }
}