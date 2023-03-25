using System.Collections.Generic;
using System.Collections;
using System;

using UnityEngine;

namespace RiseOfWar
{
    public static class PlayerBadgesRegistry
    {
        private static List<PlayerBadge> _playerBadges = new List<PlayerBadge>();

        public static PlayerBadge[] playerBadges
        {
            get { return _playerBadges.ToArray(); }
        }

        public static PlayerBadge GetPlayerBadgeByName(string name)
        {
            for (int _i = 0; _i < _playerBadges.Count; _i++)
            {
                if (_playerBadges[_i].name == name)
                {
                    return _playerBadges[_i];
                }
            }

            Plugin.LogError($"PlayerBadgesRegistry: Could not find player badge \"{name}\"!");
            return null;
        }

        public static PlayerBadge GetPlayerBadgeByIndex(int index)
        {
            if (index < 0 || index >= _playerBadges.Count)
            {
                Plugin.LogError($"PlayerBadgesRegistry: Given index ({index}) cannot be lower than 0 or higher than player badges count ({_playerBadges.Count})!");
            }

            return _playerBadges[index];
        }

        public static void InitPlayerBadges()
        {
            _playerBadges = new List<PlayerBadge>()
            {
                new PlayerBadge(
                    name: "Tight Grip",
                    description: "Reduces the amount of horizontal recoil.",
                    badgeIconPathBronze: Application.dataPath + GameConfiguration.defaultBadgesPath + "tight_grip_bronze.png",
                    badgeIconPathSilver: Application.dataPath + GameConfiguration.defaultBadgesPath + "tight_grip_silver.png",
                    badgeIconPathGold: Application.dataPath + GameConfiguration.defaultBadgesPath + "tight_grip_gold.png",
                    onCalled: (object _caller) =>
                    {
                        OnTightGripBadgeCalled(_caller, GetPlayerBadgeByIndex(0));
                    }
                ),
                new PlayerBadge(
                    name: "Fast Reload",
                    description: "Shortens the reload time.",
                    badgeIconPathBronze: Application.dataPath + GameConfiguration.defaultBadgesPath + "fast_reload_bronze.png",
                    badgeIconPathSilver: Application.dataPath + GameConfiguration.defaultBadgesPath + "fast_reload_silver.png",
                    badgeIconPathGold: Application.dataPath + GameConfiguration.defaultBadgesPath + "fast_reload_gold.png",
                    onCalled: (object _caller) =>
                    {
                        OnFastReloadBadgeCalled(_caller, GetPlayerBadgeByIndex(1));
                    }
                ),
            };
        }

        private static void OnFastReloadBadgeCalled(object _caller, PlayerBadge _badge)
        {
            if (_caller is Weapon)
            {
                Weapon _weapon = _caller as Weapon;
                _weapon.animator.speed = (_badge.level == 1) ? 1.18f : (_badge.level == 2) ? 1.3f : 1.43f;
            }
        }
        private static void OnTightGripBadgeCalled(object _caller, PlayerBadge _badge)
        {
            if (_caller is PlayerFpParent)
            {
                PlayerFpParent _parent = (PlayerFpParent)_caller;
                float _horizontalRecoilMultiplier = 1;

                switch (_badge.level)
                {
                    case 0:
                        _horizontalRecoilMultiplier = 0.75f;
                        break;
                    case 1:
                        _horizontalRecoilMultiplier = 0.65f;
                        break;
                    case 2:
                        _horizontalRecoilMultiplier = 0.35f;
                        break;
                }

                _parent.GetAdditionalData().horizontalRecoilMultiplier = _horizontalRecoilMultiplier;
            }
        }
    }
}