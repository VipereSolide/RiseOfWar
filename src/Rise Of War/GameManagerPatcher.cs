﻿using HarmonyLib;
using Lua;
using MapEditor;
using System;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

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

        [HarmonyPatch(typeof(GameManager), "HandleArgument")]
        [HarmonyPrefix]
        private static bool HandleArgumentPatch(GameManager __instance, string argument, string parameter)
        {
            string message = "GameManager: " + string.Format("Handling argument: {0} {1}", argument, parameter) + ".";
            ScriptConsole.instance.LogInfo(message);
            Plugin.Log(message);

            if (argument == "-debug")
            {
                Plugin.Log("GameManager: Using debug mode.");

                GameConfiguration.isDebugModeEnabled = true;
                return false;
            }

            if (argument == "-generatenavcache")
            {
                Plugin.Log("GameManager: Generate cache mode enabled.");

                __instance.generateNavCache = true;
                __instance.navCacheWritebackPath = parameter;
                ModManager.instance.noContentMods = true;

                return false;
            }

            if (argument == "-custommap")
            {
                Plugin.Log($"GameManager: Loading custom map \"{parameter}\".");

                InstantActionMaps.MapEntry mapEntry = new InstantActionMaps.MapEntry();
                mapEntry.isCustomMap = true;
                mapEntry.sceneName = parameter;
                __instance.CallPrivateMethod("SetupDefaultGameParameters");

                GameManager.StartLevel(mapEntry, __instance.gameModeParameters);
                return false;
            }

            if (argument == "-editmap")
            {
                Plugin.Log($"GameManager: Loading map editor for map = \"{parameter}\".");

                InstantActionMaps.MapEntry entry = SceneConstructor.InstantActionMapsEntry(parameter, SceneConstructor.Mode.Edit);
                __instance.CallPrivateMethod("SetupDefaultGameParameters");

                GameManager.StartLevel(entry, __instance.gameModeParameters);
                return false;
            }

            if (argument == "-map")
            {
                __instance.CallPrivateMethod("SetupDefaultGameParameters");
                __instance.CallPrivateMethod("AutoStartMapWhenLoadingDone", new object[] { parameter });

                return false;
            }

            if (argument == "-debuggizmos")
            {
                __instance.CallPrivateMethod("InitializeIngameDebugGizmos");

                return false;
            }

            if (argument == "-nocontentmods")
            {
                ModManager.instance.noContentMods = true;
                return false;
            }

            if (argument == "-noworkshopmods")
            {
                ModManager.instance.noWorkshopMods = true;
                return false;
            }

            if (argument == "-verbose")
            {
                GameManager.verboseLogging = true;
                return false;
            }

            if (argument == "-testcontentmod")
            {
                if (!__instance.testContentModMode)
                {
                    __instance.testContentModMode = true;

                    __instance.CallPrivateMethod("InitializeIngameDebugGizmos");
                    __instance.CallPrivateMethod("InitializeTestContentModMode");

                    ModManager.instance.ClearContentModData();
                    __instance.gameInfo.LoadOfficial();
                }

                GC.Collect();
                long _totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
                ModInformation _modInfo = ModManager.instance.LoadSingleModContentBundle(parameter);
                GC.Collect();

                int _usedMemory = (int)((Profiler.GetTotalAllocatedMemoryLong() - _totalAllocatedMemoryLong) / 1000000L);
                ScriptConsole.instance.LogInfo("Loaded content mod: {0}, memory usage: {1} MB", new object[]
                {
                    parameter,
                    _usedMemory
                });

                __instance.gameInfo.AdditiveLoadSingleMod(_modInfo);
                return false;
            }
            if (argument == "-benchmark")
            {
                new InstantActionMaps.MapEntry();

                if (!__instance.GetProperty<bool>("autoStartMapArmed"))
                {
                    __instance.CallPrivateMethod("AutoStartMapWhenLoadingDone", new object[] { "island" });
                }

                int _actorCount;

                if (!string.IsNullOrEmpty(parameter) && int.TryParse(parameter, out _actorCount))
                {
                    __instance.CallPrivateMethod("SetupBenchmarkGameParameters", new object[] { _actorCount });
                }
                else
                {
                    __instance.CallPrivateMethod("SetupBenchmarkGameParameters", new object[] { 60 });
                }

                Benchmark.isRunning = true;
                __instance.benchmarkMutator.isEnabled = true;
                ModManager.instance.builtInMutators.Add(__instance.benchmarkMutator);

                return false;
            }

            if (argument == "-testsessionid")
            {
                RavenscriptManager.instance.OnStartTestSession(parameter);
                return false;
            }

            if (argument == "-testsession")
            {
                RavenscriptManager.instance.OnStartTestSession(parameter);
                return false;
            }

            if (argument == "-modstagingpath")
            {
                ModManager.instance.modStagingPathOverride = parameter;
                ScriptConsole.instance.LogInfo("Mod staging path set to: {0}", new object[]
                {
                    ModManager.ModStagingPath()
                });
                return false;
            }

            if (!(argument == "-nointro"))
            {
                if (argument == "-resetresolution")
                {
                    Screen.SetResolution(1280, 720, false);
                    return false;
                }

                if (argument == "-strictmodversion")
                {
                    ModManager.instance.strictModVersionFilter = true;
                    return false;
                }

                string _errorLog = "GameManager: " + string.Format("Unrecognized Argument {0}", argument) + ".";
                ScriptConsole.instance.LogInfo(_errorLog);
                Plugin.LogWarning(_errorLog);
                return false;
            }

            return false;
        }
    }
}