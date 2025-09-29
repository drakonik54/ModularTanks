using UnityEngine;

public class TankHullDatabase : MonoBehaviour
{
    public TankStatHull[] hulls;  // ���� � ��������� ����� �������� 2 �������

    void Start()
    {
        foreach (var hull in hulls)
        {
            Debug.Log($"������ {hull.hullName}: HP={hull.hitPoints}, ��������={hull.power}, �����={hull.mass}, ����.��������={hull.maxSpeed}, ����������������={hull.turnSpeed}, ������������={hull.mudTraversal}");
        }
    }
}
