using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WindSway : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] vertices;

    [Header("Bush Sway Settings")]
    public float swayAmount = 0.1f;
    public float swayFrequency = 1f;
    public float bottomY = 0f;

    [Header("Wind Response")]
    public float windSensitivity = 1f;
    public float directionInfluence = 0.5f;

    private WeatherManager weatherManager;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = Instantiate(mf.sharedMesh);
        mf.mesh = mesh;

        baseVertices = mesh.vertices;
        vertices = new Vector3[baseVertices.Length];

        weatherManager = WeatherManager.Instance;
        if (weatherManager == null)
        {
            Debug.LogWarning("WeatherManager не найден! Куст будет качаться без учета ветра.");
        }
    }

    void Update()
    {
        Vector3 worldWindDir = Vector3.right;
        float windSpeed = 1f;

        if (weatherManager != null)
        {
            worldWindDir = weatherManager.GetWindDirection();
            windSpeed = weatherManager.GetWindStrengthAtPosition(transform.position);
        }

        // КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: преобразуем мировое направление ветра в локальные координаты объекта
        Vector3 localWindDir = transform.InverseTransformDirection(worldWindDir);

        float effectiveSwaySpeed = windSpeed * windSensitivity;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = baseVertices[i];

            if (vertex.y > bottomY)
            {
                float heightFactor = (vertex.y - bottomY) / (GetMaxHeight() - bottomY);
                heightFactor = Mathf.Clamp01(heightFactor);

                // Основное качание по Y
                float swayY = Mathf.Sin(Time.time * effectiveSwaySpeed + vertex.x * swayFrequency)
                             * swayAmount * heightFactor * windSpeed;

                // Используем ЛОКАЛЬНОЕ направление ветра для влияния на X и Z
                float swayX = Mathf.Sin(Time.time * effectiveSwaySpeed * 0.7f + vertex.z * swayFrequency)
                             * swayAmount * heightFactor * localWindDir.x * directionInfluence * windSpeed;

                float swayZ = Mathf.Sin(Time.time * effectiveSwaySpeed * 0.8f + vertex.x * swayFrequency)
                             * swayAmount * heightFactor * localWindDir.z * directionInfluence * windSpeed;

                vertex.x += swayX;
                vertex.y += swayY;
                vertex.z += swayZ;
            }

            vertices[i] = vertex;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    float GetMaxHeight()
    {
        float maxY = float.MinValue;
        foreach (Vector3 v in baseVertices)
        {
            if (v.y > maxY)
                maxY = v.y;
        }
        return maxY;
    }
}
