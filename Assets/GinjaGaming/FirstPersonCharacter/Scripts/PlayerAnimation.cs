using SpaceGame.StateMachine.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GinjaGaming.FirstPersonCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        private PlayerController _playerController;
        private PlayerState _playerState;
        private Animator _animator;

        [SerializeField] private float animationBlendSpeed = 0.015f;

        private Vector3 _currentBlendTreeVelocity = Vector3.zero;

        public static int _velocityX = Animator.StringToHash("velocityX");
        public static int _velocityY = Animator.StringToHash("velocityY");
        public static int _inputMagnitudeClamped = Animator.StringToHash("inputMagnitudeClamped");
        public static int _rotationMismatch = Animator.StringToHash("rotationMismatch");
        public static int _isRotatingToTarget = Animator.StringToHash("isRotatingToTarget");
        public static int _isIdling = Animator.StringToHash("isIdling");
        public static int _isWalking = Animator.StringToHash("isWalking");
        public static int _isSprinting = Animator.StringToHash("isSprinting");
        public static int _isGrounded = Animator.StringToHash("isGrounded");
        public static int _isJumping = Animator.StringToHash("isJumping");
        public static int _isFalling = Animator.StringToHash("isFalling");

        private void Start()
        {
            _playerController = GetComponent<PlayerController>();
            _animator = GetComponent<Animator>();
            _playerState = GetComponent<PlayerState>();
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;
            bool isRunning = _playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
            bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
            bool isGrounded = _playerState.InGroundedState();

            _animator.SetBool(_isGrounded, isGrounded);
            _animator.SetBool(_isIdling, isIdling);
            _animator.SetBool(_isFalling, isFalling);
            _animator.SetBool(_isJumping, isJumping);

            if (isGrounded)
            {
                Vector2 inputTarget = isSprinting ? _playerController.MovementInput * 2 :
                                      isRunning ? _playerController.MovementInput * 1 : _playerController.MovementInput * 0.5f;

                _currentBlendTreeVelocity = Vector3.Lerp(_currentBlendTreeVelocity, inputTarget, animationBlendSpeed);
                float inputMagnitude = Mathf.Max(Mathf.Abs(_currentBlendTreeVelocity.x), Mathf.Abs(_currentBlendTreeVelocity.y));

                _animator.SetFloat(_velocityX, _currentBlendTreeVelocity.x);
                _animator.SetFloat(_velocityY, _currentBlendTreeVelocity.y);
                _animator.SetFloat(_inputMagnitudeClamped, inputMagnitude);
                _animator.SetFloat(_rotationMismatch, _playerController.RotationMismatch);
                _animator.SetBool(_isRotatingToTarget, _playerController.IsRotatingToTarget);
            }
        }
    }
}
