using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerGravity : MonoBehaviour
{
    [SerializeField] bool useGravity = true;
    [SerializeField] float gravity;
    public float GetGravity()
    {
        return gravity;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Toggle Gravity
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Toggle Gravity");
            useGravity = !useGravity;
        }
    }

    public Vector3 Apply(Vector3 velocity)
    {
        if (!useGravity)
            return velocity;

        Vector3 newVel = new Vector3(velocity.x, velocity.y + gravity * Time.deltaTime, velocity.z);
        return newVel;
    }
}
