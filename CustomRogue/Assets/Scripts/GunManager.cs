using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class GunManager : MonoBehaviour
{
    // Shared between all guns
    static PlayerCamera playerCamera;

    public GunScriptableObject gunConfig;
    public Transform shootPointTransform;
    bool canShoot = true;

    // Bullet object pool
    ObjectPool<BulletManager> bulletPool;

    static public void SetCurrentCamera(PlayerCamera cam)
    {
        playerCamera = cam;
    }

    void Awake()
    {
        bulletPool = new ObjectPool<BulletManager>(
            CreateBullet,
            OnTakeBullet,
            OnReleaseBullet,
            OnDestroyBullet,
            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 100
        );
    }

    BulletManager CreateBullet()
    {
        GameObject bulletGO = Instantiate(gunConfig.bulletPrefab);
        BulletManager bullet = bulletGO.GetComponent<BulletManager>();

        bullet.Pool = bulletPool;
        return bullet;
    }

    void OnTakeBullet(BulletManager bullet)
    {
        bullet.OnShot(shootPointTransform.position, GetAimPoint());
    }

    void OnReleaseBullet(BulletManager bullet)
    {
        bullet.OnRelease();
    }

    void OnDestroyBullet(BulletManager bullet)
    {
        Destroy(bullet.gameObject);
    }

    public void Shoot()
    {
        if (canShoot)
        {
            BulletManager bullet = bulletPool.Get();

            StartCoroutine(ShotDelay());
        }
    }

    IEnumerator ShotDelay()
    {
        canShoot = false;
        yield return new WaitForSeconds(gunConfig.shootConfig.fireRate);
        canShoot = true;
    }

    Vector3 GetAimPoint()
    {
        Transform cameraTransform = playerCamera.GetActiveCamera().transform;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            targetPoint = hit.point;
        }
        else
        {
            // Nothing hit, shoot into the distance
            targetPoint = ray.GetPoint(50f);
        }

        return (targetPoint - shootPointTransform.position).normalized;
    }
}
