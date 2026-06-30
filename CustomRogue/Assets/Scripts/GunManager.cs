using UnityEngine;

public class GunManager : MonoBehaviour
{
    public GunScriptableObject gunConfig;
    public Transform shootPointTransform;

    public void Shoot(Vector3 dir)
    {
        Instantiate(gunConfig.bulletPrefab, shootPointTransform.position, Quaternion.LookRotation(dir));
    }
}
