using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    public class SquadPatcher
    {
        [HarmonyPatch(typeof(Squad), "DropMember")]
        [HarmonyPrefix]
        private static void DropMemberPatch(Squad __instance, ActorController a)
        {
            if (a == __instance.Leader())
            {
                foreach (ActorController _actor in __instance.members)
                {
                    if (_actor.actor.IsSeated() && _actor.actor.aiControlled)
                    {
                        InitializeVehicleDrop(_actor, __instance);
                    }
                }
            }
        }

        private static void InitializeVehicleDrop(ActorController actorController, Squad squad)
        {
            actorController.StartCoroutine(InternalInitVehicleDrop(actorController, squad));
        }

        private static IEnumerator InternalInitVehicleDrop(ActorController actorController, Squad squad)
        {
            yield return new WaitForSeconds(0.75f + Random.Range(0f, 0.5f));

            if (actorController.GetSquad() == squad)
            {
                squad.DropMember(actorController);
            }

            actorController.actor.LeaveSeat(false);
        }
    }
}