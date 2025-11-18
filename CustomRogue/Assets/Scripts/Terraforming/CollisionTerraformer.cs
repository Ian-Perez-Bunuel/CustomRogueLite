using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class CollisionTerraformer : Terraformer
{
    public float editRadius;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        world = FindAnyObjectByType<MarchingCubesCompute>();
    }

    public override void Edit()
    {
        world.EditSphere(transform.position, editRadius, breaking);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Edit();

        Destroy(gameObject);
    }

    public void AddForceInDir(Vector3 dir)
    {
        rb.AddForce(dir * 100.0f);
    }
}
