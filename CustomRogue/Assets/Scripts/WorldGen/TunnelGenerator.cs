using UnityEngine;

public class TunnelGenerator : MonoBehaviour
{
    public GameObject tunnelPrefab;
    TunnelSegment tunnelSegment;

    public int segmentAmount = 1;

    // Params
    public float angleLeeway;

    public float minLength;
    public float maxLength;

    public float minRadius;
    public float maxRadius;

    static Vector3 centerPos = Vector3.zero;

    public static void SetCenterPos(Vector3 t_centerPos)
    {
        centerPos = t_centerPos;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tunnelSegment = Instantiate(tunnelPrefab, transform.position, Quaternion.identity, transform).GetComponent<TunnelSegment>();

        Vector3 randPos;
        randPos.x = Random.Range(centerPos.x - 100, centerPos.x + 100);
        randPos.y = Random.Range(centerPos.y - 100, centerPos.y + 100);
        randPos.z = Random.Range(centerPos.z - 100, centerPos.z + 100);
        transform.position = randPos;

        Quaternion randomRotation = Random.rotation;
        SetupNextSegment(transform.position, Vector3.zero, randomRotation, Vector3.zero);

        if (tunnelSegment != null)
        {
            float l = Random.Range(minLength, maxLength);
            float r1 = Random.Range(minRadius, maxRadius);
            float r2 = Random.Range(minRadius, maxRadius);
            tunnelSegment.SetAllParams(l, r1, r2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            GenerateSegments();
        }
    }

    void GenerateSegments()
    {
        segmentAmount += Random.Range(-segmentAmount / 4, segmentAmount / 4);

        for (int i = 0; i < segmentAmount; i++)
        {
            SpawnSegment();
        }
    }

    void SpawnSegment()
    {
        Debug.Log("Placed next segment");
        float length = Random.Range(minLength, maxLength);
        Vector3 spawnPos = tunnelSegment.GetEndPoint();
        // Move forward relative to the last segment's transform
        spawnPos += tunnelSegment.transform.forward * (length / 1.999f);

        Vector3 rotationPerAxis = GetRandomRotation();

        SetupNextSegment(spawnPos, tunnelSegment.GetEndPoint(), tunnelSegment.transform.rotation, rotationPerAxis);
        float r2 = Random.Range(minRadius, maxRadius);
        tunnelSegment.SetAllParams(length, tunnelSegment.GetEndRadius(), r2);
        tunnelSegment.Edit();
    }

    public void SetupNextSegment(Vector3 spawnPos, Vector3 endCenterPosition, Quaternion baseRotation, Vector3 rotationPerAxis)
    {
        // Make the tunnelManager hold every segment of it's own tunnels
        tunnelSegment.transform.position = spawnPos;
        tunnelSegment.transform.localRotation = baseRotation;

        // Rotation
        tunnelSegment.transform.RotateAround(
            endCenterPosition,      // Pivot point
            Vector3.right,          // Axis
            rotationPerAxis.x
        );
        tunnelSegment.transform.RotateAround(
            endCenterPosition,      // Pivot point
            Vector3.up,             // Axis
            rotationPerAxis.y
        );
        tunnelSegment.transform.RotateAround(
            endCenterPosition,      // Pivot point
            Vector3.forward,        // Axis
            rotationPerAxis.z
        );
    }

    Vector3 GetRandomRotation()
    {
        Vector3 rot = Vector3.zero;

        rot.x = Random.Range(-angleLeeway, angleLeeway + 1);
        rot.y = Random.Range(-angleLeeway, angleLeeway + 1);
        rot.z = Random.Range(-angleLeeway, angleLeeway + 1);

        return rot;
    }
}
