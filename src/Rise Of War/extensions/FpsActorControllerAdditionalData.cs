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

        public FpsActorControllerAdditionalData()
        {
            currentSeatCamera = null;
            stamina = 0;
        }

        public FpsActorControllerAdditionalData(Camera _currentSeatCamera)
        {
            currentSeatCamera = _currentSeatCamera;
        }

        public FpsActorControllerAdditionalData(Seat _currentSeat)
        {
            currentSeat = _currentSeat;
        }
    }
}