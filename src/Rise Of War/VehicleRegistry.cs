using System.Collections.Generic;

using UnityEngine;

namespace RiseOfWar
{
    using Events;

    public static class VehicleRegistry
    {
        private static List<Vehicle> _registeredVehicles = new List<Vehicle>();

        public static Vehicle[] RegisteredVehicles { get { return _registeredVehicles.ToArray(); } }

        public static void RegisterVehicle(Vehicle vehicle)
        {
            _registeredVehicles.Add(vehicle);
        }

        public static void Awake()
        {
        }

        public static void OnPlayerSpawned(OnPlayerSpawnedEvent _event)
        {
            Actor _player = ReferenceManager.player;

            int _width = 0;
            int _length = 0;

            List<GameObject> _vehicles = new List<GameObject>();

            foreach (VehicleSpawner.VehicleSpawnType _vehicleSpawnType in VehicleSpawner.ALL_VEHICLE_TYPES)
            {
                foreach (GameObject _vehiclePrefab in ModManager.instance.vehiclePrefabs[_vehicleSpawnType])
                {
                    _vehicles.Add(_vehiclePrefab);
                }
            }

            Plugin.Log($"VehicleRegistry: Vehicle counts = {_vehicles.Count}.");

            foreach (GameObject _vehiclePrefab in _vehicles)
            {
                if (_vehiclePrefab == null)
                {
                    continue;
                }

                // Vehicle _vehicle = _vehiclePrefab.GetComponent<Vehicle>();

                Vector3 _position = Vector3.zero;
                RaycastHit _hit;

                if (Physics.Raycast(_player.Position() + new Vector3(_width, 100, _length), Vector3.down, out _hit))
                {
                    _position = _hit.point;
                }

                var _vehicle = GameObject.Instantiate(_vehiclePrefab, _position, Quaternion.identity);

                _width += 5;

                if (_width > 25)
                {
                    _width = 0;
                    _length += 5;
                }
            }
        }
    }
}