using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public Transform[] target;
    void LateUpdate()
    {
        transform.position = target[0].position;
        transform.LookAt(target[1].position);
    }
}
