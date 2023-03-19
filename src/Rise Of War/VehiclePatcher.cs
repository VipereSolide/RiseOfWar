using HarmonyLib;

namespace RiseOfWar
{
    public class VehiclePatcher
    {
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