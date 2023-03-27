using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace RiseOfWar
{
    using Events;

    public class CapturePointPatcher
    {
        static Actor[] ActorsInCapturePointRange(CapturePoint point)
        {
            List<Actor> _actors = ActorManager.AliveActors();
            List<Actor> _output = new List<Actor>();

            foreach (Actor _actor in _actors)
            {
                if (point.owner == _actor.team)
                {
                    if (point.IsInsideProtectRange(_actor.CenterPosition()))
                    {
                        _output.Add(_actor);
                    }
                }
                else
                {
                    if (point.IsInsideCaptureRange(_actor.CenterPosition()))
                    {
                        _output.Add(_actor);
                    }
                }
            }

            return _output.ToArray();
        }

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
            Actor[] _actorsInRange = ActorsInCapturePointRange(__instance);

            foreach (Actor _actor in _actorsInRange)
            {
                Debug.Log("Actors in range: " + _actor.name);
            }

            if (_isNeutralized && __instance.owner == GameManager.PlayerTeam())
            {
                // Lost
                _interaction = new OnCapturePointInteractionEvent(OnCapturePointInteractionEvent.InteractionType.Lost, _actorsInRange, __instance.owner, team, __instance);
            }

            if (_isNeutralized && __instance.owner != GameManager.PlayerTeam())
            {
                // Neutralized
                _interaction = new OnCapturePointInteractionEvent(OnCapturePointInteractionEvent.InteractionType.Neutralized, _actorsInRange, __instance.owner, team, __instance);
            }

            if (!_isNeutralized && team == GameManager.PlayerTeam())
            {
                // Enemy captured
                _interaction = new OnCapturePointInteractionEvent(OnCapturePointInteractionEvent.InteractionType.Captured, _actorsInRange, __instance.owner, team, __instance);
            }

            if (!_isNeutralized && team != GameManager.PlayerTeam())
            {
                // Player captured
                _interaction = new OnCapturePointInteractionEvent(OnCapturePointInteractionEvent.InteractionType.EnemyCapture, _actorsInRange, __instance.owner, team, __instance);
            }

            EventManager.onCapturePointInteraction.Invoke(_interaction);
        }
    }
}