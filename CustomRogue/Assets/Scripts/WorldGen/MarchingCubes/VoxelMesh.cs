using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof(MeshCollider))]
public class VoxelMesh : MonoBehaviour
{
    public GameObject cellPrefab;
    public List<Collider> cells;

    public Transform voxelParent;

    MeshCollider meshCollider;
    public float resolution = 0.25f; // Cube's size

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cells = new List<Collider>();

        meshCollider = GetComponent<MeshCollider>();
        CreateGrid();
    }

    void CreateGrid()
    {
        Vector3 min = meshCollider.bounds.min;
        Vector3 max = meshCollider.bounds.max;

        for (float x = min.x; x < max.x; x += resolution)
        {
            for (float y = min.y; y < max.y; y += resolution)
            {
                for (float z = min.z; z < max.z; z += resolution)
                {
                    GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, z), Quaternion.identity, voxelParent);
                    cell.GetComponent<CellDetection>().setSize(resolution);
                    Collider cellCollider = cell.GetComponent<Collider>();
                    cells.Add(cellCollider);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
