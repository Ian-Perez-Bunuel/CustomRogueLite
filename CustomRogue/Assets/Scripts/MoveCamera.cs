using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform firstPersonCamera;
    public Transform cameraPosition;

    // Update is called once per frame
    void Update()
    {
        firstPersonCamera.position = cameraPosition.position;
        firstPersonCamera.rotation = cameraPosition.rotation;
    }
}
