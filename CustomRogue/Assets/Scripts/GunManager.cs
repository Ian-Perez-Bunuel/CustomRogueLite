using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GunManager : MonoBehaviour
{
    // Shared between all guns
    static PlayerCamera playerCamera;

    // Shared ammo
    public static ComputeBuffer ammoBuffer;
    static int[] ammo;

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
        if (ammoBuffer == null)
        {
            int amountOfAmmos = Enum.GetValues(typeof(PointMaterial)).Length;

            ammo = new int[amountOfAmmos];
            ammoBuffer = new ComputeBuffer(amountOfAmmos, sizeof(int));

            // Set base ammo to 100 on all materials
            for (int i = 0; i < amountOfAmmos; i++) 
            {
                ammo[i] = 100;
            }
            UpdateAmmoGPU();
        }

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

    public static void UpdateAmmoCPU()
    {
        ammoBuffer.GetData(ammo);
    }
    public static void UpdateAmmoGPU()
    {
        ammoBuffer.SetData(ammo);
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
        if (canShoot && ammo[(int)gunConfig.ammoType] >= gunConfig.ammoPerShot)
        {
            BulletManager bullet = bulletPool.Get();

            // Reduce ammo
            ammo[(int)gunConfig.ammoType] -= gunConfig.ammoPerShot;
            UpdateAmmoGPU();

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
            targetPoint = ray.GetPoint(1000f);
        }

        return (targetPoint - shootPointTransform.position).normalized;
    }

    void OnDestroy()
    {
        if (ammoBuffer != null)
        {
            ammoBuffer.Release();
            ammoBuffer = null;
            ammo = null;
        }
    }
}
