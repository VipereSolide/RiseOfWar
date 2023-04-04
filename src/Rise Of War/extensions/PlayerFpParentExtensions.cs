using System.Runtime.CompilerServices;
using UnityEngine;

namespace RiseOfWar
{
    public static class PlayerFpParentExtensions
    {
        private static readonly ConditionalWeakTable<PlayerFpParent, PlayerFpParentAdditionalData> _data = new ConditionalWeakTable<PlayerFpParent, PlayerFpParentAdditionalData>();

        public static PlayerFpParentAdditionalData GetAdditionalData(this PlayerFpParent _fpParent)
        {
            return _data.GetOrCreateValue(_fpParent);
        }

        public static void AddData(this PlayerFpParent _fpParent, PlayerFpParentAdditionalData _value)
        {
            try
            {
                _data.Add(_fpParent, _value);
            }
            catch (System.Exception _ex)
            {
                Plugin.LogError($"PlayerFpParentExtensions: Couldn't add data to player first person parent: " + _ex);
            }
        }

        public static void CreateRecoilAnchor(this PlayerFpParent __instance)
        {
            Transform _recoilAnchor = new GameObject("Recoil Anchor").transform;
            _recoilAnchor.SetParent(__instance.fpCameraRoot.parent, false);
            _recoilAnchor.ResetLocalTransform();

            __instance.GetAdditionalData().recoilAnchor = _recoilAnchor;
            __instance.fpCameraRoot.SetParent(_recoilAnchor);
        }

        public static void AddCameraRecoil(this PlayerFpParent _playerFpParent, float _upward, float _rightward, float _variance)
        {
            Plugin.Log($"PlayerFpParentExtensions: Received recoil. Righward {_rightward}; Upward {_upward}; Recoil variance {_variance}.");

            if (_playerFpParent.GetAdditionalData().recoilAnchor == null)
            {
                _playerFpParent.CreateRecoilAnchor();
            }

            Weapon _weapon = _playerFpParent.GetProperty<Actor>("actor").activeWeapon;

            if (_weapon != null)
            {
                WeaponAdditionalData _data = _weapon.GetAdditionalData();
                _upward += _data.modifications.GetModifiedValue(_upward, Modification.Modifications.CAMERA_RECOIL_UP);
                _rightward += _data.modifications.GetModifiedValue(_rightward, Modification.Modifications.CAMERA_RECOIL_RIGHT);
                _variance += _data.modifications.GetModifiedValue(_variance, Modification.Modifications.CAMERA_RECOIL_VARIANCE);
            }

            if (FpsActorController.instance.GetAdditionalData() != null)
            {
                foreach (var _badge in FpsActorController.instance.GetAdditionalData().playerBadges)
                {
                    _badge.Execute(_playerFpParent);
                }
            }

            _variance *= _playerFpParent.GetAdditionalData().horizontalRecoilMultiplier;
            _rightward *= _playerFpParent.GetAdditionalData().horizontalRecoilMultiplier;

            _rightward = _rightward + Random.Range(0, _variance);
            if (Random.value >= 0.5f)
            {
                _rightward *= -1f;
            }

            Vector2 _finalRecoil = new Vector2(_rightward, _upward);
            MouseController.instance.AddOffset(-_finalRecoil.y, _finalRecoil.x);
        }
    }
}