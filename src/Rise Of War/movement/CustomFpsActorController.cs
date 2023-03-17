using System;
using UnityEngine;
using HarmonyLib;
using UnityStandardAssets.Characters.FirstPerson;

namespace RiseOfWar
{
    public class CustomFpsActorController : MonoBehaviour
    {
        private CharacterController _controller;
        private FpsActorController _fpsActorController;

        private float _localGravity = -9.81f;
        private Vector2 _input = new Vector2(0, 0);
        private Vector3 _velocity = new Vector3(0, 0, 0);
        private float _currentSpeed = 0;

        private void Start()
        {
        }

        private void Update()
        {
            GetInputs();
            GravityVelocity();
            CalculateCurrentSpeed();
            CalculateInputVelocity();
            GetPlayerFinalVelocity();
        }

        public void Init(FpsActorController _actorController, CharacterController _characterController)
        {
            _fpsActorController = _actorController;
            _controller = _characterController;
        }

        private void CalculateCurrentSpeed()
        {
            if (_fpsActorController.IsSprinting())
            {
                _currentSpeed = GameConfiguration.sprintingSpeed;
                return;
            }

            _currentSpeed = GameConfiguration.walkingSpeed;
        }

        private void CalculateInputVelocity()
        {
            _velocity = (transform.right * _input.x) + (transform.forward * _input.y);
            _velocity = Vector3.ClampMagnitude(_velocity, 1);
        }

        private void GravityVelocity()
        {
            _velocity.SetY(_localGravity);

            if (_velocity.y < 2 && _controller.isGrounded)
            {
                _velocity.y = 0;
            }
        }

        private void GetInputs()
        {
            _input = new Vector2(SteelInput.GetAxis(SteelInput.KeyBinds.Horizontal), SteelInput.GetAxis(SteelInput.KeyBinds.Vertical));
        }
    
        private void GetPlayerFinalVelocity()
        {
            _velocity = _velocity * Time.deltaTime * _currentSpeed;
        }
    }
}