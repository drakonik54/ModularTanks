using UnityEngine;

[CreateAssetMenu(fileName = "TankStatGun", menuName = "ModularTank/TankGun")]
public class TankStatGun : ScriptableObject
{
    public string GunName;
    public float Caliber;
    public float Damage;
    public float Reload;
}
