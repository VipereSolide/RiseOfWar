using System;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    using Events;

    public class VehiclePatcher
    {
        [HarmonyPatch(typeof(Vehicle), "Damage")]
        [HarmonyPrefix]
        private static bool DamagePatch(Vehicle __instance, DamageInfo info)
        {
			if (__instance.isInvulnerable)
			{
				return false;
			}

			if (info.sourceWeapon != null)
            {
				info.healthDamage = info.sourceWeapon.GetAdditionalData().damageToVehicles;
            }

			if (!__instance.dead)
			{
				IngameUi.OnDamageDealt(info, new HitInfo(__instance));
			}

			__instance.health = Mathf.Clamp(__instance.health - info.healthDamage, 0f, __instance.maxHealth);
			
			if (info.sourceActor != null)
			{
				__instance.SetProperty("lastDamageSource", info.sourceActor);
				__instance.GetProperty<TimedAction>("lastDamageSourceAppliesAction").Start();

				if (info.healthDamage > 400f)
				{
					info.sourceActor.MarkHighPriorityTarget(10f);
				}
			}
			
			if (info.healthDamage > 900f)
			{
				__instance.CallPrivateMethod("HeavyDamage");
			}

			foreach (Seat seat in __instance.seats)
			{
				if (seat.IsOccupied())
				{
					seat.occupant.controller.OnVehicleWasDamaged(info.sourceActor, info.healthDamage);
				}
			}

			if (__instance.health <= 0f && !__instance.dead && !__instance.burning)
			{
				if (__instance.burnTime <= 0f)
				{
					__instance.Die(info);
				}
				else
				{
					__instance.CallPrivateMethod("StartBurning", new object[] { info });
				}
			}

			if (__instance.health < 0.5f * __instance.maxHealth && __instance.GetProperty<ParticleSystem>("smokeParticles") != null)
			{
				__instance.GetProperty<ParticleSystem>("smokeParticles").Play();
			}

			return false;
		}

        [HarmonyPatch(typeof(Vehicle), "Awake")]
        [HarmonyPostfix]
        private static void AwakePatch(Vehicle __instance)
        {
			if (__instance == null)
            {
				return;
            }

            EventManager.onVehicleSpawn.Invoke(new OnVehicleSpawnEvent(__instance));
            
			__instance.AddData(new VehicleAdditionalData()
            {
				owner = __instance.Driver()
            });
        }

		[HarmonyPatch(typeof(Vehicle), "OnDriverEntered")]
		[HarmonyPostfix]
		private static void OnDriverEnteredPatch(Vehicle __instance)
        {
			VehicleAdditionalData _additionalData = __instance.GetAdditionalData();

			if (_additionalData.owner == null && __instance.HasDriver() && !__instance.Driver().aiControlled)
			{
				_additionalData.owner = __instance.Driver();
			}
            else
            {
				int _seatIndex = 0;

				while (__instance.seats[0].occupant != _additionalData.owner)
                {
					if (_seatIndex >= __instance.seats.Count)
                    {
						__instance.seats[0].occupant.LeaveSeat(false);
						break;
					}

					__instance.seats[0].occupant.EnterSeat(__instance.seats[_seatIndex], false);
					_seatIndex++;
				}
            }
		}

        [HarmonyPatch(typeof(Vehicle), "StartBurning")]
        [HarmonyPostfix]
        private static void StartBurningPatch(Vehicle __instance, DamageInfo info)
        {
            foreach(Seat _seat in __instance.seats)
            {
                Actor _occupant = _seat.occupant;

                if (_occupant != null)
                {
                    info.healthDamage = _occupant.health + 1;
                    _occupant.Damage(info);
                }
            }

            __instance.Die(info);
        }
    }
}