using System;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    using Events;

    public class CapturePointPatcher
    {
        [HarmonyPatch(typeof(CapturePoint), "SetOwner")]
        [HarmonyPrefix]
        static void SetOwnerPatch(CapturePoint __instance, int team, bool initialOwner = false)
        {
            if (initialOwner)
            {
                return;
            }

            if (team == GameManager.PlayerTeam())
            {
                EventManager.onCapturePointInteraction(new OnCapturePointInteractionEvent(
                    OnCapturePointInteractionEvent.InteractionType.Captured,
                    ActorManager.AliveActorsInRange(__instance.transform.position, __instance.captureRange),
                    __instance.owner,
                    team
                ));
            }
            else
            {
                if (team < -1)
                {
                    EventManager.onCapturePointInteraction(new OnCapturePointInteractionEvent(
                        OnCapturePointInteractionEvent.InteractionType.Neutralized,
                        ActorManager.AliveActorsInRange(__instance.transform.position, __instance.captureRange),
                        __instance.owner,
                        team
                    ));

                    return;
                }

                EventManager.onCapturePointInteraction(new OnCapturePointInteractionEvent(
                    OnCapturePointInteractionEvent.InteractionType.Lost,
                    ActorManager.AliveActorsInRange(__instance.transform.position, __instance.captureRange),
                    __instance.owner,
                    team
                ));
            }
        }
    }
}