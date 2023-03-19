using System;

using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace RiseOfWar
{
    public class MouseController : MonoBehaviour
    {
        public static MouseController instance;

        protected PlayerFpParent _owner;
        protected FpsActorController _controller;

        public PlayerFpParent owner
        {
            get { return _owner; }
        }
        public FpsActorController controller
        {
            get { return _controller; }
        }

        protected Vector2 _input;
        protected Vector2 _current;
        protected Vector2 _addedOffset;
        protected Vector2 _currentAdded;

        public void SetMouseInput(float _horizontal, float _vertical)
        {
            _input.x = _horizontal;
            _input.y = _vertical;
        }
        public void AddOffset(float _horizontal, float _vertical)
        {
            _addedOffset.x += _vertical;
            _addedOffset.y += _horizontal;
        }
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }

            instance = this;
        }
        public void Init(PlayerFpParent _owner, FpsActorController _controller)
        {
            this._owner = _owner;
            this._controller = _controller;
        }
        protected float GetSensitivity()
        {
            Actor _player = ActorManager.instance.player;

            if (_player != null && _player.activeWeapon != null && _player.activeWeapon.aiming && _player.activeWeapon.weaponProperties() != null)
            {
                float _aimFov = _player.activeWeapon.weaponProperties().aiming.GetFloat(WeaponXMLProperties.Aiming.AIM_FOV);

                if (_aimFov > 0)
                {
                    return _aimFov * _controller.mouseSensitivity / GameConfiguration.defaultAimingFieldOfView;
                }
            }

            return _controller.mouseSensitivity;
        }
        private void Update()
        {
            _currentAdded = Vector2.Lerp(_currentAdded, _addedOffset, Time.deltaTime * GameConfiguration.cameraRecoilSmoothness);

            if (MouseLook.paused)
            {
                return;
            }

            _current.y -= GetSensitivity() * _input.y;
            _current.x += GetSensitivity() * _input.x;

            _current.y = Mathf.Clamp(_current.y, -75, 75 - _currentAdded.y);

            _owner.fpCameraRoot.localEulerAngles = new Vector3(_current.y + _currentAdded.y, 0, 0);
            _owner.fpCameraRoot.parent.localEulerAngles = new Vector3(0, _current.x + _currentAdded.x, 0);
        }
    }
}