using UnityEngine;

namespace RiseOfWar
{
    public static class GameConfiguration
    {
        public static string defaultWeaponDataPath = "/Resources/Data/Weapons/";
        public static string defaultWeaponModificationsPath = "/Resources/Data/Modifications/";
        public static string defaultImagesPath = "/Resources/Images/";
        public static string defaultAssetBundlesPath = "/Resources/Data/_packed/";
        public static string defaultMusicThemesPath = "/Resources/Sounds/Music/Themes/";
        public static string defaultCaptureJinglesPath = "/Resources/Sounds/Music/Capture Jingles/";
        public static string defaultPatchesPath = "/Resources/Data/Patches/";
        public static string defaultBadgesPath = "/Resources/Images/Badges/";
        public static string defaultWhistlePath = "/Resources/Sounds/Whistling/";
        public static string defaultHurtPath = "/Resources/Sounds/Damage/Pain/";
        public static string defaultMeshModificationsPath = "/Resources/Data/Mesh Modifications/";

        public static Color32 hitmarkerKillColor = Color.red;
        public static Color32 hitmarkerNormalHitColor = Color.white;

        public static readonly int interractableLayer = 9;

        public static readonly float actorDamageInvulnerabilityTime = 0.05f;

        public static readonly float capturePointCaptureDelay = 0.2f;

        public static readonly float killfeedItemLifetime = 5f;

        /// <summary>
        /// Represents the field of view of the camera when any unscoped weapon is aiming.
        /// </summary>
        public static float defaultAimingFieldOfView = 55;

        public static float constantAimingFOVSpeedMultiplier = 1.5f;

        public static float projectileLifetime = 20f;

        public static float whistleDelay = 7;

        public static float weaponPickupDistance = 2f;

        public static float droppedWeaponLifetime = 15;

        /// <summary>
        /// The time the hitmarkers stay active on screen before disappearing.
        /// </summary>
        public static float hitmarkerLifetime = 0.08f;

        public static float cameraRecoilSmoothness = 20;

        /// <summary>
        /// The interval in frames count between two killfeed clear. This is to avoid duplicated messages.
        /// The lower the number, the less chances there will be to get a duplicated message, but the more
        /// it will impact performances.
        /// </summary>
        public static int killfeedClearInterval = 2;

        public static float globalKillfeedItemLifetime = 6;

        public static bool isDebugModeEnabled = false;

        /* UI SETTINGS */

        public static Color32 playerUIAttachmentInactiveColor = new Color32(131, 130, 123, 255);
        public static Color32 playerUIAttachmentActiveColor = new Color32(255, 255, 255, 255);
        public static string playerUICurrentAmmoZeroColor = "83827b";

        public static readonly string WHITE_COLOR = "C2BFB3";
        public static readonly string GREEN_COLOR = "95BD63";
        public static readonly string RED_COLOR = "832423";
        public static readonly string BLUE_COLOR = "435462";

        /* MOVEMENT SETTINGS */

        public static float walkingSpeed;
        public static float sprintingSpeed;
        public static float speedTransitionSpeed;
        public static float velocityStartSpeed;
        public static float playerMaxStamina = 15;
        public static float playerStaminaIncreaseSpeed = 1;
        public static float playerStaminaDecreaseSpeed = 1.25f;
        public static float playerJumpStamina = 3;
    }
}