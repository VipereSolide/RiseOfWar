using System;

using Random = UnityEngine.Random;
using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    public class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GameManager), "Awake")]
        [HarmonyPostfix]
        static void AwakePatch()
        {
            SoundManager.instance.ResetAudioMixerGroup();
        }

        [HarmonyPatch(typeof(GameManager), "StartGame")]
        [HarmonyPostfix]
        static void StartGamePatch(GameManager __instance)
        {
            DecalManager.SetBloodDecalColor(0, Random.ColorHSV(0, 0.05f, 0.5f, 1f, 0.1f, 0.5f, 0.1f, 0.125f));
            DecalManager.SetBloodDecalColor(1, Random.ColorHSV(0, 0.05f, 0.5f, 1f, 0.1f, 0.5f, 0.1f, 0.125f));
        }

        [HarmonyPatch(typeof(GameManager), "SetupVehiclePrefab")]
        [HarmonyPrefix]
        private static bool SetupVehiclePrefabPatch(GameObject parentObject, ModContentInformation contentInfo)
        {
            try
            {
                Vehicle _vehicle = parentObject.GetComponent<Vehicle>();
                if (_vehicle != null && _vehicle is IAutoUpgradeComponent)
                {
                    AutoUpgradeComponent.Upgrade(_vehicle);
                }

                GameManager.SetWorldSoundMixForObject(parentObject);
                GameManager.SetupRecursiveLayer(parentObject.transform, 12);
                ModManager.PreprocessContentModPrefab(parentObject, contentInfo);

                if (_vehicle.rigidbody != null && !_vehicle.rigidbody.isKinematic)
                {
                    _vehicle.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                }

                if (_vehicle.aiUseStrategy == Vehicle.AiUseStrategy.Default)
                {
                    if (_vehicle.IsAircraft() || _vehicle.aiType == Vehicle.AiType.Roam || _vehicle.aiType == Vehicle.AiType.Capture)
                    {
                        _vehicle.aiUseStrategy = Vehicle.AiUseStrategy.FromAnySpawn;
                    }
                    else
                    {
                        _vehicle.aiUseStrategy = Vehicle.AiUseStrategy.OnlyFromFrontlineSpawn;
                    }
                }

                foreach (Seat _seat in _vehicle.seats)
                {
                    MountedWeapon[] _weapons = _seat.weapons;

                    for (int i = 0; i < _weapons.Length; i++)
                    {
                        GameManager.SetupWeaponPrefab(_weapons[i], _vehicle, contentInfo);
                    }
                }

                VehicleRegistry.RegisterVehicle(_vehicle);
            }
            catch (Exception _exception)
            {
                Plugin.LogError($"GameManagerPatcher: Could not setup vehicle prefab correctly! {_exception}");
            }

            return false;
        }
    }
}