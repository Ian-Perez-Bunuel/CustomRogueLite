using UnityEngine;

public class WorldBorder : MonoBehaviour
{
    MarchingCubesCompute world;
    Vector3 worldDimensions;
    Vector3 worldCenter;

    [SerializeField] GameObject centerObj;
    [SerializeField] GameObject[] worldEdges;
    [SerializeField] GameObject[] worldWalls;

    [Header("Wall Parameters")]
    float wallThickness = 1f;

    [Header("Edge Parameters")]
    float edgeThickness = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        world = FindAnyObjectByType<MarchingCubesCompute>();

        worldDimensions = world.GetDimensions();
        worldCenter = world.GetWorldCenter();

        TunnelGenerator.SetCenterPos(worldCenter);

        // Center Obj
        centerObj.transform.position = worldCenter;
        SetupEdges();
        SetupWalls();
    }

    void SetupEdges()
    {
        if (worldEdges == null || worldEdges.Length < 12)
        {
            Debug.LogWarning("WorldBorder needs 12 edge cubes.");
            return;
        }

        float w = worldDimensions.x;
        float h = worldDimensions.y;
        float d = worldDimensions.z;

        float hw = w * 0.5f;
        float hh = h * 0.5f;
        float hd = d * 0.5f;

        Vector3 P(float x, float y, float z)
        {
            return worldCenter + new Vector3(x, y, z);
        }

        void SetEdge(int index, Vector3 a, Vector3 b)
        {
            Transform t = worldEdges[index].transform;

            t.position = (a + b) * 0.5f;
            t.localScale = new Vector3(
                Mathf.Abs(a.x - b.x) + edgeThickness,
                Mathf.Abs(a.y - b.y) + edgeThickness,
                Mathf.Abs(a.z - b.z) + edgeThickness
            );
        }

        // Bottom rectangle
        SetEdge(0, P(-hw, -hh, -hd), P(hw, -hh, -hd));
        SetEdge(1, P(hw, -hh, -hd), P(hw, -hh, hd));
        SetEdge(2, P(hw, -hh, hd), P(-hw, -hh, hd));
        SetEdge(3, P(-hw, -hh, hd), P(-hw, -hh, -hd));

        // Top rectangle
        SetEdge(4, P(-hw, hh, -hd), P(hw, hh, -hd));
        SetEdge(5, P(hw, hh, -hd), P(hw, hh, hd));
        SetEdge(6, P(hw, hh, hd), P(-hw, hh, hd));
        SetEdge(7, P(-hw, hh, hd), P(-hw, hh, -hd));

        // Vertical edges
        SetEdge(8, P(-hw, -hh, -hd), P(-hw, hh, -hd));
        SetEdge(9, P(hw, -hh, -hd), P(hw, hh, -hd));
        SetEdge(10, P(hw, -hh, hd), P(hw, hh, hd));
        SetEdge(11, P(-hw, -hh, hd), P(-hw, hh, hd));
    }

    void SetupWalls()
    {
        if (worldWalls == null || worldWalls.Length < 6)
        {
            Debug.LogWarning("WorldBorder needs 6 wall cubes: Left, Right, Front, Back, Top, Bottom.");
            return;
        }

        float width = worldDimensions.x;
        float height = worldDimensions.y;
        float depth = worldDimensions.z;

        float minX = worldCenter.x - width * 0.5f;
        float maxX = worldCenter.x + width * 0.5f;
        float minY = worldCenter.y - height * 0.5f;
        float maxY = worldCenter.y + height * 0.5f;
        float minZ = worldCenter.z - depth * 0.5f;
        float maxZ = worldCenter.z + depth * 0.5f;

        // 0 = Left wall
        worldWalls[0].transform.position = new Vector3(minX - wallThickness * 0.5f, worldCenter.y, worldCenter.z);
        worldWalls[0].transform.localScale = new Vector3(wallThickness, height, depth);

        // 1 = Right wall
        worldWalls[1].transform.position = new Vector3(maxX + wallThickness * 0.5f, worldCenter.y, worldCenter.z);
        worldWalls[1].transform.localScale = new Vector3(wallThickness, height, depth);

        // 2 = Front wall
        worldWalls[2].transform.position = new Vector3(worldCenter.x, worldCenter.y, minZ - wallThickness * 0.5f);
        worldWalls[2].transform.localScale = new Vector3(width, height, wallThickness);

        // 3 = Back wall
        worldWalls[3].transform.position = new Vector3(worldCenter.x, worldCenter.y, maxZ + wallThickness * 0.5f);
        worldWalls[3].transform.localScale = new Vector3(width, height, wallThickness);

        // 4 = Top wall
        worldWalls[4].transform.position = new Vector3(worldCenter.x, maxY + wallThickness * 0.5f, worldCenter.z);
        worldWalls[4].transform.localScale = new Vector3(width, wallThickness, depth);

        // 5 = Bottom wall
        worldWalls[5].transform.position = new Vector3(worldCenter.x, minY - wallThickness * 0.5f, worldCenter.z);
        worldWalls[5].transform.localScale = new Vector3(width, wallThickness, depth);
    }
}
