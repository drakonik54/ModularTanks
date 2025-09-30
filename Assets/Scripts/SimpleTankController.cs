using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ������� ���������� ����� ��� ��������� ���������
/// ������������� ������� �������� ��������� �����������
/// </summary>
public class SimpleTankController : MonoBehaviour
{
    [Header("��������� ��������")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float turnSpeed = 50f;

    [Header("���������� ����� (���������� �� ��������)")]
    [SerializeField] private Transform hullTransform;     // ������ �����
    [SerializeField] private Transform turretTransform;  // �����
    [SerializeField] private Transform cannonTransform;  // �����

    [Header("��������� �����")]
    [SerializeField] private float turretRotateSpeed = 30f;
    [SerializeField] private float cannonElevationSpeed = 20f;
    [SerializeField] private float minCannonAngle = -10f;
    [SerializeField] private float maxCannonAngle = 20f;

    // ������� �����
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;

    // ����������
    private Rigidbody tankRigidbody;
    private Camera playerCamera;

    private void Awake()
    {
        InitializeComponents();
        SetupInput();
    }

    /// <summary>
    /// ������������� ����������� �����
    /// ������������� ������� �������� �����, ���� �� ��������� �������
    /// </summary>
    private void InitializeComponents()
    {
        // �������� Rigidbody ��� ����������� ��������
        tankRigidbody = GetComponent<Rigidbody>();
        if (tankRigidbody == null)
        {
            tankRigidbody = gameObject.AddComponent<Rigidbody>();
            tankRigidbody.mass = 5f; // ���� ������ ���� �������
        }

        // ��������� �����������, ���� �� ���������
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
    /// ��������� ������� ����� Unity 6.0
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
    /// ��������� �������� �����
    /// �������� ������/����� � ������� �������
    /// </summary>
    private void HandleMovement()
    {
        // �������� ������/�����
        float moveAmount = moveInput.y * moveSpeed * Time.deltaTime;
        transform.Translate(0, 0, moveAmount);

        // ������� �������
        float turnAmount = moveInput.x * turnSpeed * Time.deltaTime;
        transform.Rotate(0, turnAmount, 0);
    }

    /// <summary>
    /// ���������� ������ � ������
    /// ����� �������������� �� �����������, ����� �����������
    /// </summary>
    private void HandleTurretControl()
    {
        if (turretTransform == null) return;

        // ������� ����� �� ���� (�����������)
        float turretRotation = lookInput.x * turretRotateSpeed * Time.deltaTime;
        turretTransform.Rotate(0, turretRotation, 0);

        // ������ ����� (���������)
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
