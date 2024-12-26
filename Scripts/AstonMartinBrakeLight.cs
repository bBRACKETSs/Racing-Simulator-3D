using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstonMartinBrakeLight : MonoBehaviour
{
    public GameObject carBrakeLight;
    bool isBrakeLightOn = false;


    private void BrakeLight()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            isBrakeLightOn = !isBrakeLightOn;
            carBrakeLight.SetActive(isBrakeLightOn);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isBrakeLightOn = !isBrakeLightOn;
            carBrakeLight.SetActive(isBrakeLightOn);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isBrakeLightOn = !isBrakeLightOn;
            carBrakeLight.SetActive(isBrakeLightOn);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            isBrakeLightOn = !isBrakeLightOn;
            carBrakeLight.SetActive(isBrakeLightOn);
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            isBrakeLightOn = !isBrakeLightOn;
            carBrakeLight.SetActive(isBrakeLightOn);
        }

    }
    void Update()
    {
        BrakeLight();
    }
}
