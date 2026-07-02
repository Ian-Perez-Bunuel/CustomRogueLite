using UnityEngine;

[CreateAssetMenu(fileName = "Bullet Config", menuName = "Scriptable Objects/Bullets/Config")]
public class BulletConfig : ScriptableObject
{
    public string bulletName;

    public bool canDestroy = true;

    public float speed;

    // Swap for a curve for the movement path it should take till it reaches it's destination (OR SOMETHING IDK)
    public float gravity;

    [Header("Destruction")]
    public float explosionRadius = 1.0f;
}
