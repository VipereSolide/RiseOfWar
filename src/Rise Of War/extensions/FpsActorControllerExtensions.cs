using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace RiseOfWar
{
    public static class FpsActorControllerExtensions
    {
        public static void AddXP(this FpsActorController _instance, int xp)
        {
            _instance.actor.GetAdditionalData().score += xp;
        }

        public static FirstPersonController GetController(this FpsActorController _instance)
        {
            return _instance.GetProperty<FirstPersonController>("controller");
        }

        public static CharacterController GetCharacterController(this FpsActorController _instance)
        {
            return _instance.GetProperty<CharacterController>("characterController");
        }

        public static bool mouseLookEnabled(this FpsActorController _instance)
        {
            return _instance.allowMouseLookRavenscriptOverride && _instance.GetProperty<TimedAction>("faceKillerAction").TrueDone() && (!_instance.GetProperty<bool>("mouseViewLocked") || (_instance.GetProperty<bool>("allowMouseViewWhileAiming") && _instance.actor.IsAiming()));
        }

        public static bool weaponIsOnCooldown(this FpsActorController _instance)
        {
            return _instance.actor.HasUnholsteredWeapon() && _instance.actor.activeWeapon.CoolingDown();
        }

        public static bool reloading(this FpsActorController _instance)
        {
            return _instance.actor.HasUnholsteredWeapon() && _instance.actor.activeWeapon.reloading;
        }

        public static void SetSeatCamera(this FpsActorController _fpsActorController, Camera _camera)
        {
            if (_camera == null)
            {
                Plugin.Log("FpsActorControllerExtensions: Disabling seat camera.");
                _fpsActorController.CancelOverrideCamera();
                _fpsActorController.EnableFirstPersonRenderingMode();
                _fpsActorController.FirstPersonCamera();

                _fpsActorController.fpParent.fpCameraRoot.transform.localPosition = Vector3.zero;
                _fpsActorController.fpParent.fpCameraParent.transform.localPosition = Vector3.zero;
                _fpsActorController.fpParent.fpCamera.transform.localPosition = Vector3.zero;
                _fpsActorController.fpParent.ResetCameraOffset();
            }
            else
            {
                _fpsActorController.EnableThirdPersonRenderingMode();
            }

            _fpsActorController.GetAdditionalData().currentSeatCamera = _camera;
            Traverse.Create(_fpsActorController).Method("ResetFirstPersonCameraParent").GetValue<Type>();
        }

        public static Seat GetCurrentSeat(this FpsActorController _fpsActorController)
        {
            return _fpsActorController.GetAdditionalData().currentSeat;
        }

        private static readonly ConditionalWeakTable<FpsActorController, FpsActorControllerAdditionalData> _data = new ConditionalWeakTable<FpsActorController, FpsActorControllerAdditionalData>();

        public static FpsActorControllerAdditionalData GetAdditionalData(this FpsActorController _fpsActorController)
        {
            return _data.GetOrCreateValue(_fpsActorController);
        }

        public static void AddData(this FpsActorController _fpsActorController, FpsActorControllerAdditionalData _value)
        {
            try
            {
                _data.Add(_fpsActorController, _value);
            }
            catch (Exception)
            {
            }
        }
    }
}