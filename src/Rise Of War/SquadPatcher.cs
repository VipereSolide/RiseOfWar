﻿using System.Collections.Generic;
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
                foreach (ActorController b in __instance.members)
                {
                    if (b.actor.IsSeated() && b.actor.aiControlled)
                    {
                        InitializeVehicleDrop(b);
                    }
                }
            }
        }

        private static void InitializeVehicleDrop(ActorController actorController)
        {
            actorController.StartCoroutine(InternalInitVehicleDrop(actorController));
        }

        private static IEnumerator InternalInitVehicleDrop(ActorController actorController)
        {
            yield return new WaitForSeconds(0.75f + Random.Range(0f, 0.5f));

            actorController.actor.LeaveSeat(false);
        }
    }
}