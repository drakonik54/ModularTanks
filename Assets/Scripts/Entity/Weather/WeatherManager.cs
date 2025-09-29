using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    [Header("Wind Settings")]
    public Vector3 windDirection = new Vector3(1, 0, 0); // направление ветра (X, Y, Z)
    public float windSpeed = 1f; // скорость ветра (0-10)

    [Header("Wind Variation")]
    public float windVariation = 0.3f; // случайные колебания силы ветра
    public float variationSpeed = 0.5f; // скорость изменения колебаний

    private static WeatherManager instance;
    public static WeatherManager Instance { get { return instance; } }

    void Awake()
    {
        // Паттерн Singleton для глобального доступа
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Можно добавить динамические изменения ветра со временем
        // Например, случайные порывы ветра
    }

    public Vector3 GetWindDirection()
    {
        return windDirection.normalized;
    }

    public float GetWindSpeed()
    {
        // Добавляем случайные колебания к базовой скорости ветра
        float variation = Mathf.Sin(Time.time * variationSpeed) * windVariation;
        return Mathf.Max(0, windSpeed + variation);
    }

    // Метод для получения силы ветра в конкретной точке
    public float GetWindStrengthAtPosition(Vector3 position)
    {
        // Можно добавить зоны с разной силой ветра
        return GetWindSpeed();
    }
}
