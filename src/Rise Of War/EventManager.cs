using System;
using UnityEngine;

namespace RiseOfWar.Events
{
    public static class EventManager
    {
        public static Action<OnActorHitEvent> onActorHit;
        public static Action<OnActorDieEvent> onActorDie;
        public static Action<OnPlayerDealtDamageEvent> onPlayerDealtDamage;
        public static Action<OnProjectileHitHitboxEvent> onProjectileHitHitbox;
    }

    [Serializable]
    public class OnProjectileHitHitboxEvent
    {
        public Projectile projectile { get; set; }
        public Vector3 hitPosition { get; set; }
        public Hitbox hitbox { get; set; }

        public OnProjectileHitHitboxEvent(Projectile _projectile, Vector3 _position, Hitbox _hitbox)
        {
            projectile = _projectile;
            hitPosition = _position;
            hitbox = _hitbox;
        }
    }

    [Serializable]
    public class OnActorDieEvent
    {
        public DamageInfo damage { get; private set; }
        public bool isSilentKill { get; private set; }
        public Actor victim { get; private set; }

        public OnActorDieEvent(DamageInfo _damage, Actor _victim, bool _isSilentKill)
        {
            damage = _damage;
            isSilentKill = _isSilentKill;
            victim = _victim;
        }
    }

    [Serializable]
    public class OnActorHitEvent
    {
        public DamageInfo damage { get; private set; }
        public HitInfo hit { get; private set; }

        public OnActorHitEvent(DamageInfo _damage, HitInfo _hit)
        {
            damage = _damage;
            hit = _hit;
        }
    }

    [Serializable]
    public class OnPlayerDealtDamageEvent
    {
        public DamageInfo damage { get; private set; }
        public HitInfo hit { get; private set; }

        public OnPlayerDealtDamageEvent(DamageInfo _damage, HitInfo _hit)
        {
            damage = _damage;
            hit = _hit;
        }
    }
}