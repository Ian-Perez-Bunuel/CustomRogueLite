using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class CollisionTerraformer : Terraformer
{
    public ComputeShader computeEditting;
    public float editRadius;
    Rigidbody rb;

    public float terraformAmount = 0.1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        world = FindAnyObjectByType<MarchingCubesCompute>();
    }

    public override void Edit()
    {
        float densityChange = (breaking == true) ? -terraformAmount : terraformAmount;
        float radiusSq = editRadius * editRadius;

        computeEditting.SetFloat("radius", editRadius);
        computeEditting.SetFloat("densityChange", densityChange);
        computeEditting.SetFloat("radiusSq", radiusSq);
        computeEditting.SetVector("sphereCenter", transform.position);

        world.EditSphere(computeEditting, transform.position, editRadius);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Edit();
        gameObject.SetActive(false);
    }

    public void AddForceInDir(Vector3 dir)
    {
        rb.AddForce(dir * 100.0f);
    }
}
