using Unity.VisualScripting;
using UnityEngine;

public abstract class Terraformer : MonoBehaviour
{
    protected MarchingCubesCompute world;
    public bool breaking;

    private void Awake()
    {
        world = FindAnyObjectByType<MarchingCubesCompute>();
    }

    public abstract void Edit();
}
