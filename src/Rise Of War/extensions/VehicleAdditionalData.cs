using System.Collections.Generic;
using System;

using UnityEngine;
using Random = UnityEngine.Random;

namespace RiseOfWar
{
    [Serializable]
    public class VehicleAdditionalData
    {
        public List<string> vehicleParts = new List<string>();
        public Actor owner;

        public string DestroyPart(out bool destroyed)
        {
            if (vehicleParts.Count == 0)
            {
                destroyed = false;
                return "";
            }

            int _index = Random.Range(0, vehicleParts.Count);
            string _part = vehicleParts[_index];
            vehicleParts.RemoveAt(_index);

            destroyed = true;
            return _part;
        }

        public VehicleAdditionalData()
        {
            vehicleParts = new List<string>();
            vehicleParts.AddRange(VehicleExtensions.vehicleParts);
        }
    }
}