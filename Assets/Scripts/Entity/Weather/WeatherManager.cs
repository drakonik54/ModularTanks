using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    [Header("Wind Settings")]
    public Vector3 windDirection = new Vector3(1, 0, 0); // ����������� ����� (X, Y, Z)
    public float windSpeed = 1f; // �������� ����� (0-10)

    [Header("Wind Variation")]
    public float windVariation = 0.3f; // ��������� ��������� ���� �����
    public float variationSpeed = 0.5f; // �������� ��������� ���������

    private static WeatherManager instance;
    public static WeatherManager Instance { get { return instance; } }

    void Awake()
    {
        // ������� Singleton ��� ����������� �������
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
        // ����� �������� ������������ ��������� ����� �� ��������
        // ��������, ��������� ������ �����
    }

    public Vector3 GetWindDirection()
    {
        return windDirection.normalized;
    }

    public float GetWindSpeed()
    {
        // ��������� ��������� ��������� � ������� �������� �����
        float variation = Mathf.Sin(Time.time * variationSpeed) * windVariation;
        return Mathf.Max(0, windSpeed + variation);
    }

    // ����� ��� ��������� ���� ����� � ���������� �����
    public float GetWindStrengthAtPosition(Vector3 position)
    {
        // ����� �������� ���� � ������ ����� �����
        return GetWindSpeed();
    }
}
