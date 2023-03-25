using System;
using System.Collections.Generic;
using UnityEngine;

namespace RiseOfWar.Events
{
    public static class EventManager
    {
        public static Action<OnActorHitEvent> onActorHit;
        public static Action<OnActorDieEvent> onActorDie;
        public static Action<OnActorSpawnEvent> onActorSpawn;
        public static Action<OnVehicleSpawnEvent> onVehicleSpawn;
        public static Action<OnPlayerDealtDamageEvent> onPlayerDealtDamage;
        public static Action<OnProjectileHitHitboxEvent> onProjectileHitHitbox;
        public static Action<OnCapturePointInteractionEvent> onCapturePointInteraction;
    }

    [Serializable]
    public class OnCapturePointInteractionEvent
    {
        public enum InteractionType
        {
            Captured,
            Neutralized,
            Lost,
            EnemyCapture,
        }

        public InteractionType type { get; private set; }
        public Actor[] actorsOnPoint { get; private set; }
        public int initialOwner { get; private set; }
        public int currentOwner { get; private set; }
        public SpawnPoint spawnpoint { get; private set; }

        public OnCapturePointInteractionEvent(InteractionType type, Actor[] actorsOnPoint, int initialOwner, int currentOwner, SpawnPoint spawnpoint)
        {
            this.type = type;
            this.actorsOnPoint = actorsOnPoint;
            this.initialOwner = initialOwner;
            this.currentOwner = currentOwner;
            this.spawnpoint = spawnpoint;
        }

        public OnCapturePointInteractionEvent(InteractionType type, List<Actor> actorsOnPoint, int initialOwner, int currentOwner, SpawnPoint spawnpoint)
        {
            this.type = type;
            this.actorsOnPoint = actorsOnPoint.ToArray();
            this.initialOwner = initialOwner;
            this.currentOwner = currentOwner;
            this.spawnpoint = spawnpoint;
        }
    }

    [Serializable]
    public class OnProjectileHitHitboxEvent
    {
        public Projectile projectile { get; private set; }
        public Vector3 hitPosition { get; private set; }
        public Hitbox hitbox { get; private set; }

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
    public class OnActorSpawnEvent
    {
        public Actor actor { get; private set; }

        public OnActorSpawnEvent(Actor actor)
        {
            this.actor = actor;
        }
    }

    [Serializable]
    public class OnVehicleSpawnEvent
    {
        public Vehicle vehicle { get; private set; }

        public OnVehicleSpawnEvent(Vehicle vehicle)
        {
            this.vehicle = vehicle;
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