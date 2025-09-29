using UnityEngine;

[CreateAssetMenu(fileName = "TankTurret", menuName = "ModularTank/TankTurret")]
public class TankStatTurret : ScriptableObject
{
    public string turretName;
    public float hitPoints;            // �������� �����
    public float mass;                 // ����� �����
    public float rotationSpeed;        // �������� �������� ����� (�������/���)
    public float elevationSpeed;       // �������� ���������/������� ����� (�������/���)
    public float viewRange;            // ����� (���������� � ������)
    public float reloadMultiplier = 1f; // ��������� ����������� (1 - �������, 1.5 - ���������, 0.5 - �������)
}
