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
    public float mudTraversal;  // ����������� ������������ �� ����� (��������, 0-100%)
}

// ������ �������
// ���� ����� ������� "LightHull", ������ "HeavyHull"
