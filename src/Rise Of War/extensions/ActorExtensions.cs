using System.Runtime.CompilerServices;
using System.Reflection;
using System;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    public static class ActorExtensions
    {
        public static void AddScore(this Actor instance, int score)
        {
            instance.GetAdditionalData().AddScore(score);
        }

        public static void AddScoreWithMessage(this Actor instance, string killfeedMessage, int score)
        {
            instance.GetAdditionalData().AddScore(killfeedMessage, score);
        }

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

        public static bool IsInsideCapturePoint(this Actor actor)
        {
            return actor.GetCurrentCapturePoint() != null;
        }

        public static bool IsCaptureXPLoopStarted(this Actor actor)
        {
            return actor.GetAdditionalData().isCaptureXPLoopStarted;
        }

        public static void UpdateInsideFlag(this Actor actor)
        {
            if (actor.IsInsideCapturePoint() == false)
            {
                if (actor.IsCaptureXPLoopStarted())
                {
                    actor.StopCaptureXPLoop();
                }

                return;
            }

            CapturePoint _capturePoint = actor.GetCurrentCapturePoint();
            int _team = actor.team;

            // If the said actor is on a point that is not theirs, it
            // means they're capturing it, so we need to regularly give
            // XP to the actor.
            if (_capturePoint.owner != _team)
            {
                actor.StartCaptureXPLoop();
            }
        }

        public static void StartCaptureXPLoop(this Actor actor)
        {
            ActorAdditionalData _additionalData = actor.GetAdditionalData();
            _additionalData.isCaptureXPLoopStarted = true;

            if (Time.time >= _additionalData.nextTimeForCaptureXP)
            {
                _additionalData.nextTimeForCaptureXP = Time.time + 2;
                actor.AddScoreWithMessage($"Capture point support <#{GameConfiguration.WHITE_COLOR}>+2</color>", 1);
            }
        }

        public static void StopCaptureXPLoop(this Actor actor)
        {
            ActorAdditionalData _additionalData = actor.GetAdditionalData();
            _additionalData.isCaptureXPLoopStarted = false;
        }
    }
}