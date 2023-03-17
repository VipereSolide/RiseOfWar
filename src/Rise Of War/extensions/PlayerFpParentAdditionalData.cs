using System;
using UnityEngine;

namespace RiseOfWar
{
    [Serializable]
    public class PlayerFpParentAdditionalData
    {
        public Transform recoilAnchor;
        public MouseController mouseController;

        public PlayerFpParentAdditionalData()
        {
            recoilAnchor = null;
        }

        public PlayerFpParentAdditionalData(Transform _recoilAnchor)
        {
            recoilAnchor = _recoilAnchor;
        }
    }
}