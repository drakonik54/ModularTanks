using UnityEngine;

[CreateAssetMenu(fileName = "TankHull", menuName = "ModularTank/TankHull")]
public class TankStatHull : ScriptableObject
{
    public string hullName;
    public float hitPoints;
    public float power;
    public float mass;
    public float maxSpeed;
    public float turnSpeed;
    public float mudTraversal;  // Коэффициент проходимости по грязи (например, 0-100%)
}

// Пример ассетов
// Один можно назвать "LightHull", другой "HeavyHull"
