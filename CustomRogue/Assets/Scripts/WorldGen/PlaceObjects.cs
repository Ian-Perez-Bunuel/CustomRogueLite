using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.XR;

public class PlaceObjects : MonoBehaviour
{
    public enum PlacingArea
    {
        Top,
        Bottom,
        Both,
        Random
    }

    [SerializeField] MarchingCubesCompute world;

    [Header("Object")]
    public GameObject objPrefab;
    public int amount;

    [Header("Place Parameters")]
    Vector3 worldDimensions;
    public PlacingArea placingArea;
    public LayerMask layerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        worldDimensions = world.GetDimensions();
    }

    public void Generate()
    {
        switch (placingArea)
        {
            case PlacingArea.Top:
                PlaceAllTop();
                break;

            case PlacingArea.Bottom:
                PlaceAllBottom();
                break;

            case PlacingArea.Both:
                PlaceBoth();
                break;

            case PlacingArea.Random:
                PlaceRandom();
                break;
        }
    }
    void PlaceTop()
    {
        // Get the ray's starting point
        Vector3 rayPos;
        rayPos.x = Random.Range(0, worldDimensions.x);
        rayPos.y = 0;
        rayPos.z = Random.Range(0, worldDimensions.z);
        RaycastHit hit;

        if (Physics.Raycast(rayPos, transform.up, out hit, worldDimensions.y, layerMask))
        {
            Instantiate(objPrefab, hit.point, Quaternion.identity);
        }
    }

    void PlaceAllTop()
    {
        for (int i = 0; i < amount; i++)
        {
            PlaceTop();
        }
    }

    void PlaceBottom()
    {
        // Get the ray's starting point
        Vector3 rayPos;
        rayPos.x = Random.Range(0, worldDimensions.x);
        rayPos.y = worldDimensions.y;
        rayPos.z = Random.Range(0, worldDimensions.z);

        RaycastHit hit;

        if (Physics.Raycast(rayPos, -transform.up, out hit, worldDimensions.y, layerMask))
        {
            Instantiate(objPrefab, hit.point, Quaternion.identity);
        }
    }

    void PlaceAllBottom()
    {
        for (int i = 0; i < amount; i++)
        {
            PlaceBottom();
        }
    }

    void PlaceRandom()
    {
        for (int i = 0; i < amount; i++)
        {
            int type = Random.Range(0, 2);
            if (type == 0)
                PlaceTop();
            else
                PlaceBottom();
        }
    }

    void PlaceBoth()
    {
        for (int i = 0; i < amount; i++)
        {
            // Get the ray's starting point
            Vector3 rayPos;
            rayPos.x = Random.Range(0, worldDimensions.x);
            rayPos.y = worldDimensions.y;
            rayPos.z = Random.Range(0, worldDimensions.z);

            // Bottom
            RaycastHit hit;
            if (Physics.Raycast(rayPos, -transform.up, out hit, worldDimensions.y, layerMask))
            {
                Instantiate(objPrefab, hit.point, Quaternion.identity);
            }

            rayPos.y = 0;
            // Top
            if (Physics.Raycast(rayPos, transform.up, out hit, worldDimensions.y, layerMask))
            {
                Instantiate(objPrefab, hit.point, Quaternion.identity);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Generate();
        }
    }
}
