using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System;

using UnityEngine;

namespace RiseOfWar
{
    public static class VehicleExtensions
    {
        public static readonly List<string> vehicleParts = new List<string>()
        {
            "Engine",
            "Wheels",
            "Transmission",
            "Ammo Compartiment",
            "Breaks",
            "Right Fuel Tank",
            "Left Fuel Tank",
            "Seat Holders",
            "Bumpers",
            "Cylinders",
            "Front Lights",
            "Chassis",
            "Steering System",
            "Ignition System",
            "Propeller Shaft",
            "Axles",
            "Gear Shift",
            "Trunk",
        };

        private static readonly ConditionalWeakTable<Vehicle, VehicleAdditionalData> _data = new ConditionalWeakTable<Vehicle, VehicleAdditionalData>();

        public static VehicleAdditionalData GetAdditionalData(this Vehicle _Vehicle)
        {
            return _data.GetOrCreateValue(_Vehicle);
        }

        public static void AddData(this Vehicle _Vehicle, VehicleAdditionalData _value)
        {
            try
            {
                _data.Add(_Vehicle, _value);
            }
            catch (Exception)
            {
            }
        }
    }
}