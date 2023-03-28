using HarmonyLib;

namespace RiseOfWar
{
    public class AiActorControllerPatcher
    {
        [HarmonyPatch(typeof(AiActorController), "FillDriverSeat")]
        [HarmonyPrefix]
        private static bool FillDriverSeatPatch(AiActorController __instance)
        {
            return false;
        }

        [HarmonyPatch(typeof(AiActorController), "EnterSeat")]
        [HarmonyPrefix]
        private static bool EnterSeatPatch(AiActorController __instance, Seat seat)
        {
            Vehicle _vehicle = seat.vehicle;

            if (_vehicle == null)
            {
                return true;
            }

            VehicleAdditionalData _data = _vehicle.GetAdditionalData();
            if (_data.owner == null || (!_data.owner.dead && _vehicle.Driver() == _data.owner))
            {
                return true;
            }

            return false;
        }
    }
}