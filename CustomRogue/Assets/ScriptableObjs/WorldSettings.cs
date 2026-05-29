using UnityEngine;

[CreateAssetMenu(fileName = "WorldSettings", menuName = "Scriptable Objects/WorldSettings")]
public class WorldSettings : ScriptableObject
{
    public float surfaceLevel; // If value > surfaceLevel then it's solid
    public float boundsSize = 1;
    public Vector3 offset = Vector3.zero;

    [Range(2, 150)]
    public int numPointsPerAxis = 70;
}
