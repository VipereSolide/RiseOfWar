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
        private static bool EnterSeatPatch(AiActorController __instance, ref Seat seat)
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

            seat = null;
            return true;
        }

        [HarmonyPatch(typeof(AiActorController), "HasTargetVehicle")]
        [HarmonyPrefix]
        public static bool HasTargetVehiclePatch(AiActorController __instance, ref bool __result)
        {
            __result = false;

            if (__instance.targetVehicle)
            {
                __result = true;

                VehicleAdditionalData _data = __instance.targetVehicle.GetAdditionalData();

                if (_data.owner != null && _data.owner.IsDriver() == false)
                {
                    __result = false;
                }

                if (_data.owner != null && __instance.targetVehicle.Driver() != _data.owner)
                {
                    __result = false;
                }

                if (_data.owner != null && _data.owner.dead)
                {
                    __result = false;
                }
            }

            return false;
        }
    }
}