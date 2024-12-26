using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649
namespace UnityStandardAssets.Vehicles.Car
{
    internal enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    internal enum SpeedType
    {
        MPH,
        KPH
    }

    public class CarController : MonoBehaviour
    {
        [SerializeField] private CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
        [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
        [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];
        [SerializeField] private Vector3 m_CentreOfMassOffset;
        [SerializeField] private float m_MaximumSteerAngle;
        [Range(0, 1)][SerializeField] private float m_SteerHelper;
        [Range(0, 1)][SerializeField] private float m_TractionControl;
        [SerializeField] private float m_FullTorqueOverAllWheels;
        [SerializeField] private float m_ReverseTorque;
        [SerializeField] private float m_MaxHandbrakeTorque;
        [SerializeField] private float m_Downforce = 100f;
        [SerializeField] private SpeedType m_SpeedType;
        [SerializeField] private float m_Topspeed = 200;
        [SerializeField] private static int NoOfGears = 6;
        [SerializeField] private float m_RevRangeBoundary = 1f;
        [SerializeField] private float m_SlipLimit;
        [SerializeField] private float m_BrakeTorque;

        private Quaternion[] m_WheelMeshLocalRotations;
        private Vector3 m_Prevpos, m_Pos;
        private float m_SteerAngle;
        private int m_GearNum;
        private float m_GearFactor;
        private float m_OldRotation;
        private float m_CurrentTorque;
        private Rigidbody m_Rigidbody;
        private const float k_ReversingThreshold = 0.01f;

        public GameObject[] tailLights;
        public GameObject[] headLights;
        bool isHeadLightOn;
        bool isTailLightOn;
        public bool Skidding { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle { get { return m_SteerAngle; } }
        public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } }
        public float MaxSpeed { get { return m_Topspeed; } }
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }

        #region SpeedGauge

        public Text currentSpeedText;

        public Text currentGearText;

        int topSpeedInt;

        public GameObject gaugeNeedle;

        #endregion

        #region NitroGauge

        public Image nitroSlider;

        float nitroValue;

        bool nitroStatus;

        #endregion

        #region ExhaustEffects

        public GameObject nitroEffect;
        public ParticleSystem[] exhaustShiftEffect;

        #endregion

        #region GameSounds

        public AudioSource[] Sounds;

        #endregion

        public Transform straightDirectionRay;
        public int DirectionIndex = 1;
        GameObject WrongWayObject;

        private void Start()
        {
            nitroValue = 100f;
            isHeadLightOn = false;
            isTailLightOn = false;

            m_WheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
            }
            m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

            m_MaxHandbrakeTorque = float.MaxValue;

            m_Rigidbody = GetComponent<Rigidbody>();
            m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);
            StartCoroutine(Nitrobar());

            currentSpeedText = GameObject.FindWithTag("Speed").GetComponent<Text>();
            currentGearText = GameObject.FindWithTag("Gear").GetComponent<Text>();
            gaugeNeedle = GameObject.FindWithTag("GaugeNeedle");
            nitroSlider = GameObject.FindWithTag("NitroGauge").GetComponent<Image>();
            WrongWayObject = GameObject.FindWithTag("WrongWayObject");
            WrongWayObject.SetActive(false);
        }

        private void Update()
        {

            LightsWithBrake();
            SpeedGaugeControl();
            NitroReady();

            RaycastHit hit;

            if (Physics.Raycast(straightDirectionRay.position,straightDirectionRay.TransformDirection(Vector3.forward),out hit,Mathf.Infinity))
            {
                if (hit.transform.CompareTag("StraightDirectionObject"))
                {
                    if (DirectionIndex > int.Parse(hit.transform.gameObject.name))
                    {
                        WrongWayObject.SetActive(true);
                        //Debug.Log("TERS YÖN");
                    }
                    else
                    {
                        DirectionIndex = int.Parse(hit.transform.gameObject.name);
                        WrongWayObject.SetActive(false);
                        //Debug.Log("DOÐRU YÖN");
                    }
                }
                
            }
            Debug.DrawRay(straightDirectionRay.position, straightDirectionRay.TransformDirection(Vector3.forward)*hit.distance,Color.green);
        }

        private void NitroReady()
        {
            if (Input.GetKey(KeyCode.X))
            {
                nitroEffect.SetActive(true);
                nitroSlider.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                nitroSlider.GetComponent<Animator>().enabled = false;
                if (nitroValue > 0)
                {
                    m_Rigidbody.velocity += 7f * Time.deltaTime * m_Rigidbody.velocity.normalized;
                    nitroStatus = true;
                }
                nitroValue -= 0.5f;
                nitroSlider.fillAmount = nitroValue / 200;
                if (!Sounds[1].isPlaying)
                {
                    Sounds[1].Play();
                }
                if (nitroValue <= 0)
                {
                    nitroEffect.SetActive(false);
                    nitroStatus = false;
                    nitroValue = 0;
                    nitroSlider.fillAmount = 0f;
                }
            }

            if (Input.GetKeyUp(KeyCode.X))
            {
                nitroEffect.SetActive(false);
                if (nitroValue < 100)
                {
                    nitroStatus = false;
                }
                if (nitroValue <= 0)
                {
                    nitroEffect.SetActive(false);
                    nitroValue = 0;
                    nitroSlider.fillAmount = 0f;
                }
            }
        }
        IEnumerator Nitrobar()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.3f);

                if (!nitroStatus)
                {
                    nitroValue += 2;
                    nitroSlider.fillAmount = nitroValue / 200;
                    if (nitroValue >= 100)
                    {
                        nitroSlider.GetComponent<Animator>().enabled = true;
                        nitroValue = 100f;
                        nitroSlider.fillAmount = 0.5f;
                        nitroStatus = true;
                    }
                }
            }
        }
        private void SpeedGaugeControl()
        {
            topSpeedInt = (int)CurrentSpeed;
            currentSpeedText.text = topSpeedInt.ToString();

            if (CurrentSpeed == 0)
            {
                Quaternion rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                gaugeNeedle.transform.localRotation = rotation;
            }
            else
            {
                Quaternion rotation = Quaternion.Euler(new Vector3(0f, 0f, -CurrentSpeed * 1.33f));
                gaugeNeedle.transform.localRotation = rotation;
            }
        }
        private void LightsWithBrake()
        {
            if (Input.GetKeyDown(KeyCode.K))
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
            if (Input.GetKeyDown(KeyCode.Space))
            {

                foreach (var lights in tailLights)
                {
                    lights.GetComponent<Light>().intensity = 30;
                    lights.SetActive(true);
                }
                for (int i = 0; i < 4; i++)
                {
                    m_WheelColliders[i].GetComponent<WheelCollider>().brakeTorque = m_BrakeTorque;
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
                for (int i = 0; i < 4; i++)
                {

                    m_WheelColliders[i].GetComponent<WheelCollider>().brakeTorque = 0;

                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {

                foreach (var lights in tailLights)
                {
                    lights.GetComponent<Light>().intensity = 30;
                    lights.SetActive(true);
                }

            }
            if (Input.GetKeyUp(KeyCode.DownArrow))
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
            }
        }
        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
            float upgearlimit = (1 / (float)NoOfGears) * (m_GearNum + 1);
            float downgearlimit = (1 / (float)NoOfGears) * m_GearNum;

            if (m_GearNum > 0 && f < downgearlimit)
            {
                m_GearNum--;
                currentGearText.text = m_GearNum.ToString();
                exhaustShiftEffect[0].Play();
                exhaustShiftEffect[1].Play();
                Sounds[0].Play();
            }

            if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
            {
                m_GearNum++;
                currentGearText.text = m_GearNum.ToString();
                exhaustShiftEffect[0].Play();
                exhaustShiftEffect[1].Play();
                Sounds[0].Play();
            }

            if (topSpeedInt > 0)
            {
                if (m_GearNum == 0)
                {
                    currentGearText.text = "1";

                }
                else
                {
                    currentGearText.text = m_GearNum.ToString();

                }
            }

            if (Input.GetAxis("Vertical") == -1)
            {
                currentGearText.text = "R";
            }
        }


        // simple function to add a curved bias towards 1 for a value in the 0-1 range
        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }


        // unclamped version of Lerp, to allow value to exceed the from-to range
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }


        private void CalculateGearFactor()
        {
            float f = (1 / (float)NoOfGears);
            // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
            // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
            var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
            m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
        }


        private void CalculateRevs()
        {
            // calculate engine revs (for display / sound)
            // (this is done in retrospect - revs are not used in force/power calculations)
            CalculateGearFactor();
            var gearNumFactor = m_GearNum / (float)NoOfGears;
            var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
        }


        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion quat;
                Vector3 position;
                m_WheelColliders[i].GetWorldPose(out position, out quat);
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;
            }

            //clamp input values
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            //Set the steer on the front wheels.
            //Assuming that wheels 0 and 1 are the front wheels.
            m_SteerAngle = steering * m_MaximumSteerAngle;
            m_WheelColliders[0].steerAngle = m_SteerAngle;
            m_WheelColliders[1].steerAngle = m_SteerAngle;

            SteerHelper();
            ApplyDrive(accel, footbrake);
            CapSpeed();

            //Set the handbrake.
            //Assuming that wheels 2 and 3 are the rear wheels.
            if (handbrake > 0f)
            {
                var hbTorque = handbrake * m_MaxHandbrakeTorque;
                m_WheelColliders[2].brakeTorque = hbTorque;
                m_WheelColliders[3].brakeTorque = hbTorque;
            }


            CalculateRevs();
            GearChanging();

            AddDownForce();
            CheckForWheelSpin();
            TractionControl();
        }


        private void CapSpeed()
        {
            float speed = m_Rigidbody.velocity.magnitude;
            switch (m_SpeedType)
            {
                case SpeedType.MPH:

                    speed *= 2.23693629f;
                    if (speed > m_Topspeed)
                        m_Rigidbody.velocity = (m_Topspeed / 2.23693629f) * m_Rigidbody.velocity.normalized;
                    break;

                case SpeedType.KPH:
                    speed *= 3.6f;
                    if (speed > m_Topspeed)
                        m_Rigidbody.velocity = (m_Topspeed / 3.6f) * m_Rigidbody.velocity.normalized;
                    break;
            }
        }


        private void ApplyDrive(float accel, float footbrake)
        {

            float thrustTorque;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 4f);
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].motorTorque = thrustTorque;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                    break;

            }

            for (int i = 0; i < 4; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, m_Rigidbody.velocity) < 50f)
                {
                    m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
                }
                else if (footbrake > 0)
                {
                    m_WheelColliders[i].brakeTorque = 0f;
                    m_WheelColliders[i].motorTorque = -m_ReverseTorque * footbrake;
                }
            }
        }


        private void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                m_WheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
            {
                var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
            }
            m_OldRotation = transform.eulerAngles.y;
        }


        // this is used to add more grip in relation to speed
        private void AddDownForce()
        {
            m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up * m_Downforce *
                                                         m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
        }


        // checks if the wheels are spinning and is so does three things
        // 1) emits particles
        // 2) plays tiure skidding sounds
        // 3) leaves skidmarks on the ground
        // these effects are controlled through the WheelEffects class
        private void CheckForWheelSpin()
        {
            // loop through all wheels
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelHit;
                m_WheelColliders[i].GetGroundHit(out wheelHit);

                // is the tire slipping above the given threshhold
                if (Mathf.Abs(wheelHit.forwardSlip) >= m_SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= m_SlipLimit)
                {
                    m_WheelEffects[i].EmitTyreSmoke();

                    // avoiding all four tires screeching at the same time
                    // if they do it can lead to some strange audio artefacts
                    if (!AnySkidSoundPlaying())
                    {
                        m_WheelEffects[i].PlayAudio();
                    }
                    continue;
                }

                // if it wasnt slipping stop all the audio
                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();
                }
                // end the trail generation
                m_WheelEffects[i].EndSkidTrail();
            }
        }

        // crude traction control that reduces the power to wheel if the car is wheel spinning too much
        private void TractionControl()
        {
            WheelHit wheelHit;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].GetGroundHit(out wheelHit);

                        AdjustTorque(wheelHit.forwardSlip);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[3].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[1].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;
            }
        }


        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
            {
                m_CurrentTorque -= 10 * m_TractionControl;
            }
            else
            {
                m_CurrentTorque += 10 * m_TractionControl;
                if (m_CurrentTorque > m_FullTorqueOverAllWheels)
                {
                    m_CurrentTorque = m_FullTorqueOverAllWheels;
                }
            }
        }


        private bool AnySkidSoundPlaying()
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_WheelEffects[i].PlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
