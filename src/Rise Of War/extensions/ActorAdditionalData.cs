using UnityEngine;
using System;
using System.Collections.Generic;

namespace RiseOfWar
{
    public class ActorAdditionalData
    {
        public float lastTimeHit;
        public float lastTimeDead;
        public int xp;

        private List<RegisteredDamageSource> _registeredDamageSources = new List<RegisteredDamageSource>();  
        public RegisteredDamageSource[] registeredDamagesSources
        {
            get { return _registeredDamageSources.ToArray(); }
        }

        public bool CanGetDamaged()
        {
            Plugin.Log("ActorAdditionalData: Actor was last time hit at = " + lastTimeHit + "; Current time = " + Time.time);
            return Time.time > lastTimeHit + 0.05f;
        }

        public void AddDamageSource(RegisteredDamageSource _source)
        {
            _registeredDamageSources.Add(_source);
        }
        
        public void RemoveDamageSource(RegisteredDamageSource _source)
        {
            _registeredDamageSources.Remove(_source);
        }
        
        public void RemoveOldestDamageSource()
        {
            _registeredDamageSources.RemoveAt(_registeredDamageSources.Count - 1);
        }

        public ActorAdditionalData()
        {

        }

        [Serializable]
        public class RegisteredDamageSource
        {
            public Actor source;
            public float time;

            public RegisteredDamageSource(Actor _source, float _time)
            {
                source = _source;
                time = _time;
            }
        }
    }
}