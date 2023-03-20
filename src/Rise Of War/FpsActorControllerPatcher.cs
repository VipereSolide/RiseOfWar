using BepInEx;
using HarmonyLib;
using System.Text;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace RiseOfWar
{
    public class FpsActorControllerPatcher
    {
        static Vector3 _vehicleRootOffset = new Vector3(0, 1, 0);
        static Vector3 _vehicleOffset = new Vector3(0, 0, -3.5f);

        [HarmonyPatch(typeof(FpsActorController), "Awake")]
        [HarmonyPostfix]
        static void AwakePatch(FpsActorController __instance) { }

        [HarmonyPatch(typeof(FpsActorController), "Jump")]
        [HarmonyPostfix]
        static void JumpPatch(FpsActorController __instance)
        {
            __instance.GetAdditionalData().stamina += GameConfiguration.playerJumpStamina;
        }

        static void ManageStamina(FpsActorController __instance)
        {
            if (__instance == null)
            {
                Plugin.LogError("FpsActorControllerPatcher: Cannot manage stamina for null FPS actor controller!");
                return;
            }

            if (__instance.GetAdditionalData() == null)
            {
                __instance.AddData(new FpsActorControllerAdditionalData());

                if (__instance.GetAdditionalData() == null)
                {
                    Plugin.LogError("FpsActorControllerPatcher: Cannot manage stamina for fps actor controller with null additional data!");
                    return;
                }
            }

            if (__instance.actor == null)
            {
                Plugin.LogError("FpsActorControllerPatcher: Cannot manage stamina for null actor!");
                return;
            }

            float _currentStamina = __instance.GetAdditionalData().stamina;
            PlayerUI.instance.SetPlayerStaminaAmount(_currentStamina);

            if (__instance.IsSprinting())
            {
                if (_currentStamina < GameConfiguration.playerMaxStamina)
                {
                    _currentStamina += Time.deltaTime * GameConfiguration.playerStaminaIncreaseSpeed;
                }

                __instance.GetAdditionalData().stamina = _currentStamina;
                return;
            }

            _currentStamina -= Time.deltaTime * GameConfiguration.playerStaminaDecreaseSpeed;

            if (_currentStamina < 0f)
            {
                _currentStamina = 0f;
            }

            __instance.GetAdditionalData().stamina = _currentStamina;
        }

        static void ImplementCustomMovement(FpsActorController __instance)
        {
            __instance.GetAdditionalData().controller = __instance.gameObject.AddComponent<CustomFpsActorController>();
            __instance.GetAdditionalData().controller.Init(__instance, __instance.GetCharacterController());
            __instance.SetProperty<FirstPersonController>("controller", null);
        }

        [HarmonyPatch(typeof(FpsActorController), "Update")]
        [HarmonyPrefix]
        static bool UpdatePatch(FpsActorController __instance)
        {
            ManageStamina(__instance);

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (Time.timeScale <= 1)
                {
                    Time.timeScale = 2;
                }
                else
                {
                    Time.timeScale = 1;
                }
            }

            PlayerUI.instance.SetHealthAmount(__instance.actor.health);

            UpdateSeatCamera(__instance);

            return true;
        }

        static void UpdateSeatCamera(FpsActorController __instance)
        {
            if (__instance.GetAdditionalData().currentSeatCamera == null)
            {
                Vector3 _upVector = __instance.transform.up * (float)Traverse.Create(__instance).Field("cameraHeight").GetValue();

                if (__instance.GetCurrentSeat() != null)
                {
                    __instance.fpParent.fpCameraRoot.transform.position = __instance.GetCurrentSeat().transform.position + _upVector;
                }
                else
                {
                    __instance.fpParent.fpCameraRoot.transform.localPosition = _upVector;
                    __instance.fpParent.fpCamera.transform.localPosition = Vector3.zero;
                }
                return;
            }

            __instance.fpParent.fpCameraRoot.transform.position = __instance.GetAdditionalData().currentSeatCamera.transform.position + _vehicleRootOffset;
            __instance.fpParent.fpCamera.transform.localPosition = _vehicleOffset;
        }

        [HarmonyPatch(typeof(FpsActorController), "ResetFirstPersonCameraParent")]
        [HarmonyPostfix]
        static void ResetFirstPersonCameraParentPatch() { }

        [HarmonyPatch(typeof(FpsActorController), "StartSeated")]
        [HarmonyPrefix]
        static void StartSeatedPatch(FpsActorController __instance, Seat seat)
        {
            __instance.GetAdditionalData().currentSeat = seat;
        }

        [HarmonyPatch(typeof(FpsActorController), "BaseEndSeated")]
        [HarmonyPrefix]
        static void BaseEndSeatedPatch(FpsActorController __instance)
        {
            __instance.GetAdditionalData().currentSeat = null;
        }

        [HarmonyPatch(typeof(FpsActorController), "ReceivedDamage")]
        [HarmonyPrefix]
        static bool ReceivedDamagePatch(FpsActorController __instance, float damage, float balanceDamage, Vector3 direction)
        {
            if (balanceDamage > 50f)
            {
                __instance.Deafen();
            }

            Vector3 vector = __instance.GetActiveCamera().transform.worldToLocalMatrix.MultiplyVector(-direction);
            float angle = Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f;
            IngameUi.instance.ShowDamageIndicator(angle, damage < 2f && balanceDamage > damage);

            return false;
        }

        [HarmonyPatch(typeof(FpsActorController), "OnGUI")]
        [HarmonyPostfix]
        private static void OnGUIPatch(FpsActorController __instance)
        {
            if (__instance.actor == null && __instance.actor.activeWeapon == null)
            {
                return;
            }

            Weapon _current = __instance.actor.activeWeapon;

            if (_current == null)
            {
                return;
            }

            if (_current.GetAdditionalData() == null)
            {
                return;
            }

            StringBuilder _builder = new StringBuilder();

            _builder.AppendLine("Is complex reload: " + _current.configuration.advancedReload);
            _builder.AppendLine("Drop ammo when reloading: " + _current.configuration.dropAmmoWhenReloading);
            _builder.AppendLine("Use max ammo per reload: " + _current.configuration.useMaxAmmoPerReload);
            _builder.AppendLine("Max ammo per reload: " + _current.configuration.maxAmmoPerReload);
            _builder.AppendLine("Max remaining ammo after drop: " + _current.configuration.maxRemainingAmmoAfterDrop);

            _builder.AppendLine();

            if (_current.GetAdditionalData().modifications == null)
            {
                _builder.AppendLine("Modifications are null :(");
            }
            else
            {

                _builder.AppendLine($"Weapon current modifications:");

                if (_current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Bullet) != null)
                {
                    _builder.AppendLine($"Bullet = " + _current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Bullet).name);
                }
                else
                {
                    _builder.AppendLine($"Bullet = null");
                }

                if (_current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Sights) != null)
                {
                    _builder.AppendLine($"Sights = " + _current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Sights).name);
                }
                else
                {
                    _builder.AppendLine($"Sights = null");
                }

                if (_current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Trigger) != null)
                {
                    _builder.AppendLine($"Trigger = " + _current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Trigger).name);
                }
                else
                {
                    _builder.AppendLine($"Trigger = null");
                }

                if (_current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Spring) != null)
                {
                    _builder.AppendLine($"Spring = " + _current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Spring).name);
                }
                else
                {
                    _builder.AppendLine($"Spring = null");
                }

                if (_current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Barrel) != null)
                {
                    _builder.AppendLine($"Barrel = " + _current.GetAdditionalData().modifications.GetModification(WeaponModificationType.Barrel).name);
                }
                else
                {
                    _builder.AppendLine($"Barrel = null");
                }

                _builder.AppendLine($"");
                _builder.AppendLine($"Weapon possible modifications:");

                foreach (RegisteredWeaponModifications _registeredItem in _current.GetAdditionalData().modifications.possibleModifications)
                {
                    _builder.AppendLine($"- {_registeredItem.modification.name} ({_registeredItem.modification.GetModificationType()})");
                }
            }

            GUI.Label(new Rect(24, 24, 512, 1080), _builder.ToString());
        }
    }
}