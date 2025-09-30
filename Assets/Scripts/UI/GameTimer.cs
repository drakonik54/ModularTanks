using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float gameTimeInSeconds = 900f; // 15 минут = 900 секунд
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerBackground; // Опционально для фона
    
    [Header("Timer Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow; // Когда остается 5 минут
    [SerializeField] private Color criticalColor = Color.red;   // Когда остается 1 минута
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnTimerWarning;  // 5 минут осталось
    public UnityEngine.Events.UnityEvent OnTimerCritical; // 1 минута осталось
    public UnityEngine.Events.UnityEvent OnTimerFinished; // Время вышло
    
    private float currentTime;
    private bool isTimerRunning = true;
    private bool warningTriggered = false;
    private bool criticalTriggered = false;
    
    // Singleton для доступа из других скриптов
    public static GameTimer Instance { get; private set; }
    
    void Awake()
    {
        // Паттерн Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        currentTime = gameTimeInSeconds;
        
        // Если UI компоненты не назначены, попробуем найти их автоматически
        if (timerText == null)
        {
            timerText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        UpdateTimerDisplay();
    }
    
    void Update()
    {
        if (!isTimerRunning) return;
        
        // Уменьшаем время
        currentTime -= Time.deltaTime;
        
        // Проверяем события
        CheckTimerEvents();
        
        // Обновляем отображение
        UpdateTimerDisplay();
        
        // Проверяем окончание времени
        if (currentTime <= 0)
        {
            currentTime = 0;
            isTimerRunning = false;
            OnTimerFinished?.Invoke();
            
            Debug.Log("Время игры истекло!");
        }
    }
    
    private void CheckTimerEvents()
    {
        // Предупреждение - 5 минут (300 секунд)
        if (!warningTriggered && currentTime <= 300f)
        {
            warningTriggered = true;
            OnTimerWarning?.Invoke();
            Debug.Log("Осталось 5 минут!");
        }
        
        // Критическое предупреждение - 1 минута (60 секунд)
        if (!criticalTriggered && currentTime <= 60f)
        {
            criticalTriggered = true;
            OnTimerCritical?.Invoke();
            Debug.Log("Осталась 1 минута!");
        }
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        // Конвертируем секунды в минуты и секунды
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        
        // Форматируем текст (MM:SS)
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        
        // Меняем цвет в зависимости от оставшегося времени
        UpdateTimerColor();
    }
    
    private void UpdateTimerColor()
    {
        if (timerText == null) return;
        
        Color targetColor = normalColor;
        
        if (currentTime <= 60f) // Меньше минуты - красный
        {
            targetColor = criticalColor;
        }
        else if (currentTime <= 300f) // Меньше 5 минут - желтый
        {
            targetColor = warningColor;
        }
        
        timerText.color = targetColor;
        
        // Если есть фон, тоже меняем его прозрачность
        if (timerBackground != null && currentTime <= 60f)
        {
            Color bgColor = timerBackground.color;
            bgColor.a = Mathf.PingPong(Time.time * 2f, 0.5f) + 0.5f; // Мигание
            timerBackground.color = bgColor;
        }
    }
    
    // Публичные методы для управления таймером
    public void PauseTimer()
    {
        isTimerRunning = false;
        Debug.Log("Таймер поставлен на паузу");
    }
    
    public void ResumeTimer()
    {
        isTimerRunning = true;
        Debug.Log("Таймер возобновлен");
    }
    
    public void AddTime(float seconds)
    {
        currentTime += seconds;
        Debug.Log($"Добавлено {seconds} секунд к таймеру");
    }
    
    public void SetTime(float seconds)
    {
        currentTime = seconds;
        warningTriggered = false;
        criticalTriggered = false;
        Debug.Log($"Время установлено: {seconds} секунд");
    }
    
    // Геттеры для других скриптов
    public float GetTimeRemaining() => currentTime;
    public bool IsTimerRunning() => isTimerRunning;
    public bool IsTimeUp() => currentTime <= 0;
    
    // Методы для получения времени в разных форматах
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    public int GetMinutesRemaining() => Mathf.FloorToInt(currentTime / 60f);
    public int GetSecondsRemaining() => Mathf.FloorToInt(currentTime % 60f);
}