using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Скрипт управления танком с модульной архитектурой
/// Обеспечивает движение, поворот башни и анимацию ходовой части
/// </summary>
public class TankController : MonoBehaviour
{
    [Header("Движение танка")]
    [SerializeField, Range(0f, 50f)] private float moveSpeed = 10f;
    [SerializeField, Range(0f, 180f)] private float turnSpeed = 60f;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Компоненты танка")]
    [SerializeField] private Transform hull; // Корпус танка
    [SerializeField] private Transform turret; // Башня
    [SerializeField] private Transform cannon; // Пушка

    [Header("Ходовая часть")]
    [SerializeField] private Transform[] leftWheels = new Transform[4]; // Катки слева
    [SerializeField] private Transform[] rightWheels = new Transform[4]; // Катки справа
    [SerializeField] private Transform leftTrack; // Левая гусеница
    [SerializeField] private Transform rightTrack; // Правая гусеница
    [SerializeField, Range(0f, 1000f)] private float wheelRotationSpeed = 360f;
    [SerializeField, Range(0f, 10f)] private float trackScrollSpeed = 2f;

    [Header("Башня и пушка")]
    [SerializeField, Range(0f, 180f)] private float turretRotationSpeed = 45f;
    [SerializeField, Range(-20f, 30f)] private float cannonMinAngle = -10f;
    [SerializeField, Range(-20f, 30f)] private float cannonMaxAngle = 20f;
    [SerializeField, Range(0f, 90f)] private float cannonRotationSpeed = 30f;

    [Header("Физика")]
    [SerializeField] private Rigidbody tankRigidbody;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField, Range(0f, 1f)] private float groundCheckDistance = 1.1f;

    // Инпут система Unity 6.0
    private InputSystem_Actions inputActions;
    private Vector2 movementInput;
    private Vector2 turretInput;
    private float cannonInput;

    // Переменные для анимации
    private float currentSpeed;
    private float targetSpeed;
    private float currentTurnSpeed;
    private Vector3 lastPosition;
    private Material leftTrackMaterial;
    private Material rightTrackMaterial;
    private float trackOffset;

    // Кэшированные компоненты
    private Camera playerCamera;
    private AudioSource engineAudioSource;

    private void Awake()
    {
        InitializeComponents();
        SetupInputSystem();
        CacheTrackMaterials();
    }

    /// <summary>
    /// Инициализация компонентов танка
    /// Автоматически находит необходимые компоненты если они не назначены в инспекторе
    /// </summary>
    private void InitializeComponents()
    {
        // Автоматический поиск компонентов если не назначены
        if (tankRigidbody == null)
            tankRigidbody = GetComponent<Rigidbody>();

        if (hull == null)
            hull = transform.Find("Hull") ?? transform.Find("TankLight_1");

        if (turret == null)
            turret = transform.Find("Hull/Turret") ?? transform.Find("Turret") ?? FindDeepChild("turret");

        if (cannon == null && turret != null)
            cannon = turret.Find("Cannon") ?? FindDeepChild("cannon") ?? FindDeepChild("gun");

        // Поиск аудио компонента для звука двигателя
        engineAudioSource = GetComponent<AudioSource>();

        // Поиск камеры игрока
        playerCamera = Camera.main ?? FindObjectOfType<Camera>();

        lastPosition = transform.position;

        // Автопоиск катков и гусениц
        AutoFindWheelsAndTracks();
    }

    /// <summary>
    /// Автоматический поиск катков и гусениц в модели танка
    /// </summary>
    private void AutoFindWheelsAndTracks()
    {
        // Поиск всех дочерних объектов с названиями, содержащими wheel, track, road
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        int leftWheelIndex = 0;
        int rightWheelIndex = 0;

        foreach (Transform child in allChildren)
        {
            string name = child.name.ToLower();

            // Поиск катков
            if ((name.Contains("wheel") || name.Contains("road") || name.Contains("roller")) &&
                !name.Contains("drive") && !name.Contains("idler"))
            {
                if (name.Contains("left") || name.Contains("l_") || child.position.x < transform.position.x)
                {
                    if (leftWheelIndex < leftWheels.Length)
                        leftWheels[leftWheelIndex++] = child;
                }
                else if (name.Contains("right") || name.Contains("r_") || child.position.x > transform.position.x)
                {
                    if (rightWheelIndex < rightWheels.Length)
                        rightWheels[rightWheelIndex++] = child;
                }
            }

            // Поиск гусениц
            if (name.Contains("track") && !name.Contains("wheel"))
            {
                if (name.Contains("left") || name.Contains("l_") || child.position.x < transform.position.x)
                {
                    if (leftTrack == null) leftTrack = child;
                }
                else if (name.Contains("right") || name.Contains("r_") || child.position.x > transform.position.x)
                {
                    if (rightTrack == null) rightTrack = child;
                }
            }
        }
    }

    /// <summary>
    /// Поиск дочернего объекта по имени в глубину
    /// </summary>
    private Transform FindDeepChild(string name)
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.name.ToLower().Contains(name.ToLower()))
                return child;
        }
        return null;
    }

    /// <summary>
    /// Настройка новой Input System Unity 6.0
    /// Использует InputSystem_Actions для обработки ввода
    /// </summary>
    private void SetupInputSystem()
    {
        inputActions = new InputSystem_Actions();

        // Подписка на события движения
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        // Подписка на события управления башней (мышь)
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        // Временно используем Interact для управления пушкой
        inputActions.Player.Interact.performed += ctx => cannonInput = 1f;
        inputActions.Player.Interact.canceled += ctx => cannonInput = 0f;
        inputActions.Player.Crouch.performed += ctx => cannonInput = -1f;
        inputActions.Player.Crouch.canceled += ctx => cannonInput = 0f;
    }

    /// <summary>
    /// Кэширование материалов гусениц для анимации текстур
    /// Это нужно для создания эффекта движения гусениц
    /// </summary>
    private void CacheTrackMaterials()
    {
        if (leftTrack != null)
        {
            var renderer = leftTrack.GetComponent<Renderer>();
            if (renderer != null)
                leftTrackMaterial = renderer.material;
        }

        if (rightTrack != null)
        {
            var renderer = rightTrack.GetComponent<Renderer>();
            if (renderer != null)
                rightTrackMaterial = renderer.material;
        }
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleTurretRotation();
        HandleCannonElevation();
        UpdateWheelAnimation();
        UpdateTrackAnimation();
        UpdateEngineSound();
    }

    #region Input Handlers

    private void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        turretInput = context.ReadValue<Vector2>();
    }

    #endregion

    #region Movement System

    /// <summary>
    /// Обработка движения танка
    /// Использует физическую модель для реалистичного поведения
    /// </summary>
    private void HandleMovement()
    {
        // Плавное изменение скорости с использованием кривой ускорения
        targetSpeed = movementInput.y * moveSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed,
            accelerationCurve.Evaluate(Time.deltaTime * 3f));

        // Поворот танка
        currentTurnSpeed = movementInput.x * turnSpeed * Time.deltaTime;

        // Применение движения через Rigidbody для физического взаимодействия
        if (tankRigidbody != null)
        {
            Vector3 moveDirection = transform.forward * currentSpeed * Time.deltaTime;
            tankRigidbody.MovePosition(tankRigidbody.position + moveDirection);

            if (Mathf.Abs(currentTurnSpeed) > 0.01f)
            {
                Quaternion turnRotation = Quaternion.Euler(0, currentTurnSpeed, 0);
                tankRigidbody.MoveRotation(tankRigidbody.rotation * turnRotation);
            }
        }
        else
        {
            // Fallback для случая без Rigidbody
            transform.Translate(0, 0, currentSpeed * Time.deltaTime);
            transform.Rotate(0, currentTurnSpeed, 0);
        }
    }

    #endregion

    #region Turret Control

    /// <summary>
    /// Управление поворотом башни по мыши
    /// Башня поворачивается независимо от корпуса танка
    /// </summary>
    private void HandleTurretRotation()
    {
        if (turret == null) return;

        float mouseX = turretInput.x * turretRotationSpeed * Time.deltaTime;
        turret.Rotate(0, mouseX, 0);
    }

    /// <summary>
    /// Управление углом наклона пушки
    /// Ограничивает углы наклона в реалистичных пределах
    /// </summary>
    private void HandleCannonElevation()
    {
        if (cannon == null) return;

        float elevationChange = cannonInput * cannonRotationSpeed * Time.deltaTime;
        Vector3 currentRotation = cannon.localEulerAngles;

        // Нормализация углов для корректной работы ограничений
        float currentX = currentRotation.x;
        if (currentX > 180f) currentX -= 360f;

        float newX = Mathf.Clamp(currentX + elevationChange, cannonMinAngle, cannonMaxAngle);
        cannon.localEulerAngles = new Vector3(newX, currentRotation.y, currentRotation.z);
    }

    #endregion

    #region Track Animation

    /// <summary>
    /// Анимация вращения катков
    /// Катки вращаются в зависимости от скорости движения танка
    /// </summary>
    private void UpdateWheelAnimation()
    {
        float wheelRotation = currentSpeed * wheelRotationSpeed * Time.deltaTime;

        // Анимация левых катков
        foreach (Transform wheel in leftWheels)
        {
            if (wheel != null)
                wheel.Rotate(wheelRotation, 0, 0);
        }

        // Анимация правых катков
        foreach (Transform wheel in rightWheels)
        {
            if (wheel != null)
                wheel.Rotate(wheelRotation, 0, 0);
        }
    }

    /// <summary>
    /// Анимация движения текстуры гусениц
    /// Создает эффект движущихся гусениц через смещение UV координат
    /// </summary>
    private void UpdateTrackAnimation()
    {
        if (Mathf.Abs(currentSpeed) < 0.1f) return;

        trackOffset += currentSpeed * trackScrollSpeed * Time.deltaTime;

        // Применение смещения к материалам гусениц
        if (leftTrackMaterial != null)
            leftTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, trackOffset));

        if (rightTrackMaterial != null)
            rightTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, trackOffset));
    }

    #endregion

    #region Audio System

    /// <summary>
    /// Обновление звука двигателя
    /// Изменяет высоту и громкость звука в зависимости от скорости
    /// </summary>
    private void UpdateEngineSound()
    {
        if (engineAudioSource == null) return;

        float speedNormalized = Mathf.Abs(currentSpeed) / moveSpeed;
        engineAudioSource.pitch = Mathf.Lerp(0.8f, 1.5f, speedNormalized);
        engineAudioSource.volume = Mathf.Lerp(0.3f, 1f, speedNormalized);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Проверка контакта с землей
    /// Полезно для определения возможности движения
    /// </summary>
    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    /// <summary>
    /// Получение текущей скорости танка
    /// Может использоваться другими системами (UI, эффекты)
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    /// <summary>
    /// Получение направления башни в мировых координатах
    /// Полезно для системы стрельбы
    /// </summary>
    public Vector3 GetTurretForward()
    {
        return turret != null ? turret.forward : transform.forward;
    }

    /// <summary>
    /// Получение позиции дула пушки
    /// Используется для определения точки появления снарядов
    /// </summary>
    public Vector3 GetCannonMuzzlePosition()
    {
        if (cannon != null)
        {
            // Поиск объекта MuzzlePoint или использование конца пушки
            Transform muzzle = cannon.Find("MuzzlePoint");
            return muzzle != null ? muzzle.position : cannon.position + cannon.forward * 2f;
        }
        return transform.position + transform.forward * 3f;
    }

    #endregion

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Визуализация в редакторе
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);

        if (turret != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(turret.position, turret.forward * 5f);
        }
    }
#endif
}
