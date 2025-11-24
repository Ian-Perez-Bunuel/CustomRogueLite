using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class Destructable : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Terraformer")
        {
            AnchorDestroyed();
        }
    }

    void AnchorDestroyed()
    {
        // Unparent them
        foreach (Transform child in transform)
        {
            child.GetComponent<Rigidbody>().isKinematic = false;
            child.SetParent(null); // removes parent
        }

        Destroy(gameObject);
    }
}
