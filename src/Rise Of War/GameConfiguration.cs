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
        public static string defaultPatchesPath = "/Resources/Data/Patches/";

        /// <summary>
        /// Represents the field of view of the camera when any unscoped weapon is aiming.
        /// </summary>
        public static float defaultAimingFieldOfView = 55;

        public static float constantAimingFOVSpeedMultiplier = 1.5f;

        public static float projectileLifetime = 20f;

        /// <summary>
        /// The time the hitmarkers stay active on screen before disappearing.
        /// </summary>
        public static float hitmarkerLifetime = 0.08f;

        /// <summary>
        /// The interval in frames count between two killfeed clear. This is to avoid duplicated messages.
        /// The lower the number, the less chances there will be to get a duplicated message, but the more
        /// it will impact performances.
        /// </summary>
        public static int killfeedClearInterval = 2;

        /* UI SETTINGS */

        public static Color32 playerUIAttachmentInactiveColor = new Color32(131, 130, 123, 255);
        public static Color32 playerUIAttachmentActiveColor = new Color32(255, 255, 255, 255);
        public static string playerUICurrentAmmoZeroColor = "83827b";

        /* MOVEMENT SETTINGS */

        public static float walkingSpeed;
        public static float sprintingSpeed;
        public static float speedTransitionSpeed;
        public static float velocityStartSpeed;
    }
}