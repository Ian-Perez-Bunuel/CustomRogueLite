using Unity.VisualScripting;
using UnityEngine;

public class Teraformer : MonoBehaviour
{
    [SerializeField] MarchingCubesCompute world;
    public bool active;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            Chunk chunk = world.GetChunkFromWorldPos(transform.position);

            chunk.EditData(transform.position, 2, true);
        }
    }
}
