using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    public BulletConfig config;
    Rigidbody rb;

    TrailRenderer trail;
    [SerializeField] TrailConfigScriptableObject trailConfig;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        trail = GetComponent<TrailRenderer>();
        trailConfig.ApplyTo(trail);
    }



    void FixedUpdate()
    {
        // Gravity
        rb.AddForce(Vector3.down * config.gravity, ForceMode.Acceleration);

        rb.AddForce(transform.forward * config.speed, ForceMode.Force);
    }
}
