using UnityEngine;

public class Gravity : MonoBehaviour
{
    Rigidbody rb;

    Vector3 gravityForce;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("ERROR - No rigidbody found!");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForce(gravityForce, ForceMode.Acceleration);
    }

    public void SetGravity(Vector3 t_force)
    {
        gravityForce = t_force;
    }
}
