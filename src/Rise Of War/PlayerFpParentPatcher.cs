using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace RiseOfWar
{
    public class PlayerFpParentPatcher
    {
        [HarmonyPatch(typeof(PlayerFpParent), "Awake")]
        [HarmonyPostfix]
        private static void AwakePatch(PlayerFpParent __instance)
        {
            MouseController _controller = __instance.gameObject.AddComponent<MouseController>();
            _controller.Init(__instance, FpsActorController.instance);
            __instance.GetAdditionalData().mouseController = _controller;
        }

        [HarmonyPatch(typeof(PlayerFpParent), "Start")]
        [HarmonyPrefix]
        private static void StartPatch(PlayerFpParent __instance)
        {
            Traverse.Create(__instance).Field("fovSpeed").SetValue(0);
            __instance.CreateRecoilAnchor();
        }

        [HarmonyPatch(typeof(PlayerFpParent), "FixedUpdate")]
        [HarmonyPrefix]
        private static bool FixedUpdatePatch(PlayerFpParent __instance)
        {
            FirstPersonController _fpsController = (FirstPersonController)Traverse.Create(__instance).Field("fpsController").GetValue();
            Actor _actor = (Actor)Traverse.Create(__instance).Field("actor").GetValue();
            MeanFilterVector3 _smoothInputFilter = (MeanFilterVector3)Traverse.Create(__instance).Field("smoothInputFilter").GetValue();
            float _proneCrawlAmount = (float)Traverse.Create(__instance).Field("proneCrawlAmount").GetValue();
            float _walkOffsetMagnitude = (float)Traverse.Create(__instance).Field("walkOffsetMagnitude").GetValue();
            Spring _rotationSpring = (Spring)Traverse.Create(__instance).Field("rotationSpring").GetValue();
            Spring _positionSpring = (Spring)Traverse.Create(__instance).Field("positionSpring").GetValue();
            Vector3 _lastPosition = (Vector3)Traverse.Create(__instance).Field("lastPosition").GetValue();
            Vector3 _lastRotation = (Vector3)Traverse.Create(__instance).Field("lastRotation").GetValue();
            TimedAction _weaponSnapAction = (TimedAction)Traverse.Create(__instance).Field("weaponSnapAction").GetValue();
            float _weaponSnapFrequency = (float)Traverse.Create(__instance).Field("weaponSnapFrequency").GetValue();
            float _weaponSnapMagnitude = (float)Traverse.Create(__instance).Field("weaponSnapMagnitude").GetValue();
            Vector3 _shoulderParentOrigin = (Vector3)Traverse.Create(__instance).Field("shoulderParentOrigin").GetValue();

            float num = _fpsController.StepCycle() * 3.1415927f;
            float num2 = 0f;
            Vector3 input = new Vector3(SteelInput.GetAxis(SteelInput.KeyBinds.Horizontal), 0f, SteelInput.GetAxis(SteelInput.KeyBinds.Vertical));
            Vector3 vector = _smoothInputFilter.Tick(input);
            float num3 = __instance.proneCrawlAction.TrueDone() ? Mathf.Clamp01(vector.magnitude) : 0f;

            if (!_actor.IsSeated())
            {
                num2 = Mathf.Clamp01(_actor.Velocity().sqrMagnitude / 60f) * (_actor.IsAiming() ? 0.003f : 0.04f);
                if (_actor.activeWeapon != null)
                {
                    if (_actor.controller.IsSprinting())
                    {
                        num2 *= _actor.activeWeapon.sprintBobMultiplier;
                    }
                    else
                    {
                        num2 *= _actor.activeWeapon.walkBobMultiplier;
                    }

                    num3 *= _actor.activeWeapon.proneBobMultiplier;
                }
            }

            Vector3 localEulerAngles = __instance.fpCameraRoot.localEulerAngles;
            Vector3 zero = Vector3.zero;
            Vector3 zero2 = Vector3.zero;

            Traverse.Create(__instance).Field("proneCrawlAmount").SetValue(Mathf.MoveTowards(_proneCrawlAmount, num3, Time.deltaTime * 2.5f));

            if (_proneCrawlAmount > 0f)
            {
                Traverse.Create(__instance).Field("walkOffsetMagnitude").SetValue(Mathf.MoveTowards(_walkOffsetMagnitude, 0f, Time.deltaTime * 0.06f));

                float num4 = Mathf.Pow(Mathf.Sin(num * 2f), 2f) * 1.5f;
                float num5 = Mathf.SmoothStep(0f, 1f, _proneCrawlAmount);
                float num6 = Mathf.DeltaAngle(0f, localEulerAngles.x);

                zero = new Vector3((Mathf.Cos(num * 2f) - num4 * vector.x) * num5 * 0.05f, (num6 * 0.002f - 0.13f) * num5, num4 * vector.z * num5 * 0.05f);
                zero2 = new Vector3(num6 * -0.7f * num5, (Mathf.Cos(num * 2f) - 1f) * num5 * 20f, 0f);
            }
            else
            {
                if (!_fpsController.OnGround())
                {
                    Traverse.Create(__instance).Field("walkOffsetMagnitude").SetValue(Mathf.MoveTowards(_walkOffsetMagnitude, 0f, Time.deltaTime * 0.24f));
                    if (_fpsController.Velocity().y < -2f && !_actor.IsAiming())
                    {
                        _rotationSpring.AddVelocity(new Vector3(-3f, Mathf.Sin(Time.time * 6f), 0f));
                    }
                }
                else
                {
                    Traverse.Create(__instance).Field("walkOffsetMagnitude").SetValue(Mathf.MoveTowards(_walkOffsetMagnitude, 0f, Time.deltaTime * 0.06f));
                }

                zero = new Vector3(Mathf.Cos(num) * _walkOffsetMagnitude, Mathf.Sin(num * 2f) * _walkOffsetMagnitude * 0.7f, 0f);
            }

            _positionSpring.Update();
            _rotationSpring.Update();

            Vector3 vector2 = __instance.fpCameraParent.worldToLocalMatrix.MultiplyVector(__instance.movementProbe.position - _lastPosition);
            Vector2 vector3 = new Vector2(Mathf.DeltaAngle(_lastRotation.x, __instance.movementProbe.eulerAngles.x), Mathf.DeltaAngle(_lastRotation.y, __instance.movementProbe.eulerAngles.y));
            Traverse.Create(__instance).Field("lastPosition").SetValue(__instance.movementProbe.position);
            Traverse.Create(__instance).Field("lastRotation").SetValue(__instance.movementProbe.eulerAngles);

            float num7 = 0f;
            if (!_weaponSnapAction.TrueDone())
            {
                float num8 = 1f - _weaponSnapAction.Ratio();
                num7 = num8 * Mathf.Sin(_weaponSnapAction.Ratio() * (0.1f + num8) * _weaponSnapFrequency) * _weaponSnapMagnitude;
            }

            float _swayMultiplier = 1.0f;

            if (_actor.IsAiming() && _actor.activeWeapon != null)
            {
                if (WeaponRegistry.IsCustomWeapon(_actor.activeWeapon))
                {
                    if (_actor.activeWeapon.weaponProperties() != null)
                    {
                        _swayMultiplier = _actor.activeWeapon.weaponProperties().GetFloat("sway");
                    }
                }
            }

            __instance.shoulderParent.localPosition = _shoulderParentOrigin + (_positionSpring.position + new Vector3(0f, num7 * -0.1f, 0f) + zero) * _swayMultiplier;
            __instance.shoulderParent.localEulerAngles = (_rotationSpring.position + new Vector3(num7 * -20f, 0f, 0f) + zero2) * _swayMultiplier;

            _rotationSpring.position += new Vector3(-0.1f * vector3.x + vector2.y * 5f, -0.1f * vector3.y, 0f);
            _positionSpring.position += new Vector3(-0.0001f * vector3.y, 0.0001f * vector3.x, 0f);

            return false;
        }

        [HarmonyPatch(typeof(PlayerFpParent), "Update")]
        [HarmonyPrefix]
        private static bool UpdatePatch(PlayerFpParent __instance)
        {
            Actor _actor = (Actor)Traverse.Create(__instance).Field("actor").GetValue();
            float _targetFOV = Options.GetSlider(OptionSlider.Id.Fov);
            Weapon _currentWeapon = _actor.activeWeapon;

            if (_actor.IsAiming())
            {
                _targetFOV = GameConfiguration.defaultAimingFieldOfView;

                if (_currentWeapon != null)
                {
                    float _aimFOV = _currentWeapon.weaponProperties().aiming.GetFloat(WeaponXMLProperties.Aiming.AIM_FOV);
                    _targetFOV = (_aimFOV != 0) ? _aimFOV : GameConfiguration.defaultAimingFieldOfView;
                }
            }

            float _fovSpeed = 10f;
            if (_currentWeapon != null && _currentWeapon.weaponProperties() != null)
            {
                _fovSpeed = _actor.activeWeapon.weaponProperties().aiming.GetFloat(WeaponXMLProperties.Aiming.SPEED);
            }

            __instance.fpCamera.fieldOfView = Mathf.Lerp(__instance.fpCamera.fieldOfView, _targetFOV, Time.deltaTime * _fovSpeed * GameConfiguration.constantAimingFOVSpeedMultiplier);
            __instance.fpCameraRoot.localEulerAngles.SetX(-__instance.GetAdditionalData().recoilAnchor.localEulerAngles.x);

            return false;
        }
    }
}