using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Scriptable Objects/Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public string gunName;

    // Bullet
    public GameObject bulletPrefab;

    // Settings
    public ShootConfigScriptableObject shootConfig;
    public TrailConfigScriptableObject trailConfig;
}
