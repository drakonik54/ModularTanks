using UnityEngine;

/// <summary>
/// Простой скрипт управления танком
/// Использует стандартную Input Manager систему Unity
/// </summary>
public class SimpleTankController : MonoBehaviour
{
    [Header("=== ДВИЖЕНИЕ ТАНКА ===")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float acceleration = 5f;
    
    [Header("=== КОМПОНЕНТЫ ТАНКА ===")]
    [SerializeField] private Transform turret;
    [SerializeField] private Transform cannon;
    [SerializeField] private Rigidbody tankRigidbody;
    
    [Header("=== БАШНЯ И ПУШКА ===")]
    [SerializeField] private float turretRotationSpeed = 60f;
    [SerializeField] private float cannonRotationSpeed = 20f;
    [SerializeField] private float cannonMinAngle = -10f;
    [SerializeField] private float cannonMaxAngle = 25f;
    
    [Header("=== КАТКИ (ОПЦИОНАЛЬНО) ===")]
    [SerializeField] private Transform[] leftWheels;
    [SerializeField] private Transform[] rightWheels;
    [SerializeField] private float wheelRotationSpeed = 360f;
    
    [Header("=== ОТЛАДКА ===")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Приватные переменные
    private float currentSpeed = 0f;
    private float currentCannonAngle = 0f;
    
    private void Start()
    {
        // Автоматический поиск компонентов если не назначены
        AutoFindComponents();
        
        // Настройка Rigidbody
        SetupRigidbody();
        
        if (showDebugInfo)
            Debug.Log("SimpleTankController инициализирован!");
    }
    
    private void Update()
    {
        // Обработка всех систем управления
        HandleMovementInput();
        HandleTurretInput();
        HandleCannonInput();
        UpdateWheelAnimation();
        
        // Отладочная информация
        if (showDebugInfo && Input.GetKeyDown(KeyCode.H))
            ShowControls();
    }
    
    /// <summary>
    /// Обработка движения танка (WASD или стрелки)
    /// </summary>
    private void HandleMovementInput()
    {
        // Получаем ввод от игрока
        float moveInput = Input.GetAxis("Vertical");    // W/S или стрелки вверх/вниз
        float turnInput = Input.GetAxis("Horizontal");  // A/D или стрелки влево/вправо
        
        // Плавное изменение скорости
        float targetSpeed = moveInput * moveSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        
        // Движение вперед/назад
        if (tankRigidbody != null)
        {
            Vector3 moveDirection = transform.forward * currentSpeed * Time.deltaTime;
            tankRigidbody.MovePosition(transform.position + moveDirection);
        }
        else
        {
            transform.Translate(0, 0, currentSpeed * Time.deltaTime);
        }
        
        // Поворот танка влево/вправо
        if (Mathf.Abs(turnInput) > 0.1f)
        {
            float turnAmount = turnInput * turnSpeed * Time.deltaTime;
            
            if (tankRigidbody != null)
            {
                Quaternion turnRotation = Quaternion.Euler(0, turnAmount, 0);
                tankRigidbody.MoveRotation(transform.rotation * turnRotation);
            }
            else
            {
                transform.Rotate(0, turnAmount, 0);
            }
        }
    }
    
    /// <summary>
    /// Управление башней (мышь)
    /// </summary>
    private void HandleTurretInput()
    {
        if (turret == null) return;
        
        // Поворот башни по горизонтали (мышь X)
        float mouseX = Input.GetAxis("Mouse X");
        if (Mathf.Abs(mouseX) > 0.1f)
        {
            float turretRotation = mouseX * turretRotationSpeed * Time.deltaTime;
            turret.Rotate(0, turretRotation, 0);
        }
    }
    
    /// <summary>
    /// Управление наклоном пушки (Q/E)
    /// </summary>
    private void HandleCannonInput()
    {
        if (cannon == null) return;
        
        float cannonInput = 0f;
        
        // Клавиши для управления пушкой
        if (Input.GetKey(KeyCode.Q)) cannonInput = -1f; // Опустить пушку
        if (Input.GetKey(KeyCode.E)) cannonInput = 1f;  // Поднять пушку
        
        if (Mathf.Abs(cannonInput) > 0.1f)
        {
            float rotationChange = cannonInput * cannonRotationSpeed * Time.deltaTime;
            currentCannonAngle = Mathf.Clamp(currentCannonAngle + rotationChange, cannonMinAngle, cannonMaxAngle);
            
            cannon.localRotation = Quaternion.Euler(currentCannonAngle, 0, 0);
        }
    }
    
    /// <summary>
    /// Анимация вращения катков
    /// </summary>
    private void UpdateWheelAnimation()
    {
        if (Mathf.Abs(currentSpeed) < 0.1f) return;
        
        float wheelRotation = currentSpeed * wheelRotationSpeed * Time.deltaTime;
        
        // Вращаем левые катки
        if (leftWheels != null)
        {
            foreach (Transform wheel in leftWheels)
            {
                if (wheel != null)
                    wheel.Rotate(wheelRotation, 0, 0);
            }
        }
        
        // Вращаем правые катки  
        if (rightWheels != null)
        {
            foreach (Transform wheel in rightWheels)
            {
                if (wheel != null)
                    wheel.Rotate(wheelRotation, 0, 0);
            }
        }
    }
    
    /// <summary>
    /// Автоматический поиск компонентов танка
    /// </summary>
    private void AutoFindComponents()
    {
        // Ищем Rigidbody
        if (tankRigidbody == null)
            tankRigidbody = GetComponent<Rigidbody>();
        
        // Ищем башню
        if (turret == null)
        {
            turret = FindChildByName("turret") ?? 
                     FindChildByName("башня") ?? 
                     FindChildByName("tower");
        }
        
        // Ищем пушку
        if (cannon == null)
        {
            cannon = FindChildByName("cannon") ?? 
                     FindChildByName("gun") ?? 
                     FindChildByName("пушка");
        }
        
        // Ищем катки если не назначены
        AutoFindWheels();
        
        // Логирование найденных компонентов
        if (showDebugInfo)
        {
            Debug.Log($"Найдено компонентов:");
            Debug.Log($"- Rigidbody: {(tankRigidbody != null ? "✓" : "✗")}");
            Debug.Log($"- Башня: {(turret != null ? turret.name : "не найдена")}");
            Debug.Log($"- Пушка: {(cannon != null ? cannon.name : "не найдена")}");
            Debug.Log($"- Левых катков: {(leftWheels != null ? leftWheels.Length : 0)}");
            Debug.Log($"- Правых катков: {(rightWheels != null ? rightWheels.Length : 0)}");
        }
    }
    
    /// <summary>
    /// Автоматический поиск катков
    /// </summary>
    private void AutoFindWheels()
    {
        if (leftWheels != null && leftWheels.Length > 0 && 
            rightWheels != null && rightWheels.Length > 0) return;
        
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        var leftWheelsList = new System.Collections.Generic.List<Transform>();
        var rightWheelsList = new System.Collections.Generic.List<Transform>();
        
        foreach (Transform child in allChildren)
        {
            string name = child.name.ToLower();
            
            // Поиск объектов с именами катков
            if (name.Contains("wheel") || name.Contains("road") || name.Contains("roller") || 
                name.Contains("каток") || name.Contains("колесо"))
            {
                // Определяем сторону по позиции или имени
                if (name.Contains("left") || name.Contains("l_") || name.Contains("лев") || 
                    child.localPosition.x < 0)
                {
                    leftWheelsList.Add(child);
                }
                else if (name.Contains("right") || name.Contains("r_") || name.Contains("прав") || 
                         child.localPosition.x > 0)
                {
                    rightWheelsList.Add(child);
                }
            }
        }
        
        // Присваиваем найденные катки
        if (leftWheelsList.Count > 0) leftWheels = leftWheelsList.ToArray();
        if (rightWheelsList.Count > 0) rightWheels = rightWheelsList.ToArray();
    }
    
    /// <summary>
    /// Поиск дочернего объекта по имени
    /// </summary>
    private Transform FindChildByName(string name)
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child != transform && child.name.ToLower().Contains(name.ToLower()))
                return child;
        }
        return null;
    }
    
    /// <summary>
    /// Настройка Rigidbody для реалистичного поведения танка
    /// </summary>
    private void SetupRigidbody()
    {
        if (tankRigidbody == null) return;
        
        // Настройки для тяжелого танка
        tankRigidbody.mass = 5000f;           // Масса танка
        tankRigidbody.drag = 1f;              // Сопротивление движению
        tankRigidbody.angularDrag = 5f;       // Сопротивление вращению
        tankRigidbody.centerOfMass = new Vector3(0, -0.5f, 0); // Низкий центр масс
        
        // Ограничиваем вращение (танк не должен переворачиваться)
        tankRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | 
                                  RigidbodyConstraints.FreezeRotationZ;
    }
    
    /// <summary>
    /// Показать управление в консоли
    /// </summary>
    private void ShowControls()
    {
        Debug.Log("=== УПРАВЛЕНИЕ ТАНКОМ ===");
        Debug.Log("WASD или Стрелки - Движение танка");
        Debug.Log("Мышь - Поворот башни");
        Debug.Log("Q - Опустить пушку");
        Debug.Log("E - Поднять пушку");
        Debug.Log("H - Показать это сообщение");
        Debug.Log("========================");
    }
    
    /// <summary>
    /// Получить текущую скорость танка
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    /// <summary>
    /// Получить направление башни
    /// </summary>
    public Vector3 GetTurretForward()
    {
        return turret != null ? turret.forward : transform.forward;
    }
    
    /// <summary>
    /// Получить позицию дула пушки (для стрельбы)
    /// </summary>
    public Vector3 GetMuzzlePosition()
    {
        if (cannon != null)
        {
            return cannon.position + cannon.forward * 2f;
        }
        return transform.position + transform.forward * 3f;
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Отладочная визуализация в редакторе
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Показываем направление танка
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
        
        // Показываем направление башни
        if (turret != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(turret.position, turret.forward * 4f);
        }
        
        // Показываем дуло пушки
        if (cannon != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GetMuzzlePosition(), 0.2f);
        }
    }
    #endif
}