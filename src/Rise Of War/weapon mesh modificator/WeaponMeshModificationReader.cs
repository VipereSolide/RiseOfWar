using System.Collections.Generic;
using System.Collections;
using System;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar.WeaponMeshModificator
{
    public static class WeaponMeshModificationReader
    {
        public static void ApplyWeaponMeshModification(Weapon target, WeaponMeshModification meshModification)
        {
            //Plugin.Log("WeaponMeshModificationReader: Applying weapon mesh mod!");
            Transform _watchedTransform = target.transform;

            foreach (ModificationElement _rootElement in meshModification.Element)
            {
                A(_rootElement, _watchedTransform.Find(_rootElement.target));
            }
        }

        public static void A(ModificationElement e, Transform t)
        {
            //Plugin.Log("WeaponMeshModificationReader: Treating root element " + e.target);

            foreach (Param _p in e.Param)
            {
                foreach (ModificationType _m in WeaponMeshModification.WeaponMeshModificationTypes)
                {
                    string _t = _m.type.ToString().ToLower();

                    if (_p.name == _m.name && _p.type == _t)
                    {
                        InterpretActionForTransform(t, _m, _p);
                    }
                }
            }

            if (e.Element.Count == 0)
            {
                //Plugin.Log($"WeaponMeshModificationReader: No children!");
                return;
            }

            foreach (ModificationElement _c in e.Element)
            {
                A(_c, t.Find(_c.target));
            }
        }

        private static void InterpretActionForTransform(Transform target, ModificationType modification, Param param)
        {
            if (target == null)
            {
                return;
            }

            modification.callback.Invoke(target, param);
        }
    }
}