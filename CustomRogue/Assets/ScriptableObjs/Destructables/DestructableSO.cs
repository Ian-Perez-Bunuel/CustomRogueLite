using UnityEngine;

[CreateAssetMenu(fileName = "DestructableSO", menuName = "Scriptable Objects/DestructableSO")]
public class DestructableSO : ScriptableObject
{
    public Point[] points;
    public Vector3 localSpawnPos; // Lowest point's Y, center X, Z
    public float radius; // Calc from center of object to furthest point
}
