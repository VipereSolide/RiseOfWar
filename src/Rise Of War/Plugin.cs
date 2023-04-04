using BepInEx;
using HarmonyLib;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RiseOfWar
{
    /// <summary>
    /// This is the main plugin class that BepInEx injects and executes.
    /// This class provides MonoBehaviour methods and additional BepInEx-specific services like logging and
    /// configuration system.
    /// </summary>
    /// <remarks>
    /// BepInEx plugins are MonoBehaviours. Refer to Unity documentation for information on how to use various Unity
    /// events: https://docs.unity3d.com/Manual/class-MonoBehaviour.html
    ///
    /// To get started, check out the plugin writing walkthrough:
    /// https://bepinex.github.io/bepinex_docs/master/articles/dev_guide/plugin_tutorial/index.html
    /// </remarks>
    [BepInPlugin(PluginInfo.PLUGIN_ID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource _source;

        /*
         * INFO: Everything to change inside the base assembly:
         * Weapon.cs:
         * Line 298: Remove:
         *  if (this.user != null)
		 *  {
		 *	    this.animator.SetBool(Weapon.TUCK_PARAMETER_HASH, this.user.controller.IsSprinting() && !this.reloading);
		 *	}
		 *	
		 * Line 0: Add:
		 *  private void LateUpdate(){}
		 *	
		 *	PlayerFpParent.cs:
		 *	Line 0: Add:
		 *	private void Update(){}
		 *	
		 *	Projectile.cs:
		 *	Change UpdatePosition() to be public.
		 *	Change Travel() to be public.
        */

        private void OnGUI()
        {
            if (GameConfiguration.isDebugModeEnabled)
            {
                GUI.Label(new Rect(0, 0, 200, 50), "Debug mode");
            }
        }

        private void Awake()
        {
            _source = BepInEx.Logging.Logger.CreateLogSource(" Rise Of War");
            Log("Initializing plugin...");

            RegisterPatches();

            PlayerBadgesRegistry.InitPlayerBadges();
            VehicleRegistry.Awake();

            GameObject _soundManager = new GameObject("Sound Manager");
            _soundManager.AddComponent<SoundManager>();

            GameObject _resourceManager = new GameObject("Resource Manager");
            _resourceManager.AddComponent<ResourceManager>();

            ResourceManager.Instance.LoadLoadingScreenAssetBundle();
            ResourceManager.Instance.EnableLoadingScreen();
        }

        public static void Log(string _message, [CallerMemberName] string caller = null)
        {
            if (_message.Contains(":"))
            {
                _message = _message.Insert(_message.IndexOf(":"), $".{caller}");
            }
            else
            {
                _message = _message.Insert(0, $"{caller}: ");
            }

            _source.LogInfo(_message);
        }
        
        public static void LogWarning(string _message, [CallerMemberName] string caller = null)
        {
            if (_message.Contains(":"))
            {
                _message = _message.Insert(_message.IndexOf(":"), $".{caller}");
            }

            _source.LogWarning(_message);
        }
        
        public static void LogError(string _message, [CallerMemberName] string caller = null)
        {
            if (_message.Contains(":"))
            {
                _message = _message.Insert(_message.IndexOf(":"), $".{caller}");
            }

            _source.LogError(_message);
        }

        public static void EndLogGroup()
        {
            _source.LogInfo("");
        }

        private void RegisterPatches()
        {
            var _instance = new Harmony("Rise Of War");

            _instance.PatchAll(typeof(SeatPatcher));
            _instance.PatchAll(typeof(SquadPatcher));
            _instance.PatchAll(typeof(ActorPatcher));
            _instance.PatchAll(typeof(ActorPatcher));
            _instance.PatchAll(typeof(HitboxPatcher));
            _instance.PatchAll(typeof(WeaponPatcher));
            _instance.PatchAll(typeof(VehiclePatcher));
            _instance.PatchAll(typeof(OptionsPatcher));
            _instance.PatchAll(typeof(MainMenuPatcher));
            _instance.PatchAll(typeof(IngameUiPatcher));
            _instance.PatchAll(typeof(MouseLookPatcher));
            _instance.PatchAll(typeof(ModManagerPatcher));
            _instance.PatchAll(typeof(ProjectilePatcher));
            _instance.PatchAll(typeof(GameManagerPatcher));
            _instance.PatchAll(typeof(GameModeBasePatcher));
            _instance.PatchAll(typeof(CapturePointPatcher));
            _instance.PatchAll(typeof(DecalManagerPatcher));
            _instance.PatchAll(typeof(WeaponManagerPatcher));
            _instance.PatchAll(typeof(PlayerFpParentPatcher));
            _instance.PatchAll(typeof(AiActorControllerPatcher));
            _instance.PatchAll(typeof(WeaponSelectionUiPatcher));
            _instance.PatchAll(typeof(WeaponSwitchGroupPatcher));
            _instance.PatchAll(typeof(FpsActorControllerPatcher));
            _instance.PatchAll(typeof(FirstPersonControllerPatcher));
        }
    }
}
