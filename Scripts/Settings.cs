using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class Settings : MonoBehaviour
{
    public GameObject[] cars;
    public GameObject carSpawnPoint;

    public GameObject[] AISpawnPoints;
    public GameObject[] AICars;
    void Start()
    {
        GameObject currentCar = Instantiate(cars[PlayerPrefs.GetInt("SelectedCar")], carSpawnPoint.transform.position, carSpawnPoint.transform.rotation);

        GameObject.FindWithTag("MainCamera").GetComponent<CameraControl>().target[0] = currentCar.transform.Find("SetPosition");
        GameObject.FindWithTag("MainCamera").GetComponent<CameraControl>().target[1] = currentCar.transform.Find("Pivot");

        GameObject.FindWithTag("GameController").GetComponent<CameraSwitch>().cameras[1] = currentCar.transform.Find("Cameras/Hood").gameObject;
        GameObject.FindWithTag("GameController").GetComponent<CameraSwitch>().cameras[2] = currentCar.transform.Find("Cameras/Interior").gameObject;
        GameObject.FindWithTag("GameController").GetComponent<CameraSwitch>().rearcam = currentCar.transform.Find("Cameras/Rear").gameObject;

        for (int i = 0; i < 3; i++)
        {
            int rng = Random.Range(0, AICars.Length);

            GameObject spawnedCar = Instantiate(AICars[rng], AISpawnPoints[i].transform.position, AISpawnPoints[i].transform.rotation);

            spawnedCar.GetComponent<AIController>().spawnPointIndex = i;
        }

    }
}
