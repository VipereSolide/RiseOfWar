﻿using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace RiseOfWar
{
    public static class BehaviourHelper
    {
        public static T GetOrCreateComponent<T>(this Behaviour behaviour) where T : Component
        {
            bool _hasComponent = behaviour.TryGetComponent(out T _output);

            if (_hasComponent == false)
            {
                _output = behaviour.gameObject.AddComponent<T>();
            }

            return _output;
        }

        public static T GetProperty<T>(this MonoBehaviour _instance, string _propertyName)
        {
            return (T)Traverse.Create(_instance).Field(_propertyName).GetValue();
        }

        public static void SetProperty<T>(this MonoBehaviour _instance, string _propertyName, T _value)
        {
            Traverse.Create(_instance).Field(_propertyName).SetValue(_value);
        }

        public static T GetStaticProperty<T>(string _propertyName)
        {
            return (T)Traverse.Create(typeof(T)).Field(_propertyName).GetValue();
        }

        public static void SetStaticProperty<T>(string _propertyName, T _value)
        {
            Traverse.Create(typeof(T)).Field(_propertyName).SetValue(_value);
        }

        public static void CallPrivateMethod(this MonoBehaviour _instance, string _methodName, object[] _arguments)
        {
            MethodInfo _privateMethod = _instance.GetType().GetMethod(_methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            _privateMethod.Invoke(_instance, _arguments);
        }

        public static void CallPrivateMethod(this MonoBehaviour _instance, string _methodName)
        {
            CallPrivateMethod(_instance, _methodName, new object[] { });
        }
    }
}