using System;
using System.Collections.Generic;
using UnityEngine;

namespace RiseOfWar
{
    [Serializable]
    public partial class WeaponModifications
    {
        public Modification[] modifications;
        public List<RegisteredWeaponModifications> possibleModifications = new List<RegisteredWeaponModifications>();

        public WeaponModifications(Modification bullet, Modification sights, Modification trigger, Modification spring, Modification barrel)
        {
            modifications = new Modification[] { bullet, sights, trigger, spring, barrel };
            possibleModifications = new List<RegisteredWeaponModifications>();
        }
        public WeaponModifications()
        {
            modifications = new Modification[] { null, null, null, null, null };
            possibleModifications = new List<RegisteredWeaponModifications>();
        }

        public Modification GetModification(WeaponModificationType type)
        {
            switch (type)
            {
                case WeaponModificationType.Bullet: return modifications[0];
                case WeaponModificationType.Sights: return modifications[1];
                case WeaponModificationType.Trigger: return modifications[2];
                case WeaponModificationType.Spring: return modifications[3];
                case WeaponModificationType.Barrel: return modifications[4];
                default: return modifications[0];
            }
        }
        public void SetModification(Modification modification, WeaponModificationType type)
        {
            int _index = 0;

            switch (type)
            {
                case WeaponModificationType.Bullet: _index = 0; break;
                case WeaponModificationType.Sights: _index = 1; break;
                case WeaponModificationType.Trigger: _index = 2; break;
                case WeaponModificationType.Spring: _index = 3; break;
                case WeaponModificationType.Barrel: _index = 4; break;
                default: _index = 0; break;
            }

            modifications[_index] = modification;
        }

        public float GetModifiedValue(float baseValue, string type)
        {
            float _output = 0;

            foreach (Modification _modification in modifications)
            {
                if (_modification == null)
                {
                    continue;
                }

                Debug.Log("modification has modifications (lol): " + _modification.modifications == null);
                Modification.Modifications _m = _modification.modifications;

                if (_m.HasParam(type))
                {
                    Param _param = _m.FindParam(type);
                    float _value = MathHelper.FloatHelper.Parse(_param.text);

                    if (_param.operation == Param.OPERATION_ADD)
                    {
                        _output += _value;
                    }
                    else if (_param.operation == Param.OPERATION_SUBSTRACTION)
                    {
                        _output -= _value;
                    }
                    else if (_param.operation == Param.OPERATION_MULTIPLICATION)
                    {
                        _output = baseValue * _value - baseValue;
                    }
                    else if (_param.operation == Param.OPERATION_DIVISION)
                    {
                        _output = baseValue / _value - baseValue;
                    }
                    else if (_param.operation == Param.OPERATION_ADDED_PERCENTAGE)
                    {
                        _output += _value * baseValue / 100;
                    }
                    else if (_param.operation == Param.OPERATION_SUBSTRACTED_PERCENTAGE)
                    {
                        _output -= _value * baseValue / 100;
                    }
                    else if (_param.operation == Param.OPERATION_OVERRIDE_PERCENTAGE)
                    {
                        _output = _value * baseValue / 100;
                    }
                }
            }

            return _output;
        }
    }
}