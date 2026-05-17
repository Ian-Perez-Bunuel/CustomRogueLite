using System.Runtime.InteropServices;
using UnityEngine;

public class ChunkBuilder : MonoBehaviour
{
    [SerializeField] MarchingCubesCompute chunkCanvas;
    [SerializeField] WorldSettings worldSettings;

    Chunk chunk;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // The only one
        chunk = chunkCanvas.GetChunkFromCoord(new Vector3Int(0, 0, 0));
    }

    private void Update()
    {
        // Brush should ignore the closest Collisions
    }
}
