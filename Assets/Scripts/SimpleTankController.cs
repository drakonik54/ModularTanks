using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Простой контроллер танка для начальной настройки
/// Демонстрирует базовые принципы модульной архитектуры
/// </summary>
public class SimpleTankController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float turnSpeed = 50f;

    [Header("Компоненты танка (перетащите из иерархии)")]
    [SerializeField] private Transform hullTransform;     // Корпус танка
    [SerializeField] private Transform turretTransform;  // Башня
    [SerializeField] private Transform cannonTransform;  // Пушка

    [Header("Настройки башни")]
    [SerializeField] private float turretRotateSpeed = 30f;
    [SerializeField] private float cannonElevationSpeed = 20f;
    [SerializeField] private float minCannonAngle = -10f;
    [SerializeField] private float maxCannonAngle = 20f;

    // Системы ввода
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;

    // Компоненты
    private Rigidbody tankRigidbody;
    private Camera playerCamera;

    private void Awake()
    {
        InitializeComponents();
        SetupInput();
    }

    /// <summary>
    /// Инициализация компонентов танка
    /// Автоматически находит основные части, если не назначены вручную
    /// </summary>
    private void InitializeComponents()
    {
        // Получаем Rigidbody для физического движения
        tankRigidbody = GetComponent<Rigidbody>();
        if (tankRigidbody == null)
        {
            tankRigidbody = gameObject.AddComponent<Rigidbody>();
            tankRigidbody.mass = 5f; // Танк должен быть тяжелым
        }

        // Автопоиск компонентов, если не назначены
        if (hullTransform == null)
            hullTransform = FindChildByName("hull") ?? FindChildByName("TankLight_1");

        if (turretTransform == null)
            turretTransform = FindChildByName("turret") ?? FindChildByName("tower");

        if (cannonTransform == null && turretTransform != null)
            cannonTransform = FindChildInParent(turretTransform, "cannon") ??
                             FindChildInParent(turretTransform, "gun");

        playerCamera = Camera.main;
    }

    /// <summary>
    /// Настройка системы ввода Unity 6.0
    /// </summary>
    private void SetupInput()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
    }

    private void OnEnable() => inputActions?.Enable();
    private void OnDisable() => inputActions?.Disable();

    private void Update()
    {
        HandleMovement();
        HandleTurretControl();
    }

    /// <summary>
    /// Обработка движения танка
    /// Движение вперед/назад и поворот корпуса
    /// </summary>
    private void HandleMovement()
    {
        // Движение вперед/назад
        float moveAmount = moveInput.y * moveSpeed * Time.deltaTime;
        transform.Translate(0, 0, moveAmount);

        // Поворот корпуса
        float turnAmount = moveInput.x * turnSpeed * Time.deltaTime;
        transform.Rotate(0, turnAmount, 0);
    }

    /// <summary>
    /// Управление башней и пушкой
    /// Башня поворачивается по горизонтали, пушка наклоняется
    /// </summary>
    private void HandleTurretControl()
    {
        if (turretTransform == null) return;

        // Поворот башни по мыши (горизонталь)
        float turretRotation = lookInput.x * turretRotateSpeed * Time.deltaTime;
        turretTransform.Rotate(0, turretRotation, 0);

        // Наклон пушки (вертикаль)
        if (cannonTransform != null)
        {
            float elevationChange = -lookInput.y * cannonElevationSpeed * Time.deltaTime;
            Vector3 currentRotation = cannonTransform.localEulerAngles;
            float currentX = currentRotation.x > 180 ? currentRotation.x - 360 : currentRotation.x;
            float newX = Mathf.Clamp(currentX + elevationChange, minCannonAngle, maxCannonAngle);
            cannonTransform.localEulerAngles = new Vector3(newX, 0, 0);
        }
    }

    #region Input Callbacks
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    #endregion

    #region Utility Methods
    private Transform FindChildByName(string name)
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.name.ToLower().Contains(name.ToLower()))
                return child;
        }
        return null;
    }

    private Transform FindChildInParent(Transform parent, string name)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.name.ToLower().Contains(name.ToLower()))
                return child;
        }
        return null;
    }
    #endregion

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }
}
