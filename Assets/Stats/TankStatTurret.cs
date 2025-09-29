using UnityEngine;

[CreateAssetMenu(fileName = "TankTurret", menuName = "ModularTank/TankTurret")]
public class TankStatTurret : ScriptableObject
{
    public string turretName;
    public float hitPoints;            // Здоровье башни
    public float mass;                 // Масса башни
    public float rotationSpeed;        // Скорость поворота башни (градусы/сек)
    public float elevationSpeed;       // Скорость опускания/подъёма пушки (градусы/сек)
    public float viewRange;            // Обзор (расстояние в метрах)
    public float reloadMultiplier = 1f; // Множитель перезарядки (1 - обычная, 1.5 - медленнее, 0.5 - быстрее)
}
