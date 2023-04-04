using System;
using System.Collections.Generic;
using UnityEngine;

namespace RiseOfWar
{
    public class ActorAdditionalData
    {
        public Actor target;

        public float lastTimeHit;
        public float lastTimeDead;
        public int score;

        public bool isCaptureXPLoopStarted;
        public float nextTimeForCaptureXP;

        private List<RegisteredDamageSource> _registeredDamageSources = new List<RegisteredDamageSource>();
        public RegisteredDamageSource[] registeredDamagesSources
        {
            get { return _registeredDamageSources.ToArray(); }
        }

        public ActorAdditionalData(Actor target)
        {
            this.target = target;
        }

        public void AddScore(int score)
        {
            this.score += score;
        }

        public void AddScore(string killfeedMessage, int score)
        {
            this.score += score;

            if (target == ReferenceManager.player)
            {
                KillfeedManager.Instance.AddKillfeedItem(killfeedMessage, score);
            }
        }

        public bool CanGetDamaged()
        {
            Plugin.Log($"ActorAdditionalData: Actor last hit at time {lastTimeHit}. (current time {Time.time})");
            return Time.time > lastTimeHit + GameConfiguration.actorDamageInvulnerabilityTime;
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