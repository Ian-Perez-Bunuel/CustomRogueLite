using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Scriptable Objects/Bullets/Trail Config", order = 4)]
public class TrailConfigScriptableObject : ScriptableObject
{
    public Material material;
    public float duration = 0.5f;
    public float minVertexDistance = 0.1f;
    public Gradient color;

    public AnimationCurve widthCurve = AnimationCurve.Linear(0, 1, 1, 0);

    public void ApplyTo(TrailRenderer trail)
    {
        trail.material = material;
        trail.time = duration;
        trail.minVertexDistance = minVertexDistance;
        trail.colorGradient = color;

        trail.widthCurve = widthCurve;
    }
}
