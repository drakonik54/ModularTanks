using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TankGunDatabase : MonoBehaviour
{
    public TankStatGun[] guns;
    void Start()
    {
        foreach (var gun in guns)
        {
            Debug.Log($"<Пушка {gun.GunName}: Калибр={gun.Caliber}мм, Урон={gun.Damage}ед, Перезарядка={gun.Reload}с");
        }
    }

}
