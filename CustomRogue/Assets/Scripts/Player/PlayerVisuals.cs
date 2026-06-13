using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] GameObject defaultModel;
    [SerializeField] GameObject burrowModel;

    public void SetToDefault()
    {
        defaultModel.SetActive(true);
        burrowModel.SetActive(false);
    }

    public void SetToBurrow()
    {
        defaultModel.SetActive(false);
        burrowModel.SetActive(true);
    }

    private void Update()
    {
        if (burrowModel.activeSelf)
        {
            burrowModel.transform.rotation = orientation.rotation;
        }
    }
}
