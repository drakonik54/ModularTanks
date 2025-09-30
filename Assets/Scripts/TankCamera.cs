using UnityEngine;

/// <summary>
/// Камера танка с защитой от проваливания под землю
/// Поддерживает множественные режимы камеры для аркадного танкового шутера
/// </summary>
public class TankCamera : MonoBehaviour
{
    [Header("Цель слежения")]
    [SerializeField] private Transform target; // Танк за которым следует камера

    [Header("Настройки позиции")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 8f, -12f);
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;

    [Header("Защита от столкновений")]
    [SerializeField] private LayerMask obstacleLayer = 1; // Слой земли и препятствий
    [SerializeField] private float minDistance = 2f; // Минимальное расстояние от препятствий
    [SerializeField] private float maxDistance = 15f; // Максимальное расстояние от танка
    [SerializeField] private float sphereRadius = 0.5f; // Радиус для проверки столкновений

    [Header("Ограничения по высоте")]
    [SerializeField] private float minHeight = 2f; // Минимальная высота камеры
    [SerializeField] private float maxHeight = 50f; // Максимальная высота камеры

    [Header("Режимы камеры")]
    [SerializeField] private CameraMode currentMode = CameraMode.FollowBehind;
    [SerializeField] private bool smoothTransition = true;

    // Кэшированные компоненты
    private Camera cameraComponent;
    private Vector3 currentVelocity;
    private Vector3 desiredPosition;

    // Enum для режимов камеры
    public enum CameraMode
    {
        FollowBehind,    // За танком
        TopDown,         // Сверху
        FirstPerson,     // От первого лица
        Free            // Свободная камера
    }

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();

        // Убираем физические компоненты если они есть
        RemovePhysicsComponents();

        // Если цель не задана, пытаемся найти танк
        if (target == null)
            target = FindTankTarget();
    }

    private void Start()
    {
        // Устанавливаем начальную позицию
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

        // Отладочная информация
        DebugCameraState();
    }

    /// <summary>
    /// Удаляет физические компоненты с камеры
    /// Это предотвращает падение камеры под действием гравитации
    /// </summary>
    private void RemovePhysicsComponents()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.LogWarning("Удаляем Rigidbody с камеры - это может вызывать падение!");
            DestroyImmediate(rb);
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Debug.LogWarning("Удаляем Collider с камеры!");
            DestroyImmediate(col);
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            Debug.LogWarning("Удаляем CharacterController с камеры!");
            DestroyImmediate(cc);
        }
    }

    /// <summary>
    /// Ищет танк для слежения
    /// </summary>
    private Transform FindTankTarget()
    {
        // Ищем объект с тегом Player
        GameObject playerTank = GameObject.FindGameObjectWithTag("Player");
        if (playerTank != null)
            return playerTank.transform;

        // Ищем объект с SimpleTankController
        SimpleTankController tank = FindObjectOfType<SimpleTankController>();
        if (tank != null)
            return tank.transform;

        // Ищем объект с именем Tank
        GameObject tankObject = GameObject.Find("Tank");
        if (tankObject != null)
            return tankObject.transform;

        Debug.LogWarning("Танк не найден! Установите цель для камеры вручную.");
        return null;
    }

    /// <summary>
    /// Устанавливает начальную позицию камеры
    /// </summary>
    private void SetInitialPosition()
    {
        Vector3 initialPosition = target.position + target.TransformDirection(followOffset);
        initialPosition.y = Mathf.Max(initialPosition.y, minHeight);
        transform.position = initialPosition;
    }

    /// <summary>
    /// Обновление позиции камеры с защитой от столкновений
    /// </summary>
    private void UpdateCameraPosition()
    {
        // Вычисляем желаемую позицию в зависимости от режима
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
                return; // Свободная камера не обновляется автоматически
        }

        // Проверяем столкновения и корректируем позицию
        desiredPosition = CheckCollisions(desiredPosition);

        // Плавно перемещаемся к целевой позиции
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
    /// Вычисление позиции камеры за танком
    /// </summary>
    private Vector3 CalculateFollowBehindPosition()
    {
        Vector3 position = target.position + target.TransformDirection(followOffset);
        position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
        return position;
    }

    /// <summary>
    /// Вычисление позиции камеры сверху
    /// </summary>
    private Vector3 CalculateTopDownPosition()
    {
        Vector3 position = target.position + Vector3.up * 20f;
        return position;
    }

    /// <summary>
    /// Вычисление позиции камеры от первого лица
    /// </summary>
    private Vector3 CalculateFirstPersonPosition()
    {
        Vector3 position = target.position + Vector3.up * 2f + target.forward * 1f;
        return position;
    }

    /// <summary>
    /// Проверка столкновений и корректировка позиции камеры
    /// Это основная защита от проваливания под землю
    /// </summary>
    private Vector3 CheckCollisions(Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - target.position;
        float distance = directionToTarget.magnitude;

        // Raycast от танка к камере
        RaycastHit hit;
        if (Physics.SphereCast(target.position, sphereRadius, directionToTarget.normalized,
            out hit, distance, obstacleLayer))
        {
            // Если есть препятствие, размещаем камеру перед ним
            Vector3 safePosition = hit.point - directionToTarget.normalized * minDistance;
            safePosition.y = Mathf.Max(safePosition.y, minHeight);
            return safePosition;
        }

        // Проверяем, что камера не под землей
        if (Physics.Raycast(targetPosition, Vector3.down, out hit, 1f, obstacleLayer))
        {
            targetPosition.y = hit.point.y + minDistance;
        }

        // Финальная проверка минимальной высоты
        targetPosition.y = Mathf.Max(targetPosition.y, minHeight);

        return targetPosition;
    }

    /// <summary>
    /// Обновление поворота камеры для слежения за целью
    /// </summary>
    private void UpdateCameraRotation()
    {
        if (currentMode == CameraMode.Free) return;

        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0; // Убираем наклон для более стабильного вида

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
    /// Отладочная информация о состоянии камеры
    /// </summary>
    private void DebugCameraState()
    {
        // Проверяем если камера под землей
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f, obstacleLayer))
        {
            Debug.LogError("КАМЕРА ПОД ЗЕМЛЕЙ! Позиция: " + transform.position);
        }
    }

    #region Public Methods

    /// <summary>
    /// Установка цели для слежения
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
    /// Переключение режима камеры
    /// </summary>
    public void SetCameraMode(CameraMode mode)
    {
        currentMode = mode;
        Debug.Log($"Режим камеры изменен на: {mode}");
    }

    /// <summary>
    /// Переключение на следующий режим камеры
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

        // Показываем желаемую позицию
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(desiredPosition, 0.5f);

        // Показываем направление от танка к камере
        Gizmos.color = Color.green;
        Gizmos.DrawLine(target.position, transform.position);

        // Показываем сферу проверки столкновений
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
#endif
}
