using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/*
 * CONTROLADOR DE PERSONAJE EN 3D + ANIMACIÓN 2D DIRECCIONAL
 * ---------------------------------------------------------
 * Este script mantiene el sistema de movimiento del Starter Assets,
 * pero amplía el control del Animator para personajes 2D/3D híbridos.
 *
 * ¿Qué agrega?
 * - Detecta si el personaje está quieto o en movimiento.
 * - Detecta dirección visual para animaciones:
 *      W  -> atrás
 *      S  -> frente
 *      A/D -> lado
 * - Guarda la última dirección para que el idle correcto permanezca
 *   al soltar las teclas.
 * - Guarda si el lado actual es izquierda o derecha para usar mirror.
 *
 * IMPORTANTE:
 * Este sistema está pensado para que el personaje inicie en IDLE FRENTE.
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioSource AudioFootsteps;
        public AudioSource LandingAudio;
        public AudioSource AudioFoley;
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // Cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // Movimiento
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // Timeouts
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // IDs Animator - originales
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        // IDs Animator - nuevos para animación direccional
        private int _animIDIsMoving;
        private int _animIDFacingVertical;
        private int _animIDFacingHorizontal;
        private int _animIDFacingRight;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;
        private bool _hasAnimator;

        /*
         * DIRECCIÓN VISUAL RECORDADA
         * --------------------------
         * FacingVertical:
         *      1  = atrás
         *      0  = lado
         *     -1  = frente
         *
         * FacingHorizontal:
         *      1  = derecha
         *     -1  = izquierda
         *      0  = sin dirección lateral actual
         *
         * Iniciamos en frente porque tú lo pediste.
         */
        private int _facingVertical = -1;
        private int _facingHorizontal = 0;
        private bool _facingRight = true;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            // Busca la cámara principal si no fue asignada.
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // Valores iniciales del estado de animación
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            AplicarDireccionInicialAnimator();
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
            UpdateDirectionalAnimationData();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        /// <summary>
        /// Guarda los hashes de todos los parámetros que vamos a usar.
        /// </summary>
        private void AssignAnimationIDs()
        {
            // Originales
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

            // Nuevos
            _animIDIsMoving = Animator.StringToHash("IsMoving");
            _animIDFacingVertical = Animator.StringToHash("FacingVertical");
            _animIDFacingHorizontal = Animator.StringToHash("FacingHorizontal");
            _animIDFacingRight = Animator.StringToHash("FacingRight");
        }

        /// <summary>
        /// Aplica al Animator la orientación inicial.
        /// Como debe iniciar en idle frontal:
        /// FacingVertical = -1
        /// FacingHorizontal = 0
        /// FacingRight = true
        /// IsMoving = false
        /// </summary>
        private void AplicarDireccionInicialAnimator()
        {
            if (!_hasAnimator) return;

            _animator.SetBool(_animIDIsMoving, false);
            _animator.SetInteger(_animIDFacingVertical, _facingVertical);
            _animator.SetInteger(_animIDFacingHorizontal, _facingHorizontal);
            _animator.SetBool(_animIDFacingRight, _facingRight);
        }

        /// <summary>
        /// Revisa si el personaje está tocando el suelo.
        /// </summary>
        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            );

            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        /// <summary>
        /// Controla la rotación de la cámara.
        /// </summary>
        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f
            );
        }

        /// <summary>
        /// Movimiento principal del personaje.
        /// Aquí no cambiamos la lógica base del Starter Assets.
        /// </summary>
        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero)
                targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(
                _controller.velocity.x,
                0.0f,
                _controller.velocity.z
            ).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate
                );

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f)
                _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // Mantengo tu rotación de movimiento por ahora.
            // Más adelante, si hace conflicto con el billboard,
            // la separamos o la desactivamos.
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetRotation,
                    ref _rotationVelocity,
                    RotationSmoothTime
                );

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(
                targetDirection.normalized * (_speed * Time.deltaTime) +
                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        /// <summary>
        /// Calcula y manda al Animator la dirección visual:
        /// frente, atrás o lado; además de izquierda o derecha.
        ///
        /// IMPORTANTE:
        /// Esto se basa en el INPUT, no en la rotación del personaje.
        /// Porque tu animación 2D debe responder a W/S/A/D visualmente.
        /// </summary>
        private void UpdateDirectionalAnimationData()
        {
            Vector2 moveInput = _input.move;
            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            if (isMoving)
            {
                float absX = Mathf.Abs(moveInput.x);
                float absY = Mathf.Abs(moveInput.y);

                // Si domina el eje vertical, usamos frente/atrás.
                if (absY > absX)
                {
                    if (moveInput.y > 0f)
                    {
                        // W = animación de atrás
                        _facingVertical = 1;
                        _facingHorizontal = 0;
                    }
                    else if (moveInput.y < 0f)
                    {
                        // S = animación de frente
                        _facingVertical = -1;
                        _facingHorizontal = 0;
                    }
                }
                else
                {
                    // A o D = animación lateral
                    _facingVertical = 0;

                    if (moveInput.x > 0f)
                    {
                        _facingHorizontal = 1;
                        _facingRight = true;
                    }
                    else if (moveInput.x < 0f)
                    {
                        _facingHorizontal = -1;
                        _facingRight = false;
                    }
                }
            }

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDIsMoving, isMoving);
                _animator.SetInteger(_animIDFacingVertical, _facingVertical);
                _animator.SetInteger(_animIDFacingHorizontal, _facingHorizontal);
                _animator.SetBool(_animIDFacingRight, _facingRight);
            }
        }

        /// <summary>
        /// Maneja salto y gravedad.
        /// Conserva la lógica base del script original.
        /// </summary>
        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;

            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius
            );
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (AudioFootsteps != null)
                    AudioFootsteps.Play();

                if (AudioFoley != null)
                    AudioFoley.Play();
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (LandingAudio != null)
                    LandingAudio.Play();
            }
        }
    }
}