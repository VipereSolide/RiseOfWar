using System.Collections.Generic;
using System;

using UnityEngine;

namespace RiseOfWar
{
    [Serializable]
    public class FpsActorControllerAdditionalData
    {
        public Camera currentSeatCamera;
        public Seat currentSeat;
        public CustomFpsActorController controller;
        public float stamina;

        public List<PlayerBadge> playerBadges = new List<PlayerBadge>();

        public void InitPlayerBadges()
        {
            playerBadges = new List<PlayerBadge>();
            playerBadges.Add(PlayerBadgesRegistry.GetPlayerBadgeByName("Fast Reload").SetLevel(3));
            playerBadges.Add(PlayerBadgesRegistry.GetPlayerBadgeByName("Tight Grip").SetLevel(3));
        }

        public FpsActorControllerAdditionalData()
        {
            InitPlayerBadges();
            currentSeatCamera = null;
            stamina = 0;
        }

        public FpsActorControllerAdditionalData(Camera _currentSeatCamera)
        {
            InitPlayerBadges();
            currentSeatCamera = _currentSeatCamera;
        }

        public FpsActorControllerAdditionalData(Seat _currentSeat)
        {
            currentSeat = _currentSeat;
        }
    }
}