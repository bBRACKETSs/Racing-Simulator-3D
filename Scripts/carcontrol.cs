using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Rendering;

public enum Axel
{
    Front,
    Rear
}
[Serializable]
public struct Wheel
{
    public GameObject model;
    public WheelCollider collider;
    public Axel axel;
}
public class carcontrol : MonoBehaviour
{
    [SerializeField]
    private float acceleration = 200.0f;
    [SerializeField]
    private float handling = 1.0f;
    [SerializeField]
    private float maximumAngle = 45.0f;
    [SerializeField]
    private float brakePower;
    [SerializeField]
    private float accelerationRate;
    [SerializeField]
    private Vector3 centerOfMass = new Vector3(0f, 0.15f, 0f);
    [SerializeField]
    private List<Wheel> wheels;
    [SerializeField]
    private float inputX, inputY;

    public GameObject[] tailLights;
    public GameObject[] headLights;
    bool isHeadLightOn;
    bool isTailLightOn;
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
        isHeadLightOn = false;
        isTailLightOn = false;
    }

    void Update()
    {
        MovingPos();
        WheelTurningAngle();
        if (Input.GetKeyDown(KeyCode.L))
        {
            isHeadLightOn = !isHeadLightOn;
            isTailLightOn = !isTailLightOn;

            foreach (var headLight in headLights)
            {
                headLight.SetActive(isHeadLightOn);
            }

            foreach (var tailLight in tailLights)
            {
                tailLight.SetActive(isTailLightOn);
            }
        }
    }
    private void MovingPos()
    {
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
    }
    private void LateUpdate()
    {
        Move();
        Turn();
        Brake();
    }

    private void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.collider.motorTorque = inputY * acceleration * accelerationRate * Time.deltaTime;
        }
    }
    private void Turn()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = inputX * handling * maximumAngle;
                wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle, _steerAngle, 0.1f);
            }
        }
    }
    private void WheelTurningAngle()
    {
        foreach (var wheel in wheels)
        {
            Vector3 _position;
            Quaternion _rotation;
            wheel.collider.GetWorldPose(out _position, out _rotation);
            wheel.model.transform.position = _position;
            wheel.model.transform.rotation = _rotation;
        }
    }
    private void Brake()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            foreach (var lights in tailLights)
            {
                lights.GetComponent<Light>().intensity = 30;
                lights.SetActive(true);
            }
            foreach (var wheel in wheels)
            {
                wheel.collider.brakeTorque = brakePower;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {

            foreach (var lights in tailLights)
            {
                if (isHeadLightOn && isTailLightOn == true)
                {
                    lights.GetComponent<Light>().intensity = 10;
                }
                else
                {
                    lights.GetComponent<Light>().intensity = 10;
                    lights.SetActive(false);
                }
            }

            foreach (var wheel in wheels)
            {
                wheel.collider.brakeTorque = 0;
            }
        }
    }
}
