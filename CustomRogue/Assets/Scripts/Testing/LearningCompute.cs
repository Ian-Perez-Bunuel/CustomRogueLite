using System.Collections.Generic;
using UnityEngine;

struct Cube
{
    public Vector3 position;
    public Color color;
}

public class LearningCompute : MonoBehaviour
{
    public ComputeShader computeShader;

    List<GameObject> objects;

    private Cube[] data;
    public int width = 20;
    public int height = 20;
    public int depth = 20;
    int count = -1;

    Mesh mesh;
    public Material material;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        count = width * height * depth;
        CreateCubes();
    }

    public void CreateCubes()
    {
        objects = new List<GameObject>();

        data = new Cube[count];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    CreateCube(x, y, z);
                }
            }
        }
    }

    int Index3D(int x, int y, int z) { return x + y * width + z * width * height; }

    private void CreateCube(int x, int y, int z)
    {
        // make a visible cube with geometry
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = $"Cube {Index3D(x,y,z)}";
        // give it a material
        if (material != null) cube.GetComponent<Renderer>().material = new Material(material);

        // position in a grid
        float spacing = 1.1f;
        cube.transform.position = new Vector3(x * spacing, y * spacing, z * spacing);

        // color
        Color color = Random.ColorHSV();
        cube.GetComponent<Renderer>().material.SetColor("_Color", color);

        // track + write into data
        objects.Add(cube);
        data[Index3D(x, y, z)] = new Cube { position = cube.transform.position, color = color };
    }


    public void OnRandomizeGPU()
    {
        int colorSize = sizeof(float) * 4;
        int vector3Size = sizeof(float) * 3;
        int totalSize = colorSize + vector3Size;

        ComputeBuffer cubesBuffer = new ComputeBuffer(data.Length, totalSize);
        cubesBuffer.SetData(data);

        computeShader.SetBuffer(0, "cubes", cubesBuffer);
        computeShader.SetFloat("width", width);
        computeShader.SetFloat("height", height);
        computeShader.SetFloat("depth", depth);

        computeShader.Dispatch(0, width / 10, height / 10, depth / 10); // amount / said in compute shader

        // Read data back from buffer
        cubesBuffer.GetData(data);

        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            Cube cube = data[i];
            obj.transform.position = cube.position;
            obj.GetComponent<MeshRenderer>().material.SetColor("_Color", cube.color);
        }

        // Dispose buffer
        cubesBuffer.Dispose();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 0, 100, 50), "GPU"))
        {
            OnRandomizeGPU();
        }
    }
}
