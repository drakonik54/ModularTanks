using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TankGunDatabase : MonoBehaviour
{
    public TankStatGun[] guns;
    void Start()
    {
        foreach (var gun in guns)
        {
            Debug.Log($"<����� {gun.GunName}: ������={gun.Caliber}��, ����={gun.Damage}��, �����������={gun.Reload}�");
        }
    }

}
