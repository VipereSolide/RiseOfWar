using System;

using UnityEngine;

namespace RiseOfWar
{
    public class DroppedWeapon : MonoBehaviour
    {
        public WeaponManager.WeaponEntry prefab;

        public virtual void Pickup(Actor picker)
        {
            if (prefab != null)
            {
                picker.EquipNewWeaponEntry(prefab, 0, true);
            }

            Destroy(gameObject);
        }
    }
}