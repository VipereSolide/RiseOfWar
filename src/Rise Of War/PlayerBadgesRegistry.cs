﻿using System.Collections.Generic;
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
                )
            };
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
                        _horizontalRecoilMultiplier = 20;
                        break;
                    case 1:
                        _horizontalRecoilMultiplier = 4;
                        break;
                    case 2:
                        _horizontalRecoilMultiplier = 12;
                        break;
                }

                _parent.GetAdditionalData().horizontalRecoilMultiplier = _horizontalRecoilMultiplier;
            }
        }
    }
}