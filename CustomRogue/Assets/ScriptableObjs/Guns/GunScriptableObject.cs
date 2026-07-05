using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Scriptable Objects/Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public string gunName;
    [Header("Ammo")]
    public PointMaterial ammoType;
    public int ammoPerShot = 1;

    // Bullet
    public GameObject bulletPrefab;
    // Settings
    public ShootConfigScriptableObject shootConfig;
}
