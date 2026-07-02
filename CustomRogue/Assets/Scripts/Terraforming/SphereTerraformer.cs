using UnityEngine;
using UnityEngine.LightTransport;

public class SphereTerraformer : Terraformer
{
    public ComputeShader computeEditting;
    float radius;


    public void SetRadius(float r)
    {
        radius = r;
    }

    public override void Edit()
    {
        float densityChange = (breaking == true) ? -0.1f : 0.1f;
        float radiusSq = radius * radius;

        computeEditting.SetFloat("radius", radius);
        computeEditting.SetFloat("radiusSq", radiusSq);
        computeEditting.SetVector("sphereCenter", transform.position);
        computeEditting.SetBool("breaking", breaking);

        world.EditSphere(computeEditting, transform.position, radius);
    }
}
