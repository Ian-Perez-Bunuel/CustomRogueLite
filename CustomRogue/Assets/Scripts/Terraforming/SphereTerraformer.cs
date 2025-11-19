using UnityEngine;
using UnityEngine.LightTransport;

public class SphereTerraformer : Terraformer
{
    public ComputeShader computeEditting;
    public float radius;


    public override void Edit()
    {
        float densityChange = (breaking == true) ? -0.1f : 0.1f;
        float radiusSq = radius * radius;

        computeEditting.SetFloat("radius", radius);
        computeEditting.SetFloat("densityChange", densityChange);
        computeEditting.SetFloat("radiusSq", radiusSq);
        computeEditting.SetVector("sphereCenter", transform.position);

        world.EditSphere(computeEditting, transform.position, radius);
    }

    private void FixedUpdate()
    {
        Edit();
    }
}
