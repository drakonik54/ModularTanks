using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game States")]
    public bool isGameActive = true;
    public bool isGamePaused = false;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip criticalSound;
    [SerializeField] private AudioClip timeUpSound;
    
    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverUI; // UI панель окончания игры
    [SerializeField] private string menuSceneName = "MainMenu";
    
    // Singleton для доступа из других скриптов
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
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
        // Настраиваем аудио компонент
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Подписываемся на события таймера
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.OnTimerWarning.AddListener(OnTimerWarning);
            GameTimer.Instance.OnTimerCritical.AddListener(OnTimerCritical);
            GameTimer.Instance.OnTimerFinished.AddListener(OnGameTimeUp);
        }
        
        // Начальное состояние игры
        Time.timeScale = 1f;
        isGameActive = true;
        isGamePaused = false;
        
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }
    
    void Update()
    {
        // Пауза на ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    // Обработчики событий таймера
    private void OnTimerWarning()
    {
        Debug.Log("GameManager: Получено предупреждение о 5 минутах!");
        
        if (warningSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
        
        // Дополнительные эффекты: вибрация камеры, показ сообщения и т.д.
    }
    
    private void OnTimerCritical()
    {
        Debug.Log("GameManager: Получено критическое предупреждение о 1 минуте!");
        
        if (criticalSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(criticalSound);
        }
        
        // Дополнительные эффекты: более сильная вибрация, мигание экрана
    }
    
    private void OnGameTimeUp()
    {
        Debug.Log("GameManager: Время игры закончилось!");
        
        isGameActive = false;
        
        if (timeUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(timeUpSound);
        }
        
        // Показываем экран окончания игры
        ShowGameOverScreen();
        
        // Останавливаем все танки, снаряды и т.д.
        StopAllGameplay();
    }
    
    private void ShowGameOverScreen()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        // Замедляем время для драматического эффекта
        Time.timeScale = 0.1f;
        
        // Через несколько секунд полностью останавливаем время
        Invoke(nameof(FreezeGame), 2f);
    }
    
    private void FreezeGame()
    {
        Time.timeScale = 0f;
    }
    
    private void StopAllGameplay()
    {
        // Находим все танки и останавливаем их движение
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");
        foreach (GameObject tank in tanks)
        {
            Rigidbody tankRigidbody = tank.GetComponent<Rigidbody>();
            if (tankRigidbody != null)
            {
                tankRigidbody.linearVelocity = Vector3.zero;
                tankRigidbody.angularVelocity = Vector3.zero;
            }
            
            // Отключаем управление танком
            MonoBehaviour[] tankScripts = tank.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in tankScripts)
            {
                if (script.GetType().Name.Contains("Tank") || 
                    script.GetType().Name.Contains("Controller") ||
                    script.GetType().Name.Contains("Input"))
                {
                    script.enabled = false;
                }
            }
        }
        
        // Останавливаем все снаряды
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject projectile in projectiles)
        {
            Rigidbody projectileRigidbody = projectile.GetComponent<Rigidbody>();
            if (projectileRigidbody != null)
            {
                projectileRigidbody.linearVelocity = Vector3.zero;
            }
        }
    }
    
    // Публичные методы для управления игрой
    public void PauseGame()
    {
        if (!isGameActive) return;
        
        isGamePaused = true;
        Time.timeScale = 0f;
        
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.PauseTimer();
        }
        
        Debug.Log("GameManager: Игра поставлена на паузу");
    }
    
    public void ResumeGame()
    {
        if (!isGameActive) return;
        
        isGamePaused = false;
        Time.timeScale = 1f;
        
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.ResumeTimer();
        }
        
        Debug.Log("GameManager: Игра возобновлена");
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            SceneManager.LoadScene(0); // Первая сцена в Build Settings
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("GameManager: Выход из игры");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    // Геттеры для других скриптов
    public bool IsGameActive() => isGameActive;
    public bool IsGamePaused() => isGamePaused;
}