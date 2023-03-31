using System.Collections.Generic;
using System.Collections;
using System;

using UnityEngine;

namespace RiseOfWar
{
    public static class DroppedWeaponRegistry
    {
        public static List<DroppedWeapon> droppedWeapons = new List<DroppedWeapon>();

        public static DroppedWeapon GetClosestActiveDroppedWeapon()
        {
            foreach(DroppedWeapon _weapon in droppedWeapons)
            {
                if (_weapon.isPlayerInPickupRadius)
                {
                    return _weapon;
                }
            }

            return null;
        }
    }
}