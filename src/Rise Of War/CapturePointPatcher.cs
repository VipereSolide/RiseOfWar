﻿using System;

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

            bool _isNeutralized = team == -1;

            OnCapturePointInteractionEvent _interaction = new OnCapturePointInteractionEvent(OnCapturePointInteractionEvent.InteractionType.Lost, ActorManager.AliveActorsInRange(__instance.transform.position, __instance.captureRange), __instance.owner, team, __instance);

            if (_isNeutralized && __instance.owner == GameManager.PlayerTeam())
            {
                // Lost
                _interaction = new OnCapturePointInteractionEvent(OnCapturePointInteractionEvent.InteractionType.Lost, ActorManager.AliveActorsInRange(__instance.transform.position, __instance.captureRange), __instance.owner, team, __instance);
            }

            if (_isNeutralized && __instance.owner != GameManager.PlayerTeam())
            {
                // Neutralized
                _interaction = new OnCapturePointInteractionEvent(OnCapturePointInteractionEvent.InteractionType.Neutralized, ActorManager.AliveActorsInRange(__instance.transform.position, __instance.captureRange), __instance.owner, team, __instance);
            }

            if (!_isNeutralized && team == GameManager.PlayerTeam())
            {
                // Enemy captured
                _interaction = new OnCapturePointInteractionEvent(OnCapturePointInteractionEvent.InteractionType.Captured, ActorManager.AliveActorsInRange(__instance.transform.position, __instance.captureRange), __instance.owner, team, __instance);
            }

            if (!_isNeutralized && team != GameManager.PlayerTeam())
            {
                // Player captured
                _interaction = new OnCapturePointInteractionEvent(OnCapturePointInteractionEvent.InteractionType.EnemyCapture, ActorManager.AliveActorsInRange(__instance.transform.position, __instance.captureRange), __instance.owner, team, __instance);
            }

            EventManager.onCapturePointInteraction.Invoke(_interaction);
        }
    }
}