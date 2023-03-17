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

        public FpsActorControllerAdditionalData()
        {
            currentSeatCamera = null;
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