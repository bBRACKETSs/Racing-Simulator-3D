using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VehicleSelection : MonoBehaviour
{
    public GameObject[] cars;
    public TMP_Text carName;
    public int activeCarIndex = 0;
    public ParticleSystem selectEffect;
    void Start()
    {
        cars[activeCarIndex].SetActive(true);
        carName.text = cars[activeCarIndex].GetComponent<CarStats>().carName;
    }

    public void NextCarButton()
    {
        if (activeCarIndex != cars.Length-1)
        {
            selectEffect.Play();
            cars[activeCarIndex].SetActive(false);
            activeCarIndex++;
            cars[activeCarIndex].SetActive(true);
            carName.text = cars[activeCarIndex].GetComponent<CarStats>().carName;
        }
        else
        {
            selectEffect.Play();
            cars[activeCarIndex].SetActive(false);
            activeCarIndex = 0;
            cars[activeCarIndex].SetActive(true);
            carName.text = cars[activeCarIndex].GetComponent<CarStats>().carName;
        }
    }

    public void PreviousCarButton()
    {
        if (activeCarIndex != 0)
        {
            selectEffect.Play();
            cars[activeCarIndex].SetActive(false);
            activeCarIndex--;
            cars[activeCarIndex].SetActive(true);
            carName.text = cars[activeCarIndex].GetComponent<CarStats>().carName;
        }
        else
        {
            selectEffect.Play();
            cars[activeCarIndex].SetActive(false);
            activeCarIndex = cars.Length-1;
            cars[activeCarIndex].SetActive(true);
            carName.text = cars[activeCarIndex].GetComponent<CarStats>().carName;
        }
    }

    public void StartButton()
    {
        PlayerPrefs.SetInt("SelectedCar",activeCarIndex);
        SceneManager.LoadScene("Level1");
    }
    void Update()
    {
        
    }
}
