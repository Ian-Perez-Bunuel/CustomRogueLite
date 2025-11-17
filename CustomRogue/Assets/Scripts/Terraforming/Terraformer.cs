using Unity.VisualScripting;
using UnityEngine;

public abstract class Terraformer : MonoBehaviour
{
    [SerializeField] protected MarchingCubesCompute world;
    public bool active;
    public bool breaking;

    public abstract void Edit();
}
