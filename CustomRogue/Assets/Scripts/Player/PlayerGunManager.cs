using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunManager : MonoBehaviour
{
    public PlayerCamera playerCamera;
    [SerializeField] GameObject gunPrefab;
    GunManager gun;

    [Header("Input")]
    [SerializeField] InputActionReference shoot;

    [Header("Visuals")]
    [SerializeField] Transform gunTransform;

    private void Start()
    {
        gun = Instantiate(gunPrefab, gunTransform).GetComponent<GunManager>();
    }

    private void Update()
    {
        if (shoot.action.WasPressedThisFrame())
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        gun.Shoot(GetBulletDirection());
    }

    Vector3 GetBulletDirection()
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

        return (targetPoint - gun.shootPointTransform.position).normalized;
    }
}
