using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// ������ ���������� ������ � ��������� ������������
/// ������������ ��������, ������� ����� � �������� ������� �����
/// </summary>
public class TankController : MonoBehaviour
{
    [Header("�������� �����")]
    [SerializeField, Range(0f, 50f)] private float moveSpeed = 10f;
    [SerializeField, Range(0f, 180f)] private float turnSpeed = 60f;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("���������� �����")]
    [SerializeField] private Transform hull; // ������ �����
    [SerializeField] private Transform turret; // �����
    [SerializeField] private Transform cannon; // �����

    [Header("������� �����")]
    [SerializeField] private Transform[] leftWheels = new Transform[4]; // ����� �����
    [SerializeField] private Transform[] rightWheels = new Transform[4]; // ����� ������
    [SerializeField] private Transform leftTrack; // ����� ��������
    [SerializeField] private Transform rightTrack; // ������ ��������
    [SerializeField, Range(0f, 1000f)] private float wheelRotationSpeed = 360f;
    [SerializeField, Range(0f, 10f)] private float trackScrollSpeed = 2f;

    [Header("����� � �����")]
    [SerializeField, Range(0f, 180f)] private float turretRotationSpeed = 45f;
    [SerializeField, Range(-20f, 30f)] private float cannonMinAngle = -10f;
    [SerializeField, Range(-20f, 30f)] private float cannonMaxAngle = 20f;
    [SerializeField, Range(0f, 90f)] private float cannonRotationSpeed = 30f;

    [Header("������")]
    [SerializeField] private Rigidbody tankRigidbody;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField, Range(0f, 1f)] private float groundCheckDistance = 1.1f;

    // ����� ������� Unity 6.0
    private InputSystem_Actions inputActions;
    private Vector2 movementInput;
    private Vector2 turretInput;
    private float cannonInput;

    // ���������� ��� ��������
    private float currentSpeed;
    private float targetSpeed;
    private float currentTurnSpeed;
    private Vector3 lastPosition;
    private Material leftTrackMaterial;
    private Material rightTrackMaterial;
    private float trackOffset;

    // ������������ ����������
    private Camera playerCamera;
    private AudioSource engineAudioSource;

    private void Awake()
    {
        InitializeComponents();
        SetupInputSystem();
        CacheTrackMaterials();
    }

    /// <summary>
    /// ������������� ����������� �����
    /// ������������� ������� ����������� ���������� ���� ��� �� ��������� � ����������
    /// </summary>
    private void InitializeComponents()
    {
        // �������������� ����� ����������� ���� �� ���������
        if (tankRigidbody == null)
            tankRigidbody = GetComponent<Rigidbody>();

        if (hull == null)
            hull = transform.Find("Hull") ?? transform.Find("TankLight_1");

        if (turret == null)
            turret = transform.Find("Hull/Turret") ?? transform.Find("Turret") ?? FindDeepChild("turret");

        if (cannon == null && turret != null)
            cannon = turret.Find("Cannon") ?? FindDeepChild("cannon") ?? FindDeepChild("gun");

        // ����� ����� ���������� ��� ����� ���������
        engineAudioSource = GetComponent<AudioSource>();

        // ����� ������ ������
        playerCamera = Camera.main ?? FindObjectOfType<Camera>();

        lastPosition = transform.position;

        // ��������� ������ � �������
        AutoFindWheelsAndTracks();
    }

    /// <summary>
    /// �������������� ����� ������ � ������� � ������ �����
    /// </summary>
    private void AutoFindWheelsAndTracks()
    {
        // ����� ���� �������� �������� � ����������, ����������� wheel, track, road
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        int leftWheelIndex = 0;
        int rightWheelIndex = 0;

        foreach (Transform child in allChildren)
        {
            string name = child.name.ToLower();

            // ����� ������
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

            // ����� �������
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
    /// ����� ��������� ������� �� ����� � �������
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
    /// ��������� ����� Input System Unity 6.0
    /// ���������� InputSystem_Actions ��� ��������� �����
    /// </summary>
    private void SetupInputSystem()
    {
        inputActions = new InputSystem_Actions();

        // �������� �� ������� ��������
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        // �������� �� ������� ���������� ������ (����)
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        // �������� ���������� Interact ��� ���������� ������
        inputActions.Player.Interact.performed += ctx => cannonInput = 1f;
        inputActions.Player.Interact.canceled += ctx => cannonInput = 0f;
        inputActions.Player.Crouch.performed += ctx => cannonInput = -1f;
        inputActions.Player.Crouch.canceled += ctx => cannonInput = 0f;
    }

    /// <summary>
    /// ����������� ���������� ������� ��� �������� �������
    /// ��� ����� ��� �������� ������� �������� �������
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
    /// ��������� �������� �����
    /// ���������� ���������� ������ ��� ������������� ���������
    /// </summary>
    private void HandleMovement()
    {
        // ������� ��������� �������� � �������������� ������ ���������
        targetSpeed = movementInput.y * moveSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed,
            accelerationCurve.Evaluate(Time.deltaTime * 3f));

        // ������� �����
        currentTurnSpeed = movementInput.x * turnSpeed * Time.deltaTime;

        // ���������� �������� ����� Rigidbody ��� ����������� ��������������
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
            // Fallback ��� ������ ��� Rigidbody
            transform.Translate(0, 0, currentSpeed * Time.deltaTime);
            transform.Rotate(0, currentTurnSpeed, 0);
        }
    }

    #endregion

    #region Turret Control

    /// <summary>
    /// ���������� ��������� ����� �� ����
    /// ����� �������������� ���������� �� ������� �����
    /// </summary>
    private void HandleTurretRotation()
    {
        if (turret == null) return;

        float mouseX = turretInput.x * turretRotationSpeed * Time.deltaTime;
        turret.Rotate(0, mouseX, 0);
    }

    /// <summary>
    /// ���������� ����� ������� �����
    /// ������������ ���� ������� � ������������ ��������
    /// </summary>
    private void HandleCannonElevation()
    {
        if (cannon == null) return;

        float elevationChange = cannonInput * cannonRotationSpeed * Time.deltaTime;
        Vector3 currentRotation = cannon.localEulerAngles;

        // ������������ ����� ��� ���������� ������ �����������
        float currentX = currentRotation.x;
        if (currentX > 180f) currentX -= 360f;

        float newX = Mathf.Clamp(currentX + elevationChange, cannonMinAngle, cannonMaxAngle);
        cannon.localEulerAngles = new Vector3(newX, currentRotation.y, currentRotation.z);
    }

    #endregion

    #region Track Animation

    /// <summary>
    /// �������� �������� ������
    /// ����� ��������� � ����������� �� �������� �������� �����
    /// </summary>
    private void UpdateWheelAnimation()
    {
        float wheelRotation = currentSpeed * wheelRotationSpeed * Time.deltaTime;

        // �������� ����� ������
        foreach (Transform wheel in leftWheels)
        {
            if (wheel != null)
                wheel.Rotate(wheelRotation, 0, 0);
        }

        // �������� ������ ������
        foreach (Transform wheel in rightWheels)
        {
            if (wheel != null)
                wheel.Rotate(wheelRotation, 0, 0);
        }
    }

    /// <summary>
    /// �������� �������� �������� �������
    /// ������� ������ ���������� ������� ����� �������� UV ���������
    /// </summary>
    private void UpdateTrackAnimation()
    {
        if (Mathf.Abs(currentSpeed) < 0.1f) return;

        trackOffset += currentSpeed * trackScrollSpeed * Time.deltaTime;

        // ���������� �������� � ���������� �������
        if (leftTrackMaterial != null)
            leftTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, trackOffset));

        if (rightTrackMaterial != null)
            rightTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, trackOffset));
    }

    #endregion

    #region Audio System

    /// <summary>
    /// ���������� ����� ���������
    /// �������� ������ � ��������� ����� � ����������� �� ��������
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
    /// �������� �������� � ������
    /// ������� ��� ����������� ����������� ��������
    /// </summary>
    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    /// <summary>
    /// ��������� ������� �������� �����
    /// ����� �������������� ������� ��������� (UI, �������)
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    /// <summary>
    /// ��������� ����������� ����� � ������� �����������
    /// ������� ��� ������� ��������
    /// </summary>
    public Vector3 GetTurretForward()
    {
        return turret != null ? turret.forward : transform.forward;
    }

    /// <summary>
    /// ��������� ������� ���� �����
    /// ������������ ��� ����������� ����� ��������� ��������
    /// </summary>
    public Vector3 GetCannonMuzzlePosition()
    {
        if (cannon != null)
        {
            // ����� ������� MuzzlePoint ��� ������������� ����� �����
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
        // ������������ � ���������
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
