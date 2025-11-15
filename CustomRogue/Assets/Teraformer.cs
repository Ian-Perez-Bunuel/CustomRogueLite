using Unity.VisualScripting;
using UnityEngine;

public class Teraformer : MonoBehaviour
{
    [SerializeField] MarchingCubesCompute world;
    public bool active;
    public bool breaking;
    public float radius;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            breaking = !breaking;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (active)
        {
            world.EditSphere(transform.position, radius, breaking);
        }
    }
}
