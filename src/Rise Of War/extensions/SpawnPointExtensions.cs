using System.Collections.Generic;

using UnityEngine;

namespace RiseOfWar
{
    public static class SpawnPointExtensions
    {
        public static bool IsContested(this SpawnPoint point)
        {
            List<Actor> _actorsOnPoint = ActorManager.AliveActorsInRange(point.transform.position, point.GetCaptureRange());
            bool _areEnemiesOnPoint = false;

            foreach (Actor _actor in _actorsOnPoint)
            {
                if (_actor.dead || _actor.team == point.owner || point.IsInsideCaptureRange(_actor.CenterPosition()) == false)
                {
                    continue;
                }

                _areEnemiesOnPoint = true;
                break;
            }

            return _areEnemiesOnPoint;
        }

        public static bool IsContested(this SpawnPoint point, int team)
        {
            List<Actor> _actorsOnPoint = ActorManager.AliveActorsInRange(point.transform.position, point.GetCaptureRange());
            bool _areEnemiesOnPoint = false;

            foreach (Actor _actor in _actorsOnPoint)
            {
                if (team == point.owner)
                {
                    if (_actor.dead || _actor.team == team || point.IsInsideCaptureRange(_actor.CenterPosition()) == false)
                    {
                        continue;
                    }
                }
                else
                {
                    if (_actor.dead || _actor.team == team || point.IsInsideProtectRange(_actor.CenterPosition()) == false)
                    {
                        continue;
                    }
                }

                _areEnemiesOnPoint = true;
                break;
            }

            return _areEnemiesOnPoint;
        }
    }
}