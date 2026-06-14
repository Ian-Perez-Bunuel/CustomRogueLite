using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    

    [Header("Default")]
    [SerializeField] GameObject defaultModel;

    [Header("Burrow")]
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

    public void RotateBurrow(Vector3 velocity)
    {
        // Rotate with velocity
        Vector3 moveDir = velocity;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);

            burrowModel.transform.rotation = Quaternion.Lerp(burrowModel.transform.rotation, targetRotation, 30f * Time.deltaTime);
        }
    }
}
