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
                    _occupant.Kill(info);
                }
            }

            __instance.Die(info);
        }
    }
}