using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public GameObject[] cameras;
    public GameObject rearcam;
    private int activeCam = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            DisableCam();
            activeCam++;
            if (activeCam > cameras.Length-1)
            {
                activeCam = 0;
            }
            cameras[activeCam].SetActive(true);
        }
        RearCamera();
    }

    private void RearCamera()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            cameras[activeCam].SetActive(false);
            rearcam.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.L))
        {
            rearcam.SetActive(false);
            cameras[activeCam].SetActive(true);
        }
    }
    private void DisableCam()
    {
        foreach (var camera in cameras)
        {
            camera.SetActive(false);
        }
    }
}
