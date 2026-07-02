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
        GunManager.SetCurrentCamera(playerCamera);
        gun = Instantiate(gunPrefab, gunTransform).GetComponent<GunManager>();
    }

    private void Update()
    {
        if (shoot.action.IsPressed())
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        gun.Shoot();
    }
}
