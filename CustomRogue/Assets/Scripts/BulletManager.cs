using UnityEngine;
using UnityEngine.Pool;

public class BulletManager : MonoBehaviour
{
    public BulletConfig config;
    Rigidbody rb;

    [SerializeField] SphereTerraformer terraformer;

    [Header("Visuals")]
    [SerializeField] GameObject model;
    [SerializeField] TrailRenderer trail;
    [SerializeField] TrailConfigScriptableObject trailConfig;

    // Guard from being returned to pool twice
    bool hasHit = false;

    public IObjectPool<BulletManager> Pool { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        trailConfig.ApplyTo(trail);
        trail.widthMultiplier = model.transform.localScale.y * 0.8f;

        terraformer.SetRadius(config.explosionRadius);
    }

    public void OnShot(Vector3 pos, Vector3 dir)
    {
        hasHit = false;

        gameObject.SetActive(true);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = pos;
        transform.position = rb.position;
        rb.rotation = Quaternion.LookRotation(dir);
        transform.rotation = rb.rotation;

        trail.Clear();
        trail.emitting = true;

        rb.linearVelocity = dir * config.speed;
    }

    void FixedUpdate()
    {
        // Gravity
        rb.AddForce(Vector3.down * config.gravity, ForceMode.Acceleration);
    }

    // Call when the bullet should be released / deactivated
    private void OnHit()
    {
        if (hasHit)
            return;

        hasHit = true;
        terraformer.Edit();
        trail.emitting = false;

        Pool.Release(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnHit();
    }

    public void OnRelease()
    {
        trail.emitting = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        gameObject.SetActive(false);
    }
}
