using UnityEngine;

/// <summary>
/// ������ ����� � ������� �� ������������ ��� �����
/// ������������ ������������� ������ ������ ��� ��������� ��������� ������
/// </summary>
public class TankCamera : MonoBehaviour
{
    [Header("���� ��������")]
    [SerializeField] private Transform target; // ���� �� ������� ������� ������

    [Header("��������� �������")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 8f, -12f);
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;

    [Header("������ �� ������������")]
    [SerializeField] private LayerMask obstacleLayer = 1; // ���� ����� � �����������
    [SerializeField] private float minDistance = 2f; // ����������� ���������� �� �����������
    [SerializeField] private float maxDistance = 15f; // ������������ ���������� �� �����
    [SerializeField] private float sphereRadius = 0.5f; // ������ ��� �������� ������������

    [Header("����������� �� ������")]
    [SerializeField] private float minHeight = 2f; // ����������� ������ ������
    [SerializeField] private float maxHeight = 50f; // ������������ ������ ������

    [Header("������ ������")]
    [SerializeField] private CameraMode currentMode = CameraMode.FollowBehind;
    [SerializeField] private bool smoothTransition = true;

    // ������������ ����������
    private Camera cameraComponent;
    private Vector3 currentVelocity;
    private Vector3 desiredPosition;

    // Enum ��� ������� ������
    public enum CameraMode
    {
        FollowBehind,    // �� ������
        TopDown,         // ������
        FirstPerson,     // �� ������� ����
        Free            // ��������� ������
    }

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();

        // ������� ���������� ���������� ���� ��� ����
        RemovePhysicsComponents();

        // ���� ���� �� ������, �������� ����� ����
        if (target == null)
            target = FindTankTarget();
    }

    private void Start()
    {
        // ������������� ��������� �������
        if (target != null)
        {
            SetInitialPosition();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        UpdateCameraPosition();
        UpdateCameraRotation();

        // ���������� ����������
        DebugCameraState();
    }

    /// <summary>
    /// ������� ���������� ���������� � ������
    /// ��� ������������� ������� ������ ��� ��������� ����������
    /// </summary>
    private void RemovePhysicsComponents()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.LogWarning("������� Rigidbody � ������ - ��� ����� �������� �������!");
            DestroyImmediate(rb);
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Debug.LogWarning("������� Collider � ������!");
            DestroyImmediate(col);
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            Debug.LogWarning("������� CharacterController � ������!");
            DestroyImmediate(cc);
        }
    }

    /// <summary>
    /// ���� ���� ��� ��������
    /// </summary>
    private Transform FindTankTarget()
    {
        // ���� ������ � ����� Player
        GameObject playerTank = GameObject.FindGameObjectWithTag("Player");
        if (playerTank != null)
            return playerTank.transform;

        // ���� ������ � SimpleTankController
        SimpleTankController tank = FindObjectOfType<SimpleTankController>();
        if (tank != null)
            return tank.transform;

        // ���� ������ � ������ Tank
        GameObject tankObject = GameObject.Find("Tank");
        if (tankObject != null)
            return tankObject.transform;

        Debug.LogWarning("���� �� ������! ���������� ���� ��� ������ �������.");
        return null;
    }

    /// <summary>
    /// ������������� ��������� ������� ������
    /// </summary>
    private void SetInitialPosition()
    {
        Vector3 initialPosition = target.position + target.TransformDirection(followOffset);
        initialPosition.y = Mathf.Max(initialPosition.y, minHeight);
        transform.position = initialPosition;
    }

    /// <summary>
    /// ���������� ������� ������ � ������� �� ������������
    /// </summary>
    private void UpdateCameraPosition()
    {
        // ��������� �������� ������� � ����������� �� ������
        switch (currentMode)
        {
            case CameraMode.FollowBehind:
                desiredPosition = CalculateFollowBehindPosition();
                break;
            case CameraMode.TopDown:
                desiredPosition = CalculateTopDownPosition();
                break;
            case CameraMode.FirstPerson:
                desiredPosition = CalculateFirstPersonPosition();
                break;
            case CameraMode.Free:
                return; // ��������� ������ �� ����������� �������������
        }

        // ��������� ������������ � ������������ �������
        desiredPosition = CheckCollisions(desiredPosition);

        // ������ ������������ � ������� �������
        if (smoothTransition)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition,
                ref currentVelocity, 1f / followSpeed);
        }
        else
        {
            transform.position = desiredPosition;
        }
    }

    /// <summary>
    /// ���������� ������� ������ �� ������
    /// </summary>
    private Vector3 CalculateFollowBehindPosition()
    {
        Vector3 position = target.position + target.TransformDirection(followOffset);
        position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
        return position;
    }

    /// <summary>
    /// ���������� ������� ������ ������
    /// </summary>
    private Vector3 CalculateTopDownPosition()
    {
        Vector3 position = target.position + Vector3.up * 20f;
        return position;
    }

    /// <summary>
    /// ���������� ������� ������ �� ������� ����
    /// </summary>
    private Vector3 CalculateFirstPersonPosition()
    {
        Vector3 position = target.position + Vector3.up * 2f + target.forward * 1f;
        return position;
    }

    /// <summary>
    /// �������� ������������ � ������������� ������� ������
    /// ��� �������� ������ �� ������������ ��� �����
    /// </summary>
    private Vector3 CheckCollisions(Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - target.position;
        float distance = directionToTarget.magnitude;

        // Raycast �� ����� � ������
        RaycastHit hit;
        if (Physics.SphereCast(target.position, sphereRadius, directionToTarget.normalized,
            out hit, distance, obstacleLayer))
        {
            // ���� ���� �����������, ��������� ������ ����� ���
            Vector3 safePosition = hit.point - directionToTarget.normalized * minDistance;
            safePosition.y = Mathf.Max(safePosition.y, minHeight);
            return safePosition;
        }

        // ���������, ��� ������ �� ��� ������
        if (Physics.Raycast(targetPosition, Vector3.down, out hit, 1f, obstacleLayer))
        {
            targetPosition.y = hit.point.y + minDistance;
        }

        // ��������� �������� ����������� ������
        targetPosition.y = Mathf.Max(targetPosition.y, minHeight);

        return targetPosition;
    }

    /// <summary>
    /// ���������� �������� ������ ��� �������� �� �����
    /// </summary>
    private void UpdateCameraRotation()
    {
        if (currentMode == CameraMode.Free) return;

        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0; // ������� ������ ��� ����� ����������� ����

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            if (smoothTransition)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }

    /// <summary>
    /// ���������� ���������� � ��������� ������
    /// </summary>
    private void DebugCameraState()
    {
        // ��������� ���� ������ ��� ������
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f, obstacleLayer))
        {
            Debug.LogError("������ ��� ������! �������: " + transform.position);
        }
    }

    #region Public Methods

    /// <summary>
    /// ��������� ���� ��� ��������
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            SetInitialPosition();
        }
    }

    /// <summary>
    /// ������������ ������ ������
    /// </summary>
    public void SetCameraMode(CameraMode mode)
    {
        currentMode = mode;
        Debug.Log($"����� ������ ������� ��: {mode}");
    }

    /// <summary>
    /// ������������ �� ��������� ����� ������
    /// </summary>
    public void SwitchCameraMode()
    {
        int nextMode = ((int)currentMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length;
        SetCameraMode((CameraMode)nextMode);
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // ���������� �������� �������
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(desiredPosition, 0.5f);

        // ���������� ����������� �� ����� � ������
        Gizmos.color = Color.green;
        Gizmos.DrawLine(target.position, transform.position);

        // ���������� ����� �������� ������������
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
#endif
}
