using UnityEngine;

public class CellDetection : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Renderer>().enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        GetComponent<Renderer>().enabled = true;
    }

    public void setSize(float scale) { transform.localScale = new Vector3(scale, scale, scale); }
}
