using UnityEngine;

public class TankTurretDatabase : MonoBehaviour
{
    public TankStatTurret[] turrets;  // Сюда в редакторе можно добавить 2 башни

    void Start()
    {
        foreach (var turret in turrets)
        {
            Debug.Log($"<Башня {turret.turretName}: HP={turret.hitPoints}, Масса={turret.mass}, Скорость Вращения={turret.rotationSpeed}, Подъём орудия={turret.elevationSpeed}, Обзор={turret.viewRange}, Множитель перезарядки={turret.reloadMultiplier}");
        }
    }
}
