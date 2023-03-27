using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        public static VehicleAdditionalData GetAdditionalData(this Vehicle vehicle)
        {
            return _data.GetOrCreateValue(vehicle);
        }

        public static void AddData(this Vehicle vehicle, VehicleAdditionalData value)
        {
            try
            {
                _data.Add(vehicle, value);
            }
            catch (Exception)
            {
            }
        }

        public static void UpdateDriverState(this Vehicle vehicle)
        {
            VehicleAdditionalData _additionalData = vehicle.GetAdditionalData();

            if (_additionalData.owner == null && vehicle.HasDriver())
            {
                _additionalData.owner = vehicle.Driver();
            }
        }
    }
}