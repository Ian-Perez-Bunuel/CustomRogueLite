using UnityEngine;
using UnityEngine.LightTransport;

public class SphereTerraformer : Terraformer
{
    public float radius;


    public override void Edit()
    {
        if (active)
        {
            world.EditSphere(transform.position, radius, breaking);
        }
    }

    private void FixedUpdate()
    {
        Edit();
    }
}
