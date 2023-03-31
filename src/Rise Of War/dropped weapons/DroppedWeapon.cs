using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    public class DroppedWeapon : MonoBehaviour
    {
        public WeaponManager.WeaponEntry prefab;
        public Weapon.Configuration configuration;

        public int currentAmmo;
        public int currentSpareAmmo;
        public WeaponModifications modifications;

        public Dictionary<string, object> settings = new Dictionary<string, object>();
        private Weapon _weapon;

        private float _currentLifetime = 0;
        private bool _startedTingling = false;
        private bool _isVisible = true;

        private bool _isPlayerInPickupRadius;
        public bool isPlayerInPickupRadius { get { return _isPlayerInPickupRadius; } }

        private void Start()
        {
            _isVisible = true;
            DroppedWeaponRegistry.droppedWeapons.Add(this);
        }

        private void Update()
        {
            ManageLifetime();
            ManageVisibility();
            ManagePlayerRadius();
        }

        private void ManagePlayerRadius()
        {
            Actor _player = ActorManager.instance.player;
            bool _isCloseEnough = Vector3.Distance(transform.position, _player.Position()) <= GameConfiguration.weaponPickupDistance;

            if (!_isCloseEnough)
            {
                _isPlayerInPickupRadius = false;
                return;
            }

            bool _isPlayerVisible = false;
            RaycastHit _hit;

            if (Physics.Raycast(transform.position, transform.position - _player.Position(), out _hit))
            {
                Actor _potentialPlayer = _hit.transform.GetComponent<Actor>();
                if (_potentialPlayer == _player)
                {
                    _isPlayerVisible = true;
                }
                else
                {
                    _potentialPlayer = _hit.transform.GetComponentInParent<Actor>();
                    if (_potentialPlayer == _player)
                    {
                        _isPlayerVisible = true;
                    }
                }
            }

            _isPlayerInPickupRadius = _isPlayerVisible;
        }

        private void ManageVisibility()
        {
            foreach (Transform _children in transform)
            {
                if (_children == null)
                {
                    continue;
                }

                _children.gameObject.SetActive(_isVisible);
            }
        }

        private void ManageLifetime()
        {
            _currentLifetime += Time.deltaTime;

            if (_currentLifetime > GameConfiguration.droppedWeaponLifetime - 5 && !_startedTingling)
            {
                _startedTingling = true;
                InvokeRepeating(nameof(ToggleVisibility), 0, 0.25f);
            }

            if (_currentLifetime >= GameConfiguration.droppedWeaponLifetime)
            {
                Destroy();
            }
        }

        public void Destroy()
        {
            DroppedWeaponRegistry.droppedWeapons.Remove(this);
            Destroy(gameObject);
        }

        private void ToggleVisibility()
        {
            _isVisible = !_isVisible;
        }

        public virtual void Pickup(Actor picker)
        {
            if (prefab != null)
            {
                _weapon = picker.EquipNewWeaponEntry(prefab, 0, true);
                _weapon.configuration = configuration;
                _weapon.ResetSetup();

                Invoke(nameof(ApplyWeaponSpecs), 0.1f);
            }

            Destroy();
        }

        private void ApplyWeaponSpecs()
        {
            Plugin.Log("DroppedWeapon: Setting up correct weapon stats...");

            _weapon.ammo = currentAmmo;
            _weapon.spareAmmo = currentSpareAmmo;
            _weapon.GetAdditionalData().modifications = modifications;

            Plugin.Log("DroppedWeapon: Setted up correct weapon stats.");
        }
    }
}