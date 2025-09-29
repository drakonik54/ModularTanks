using UnityEngine;

public class TankTurretDatabase : MonoBehaviour
{
    public TankStatTurret[] turrets;  // ���� � ��������� ����� �������� 2 �����

    void Start()
    {
        foreach (var turret in turrets)
        {
            Debug.Log($"<����� {turret.turretName}: HP={turret.hitPoints}, �����={turret.mass}, �������� ��������={turret.rotationSpeed}, ������ ������={turret.elevationSpeed}, �����={turret.viewRange}, ��������� �����������={turret.reloadMultiplier}");
        }
    }
}
