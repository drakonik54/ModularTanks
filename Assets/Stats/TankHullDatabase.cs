using UnityEngine;

public class TankHullDatabase : MonoBehaviour
{
    public TankStatHull[] hulls;  // Сюда в редакторе можно добавить 2 корпуса

    void Start()
    {
        foreach (var hull in hulls)
        {
            Debug.Log($"Корпус {hull.hullName}: HP={hull.hitPoints}, Мощность={hull.power}, Масса={hull.mass}, Макс.Скорость={hull.maxSpeed}, СкоростьПоворота={hull.turnSpeed}, Проходимость={hull.mudTraversal}");
        }
    }
}
