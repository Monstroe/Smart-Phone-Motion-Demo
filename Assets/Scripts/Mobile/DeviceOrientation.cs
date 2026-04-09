using System.Collections;
using TMPro;
using UnityEngine;

public class DeviceOrientation : MonoBehaviour
{
    const float BETA = 0.05f;
    const float ZETA = 0.01f;

    [Header("Filter Settings")]
    [SerializeField] DegreesOfFreedom degreesOfFreedom = DegreesOfFreedom.Dof6;
    [SerializeField] FilterType filterType = FilterType.Madgwick;
    [Header("Debug UI")]
    [SerializeField] TMP_Text orientationText;
    [SerializeField] TMP_Text debugText;

    private DeviceFilter filter;
    private Vector3 gyroscope = Vector3.zero;
    private Vector3 accelerometer = Vector3.zero;
    private Vector3 magnetometer = Vector3.zero;
    private float previousTime = 0f;

    private bool calibrated = false;
    //private int calibrationSamples = 200;
    //private int currentSample = 0;
    private Vector3 currGyroBias = Vector3.zero;
    private Quaternion startingOrientation = Quaternion.identity;

    void Awake()
    {
        Input.gyro.enabled = true;
        if (degreesOfFreedom == DegreesOfFreedom.Dof9)
        {
            Input.compass.enabled = true;
        }

        switch (filterType)
        {
            case FilterType.Unity:
                // no filter at the moment
                break;
            case FilterType.Madgwick:
                if (degreesOfFreedom == DegreesOfFreedom.Dof6)
                {
                    filter = new MadgwickFilter(Quaternion.identity, BETA);
                }
                else
                {
                    filter = new MadgwickFilter(Quaternion.identity, BETA, ZETA);
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (!calibrated)
        {
            Quaternion reference = Input.gyro.attitude;
            filter.Calibrate(reference);
            calibrated = true;
            return;
        }

        float time = Time.time;
        float dt = time - previousTime;

        Vector3 accGravity = Input.acceleration * 9.81f; // INCLUDING GRAVITY
        Vector3 rotRate = Input.gyro.rotationRateUnbiased;

        accelerometer = accGravity;
        gyroscope = rotRate;

        if (degreesOfFreedom == DegreesOfFreedom.Dof6)
        {
            filter.Update(gyroscope, accelerometer, dt);
        }
        else
        {
            Vector3 mag = Input.compass.rawVector;
            magnetometer = mag;
            filter.Update(gyroscope, accelerometer, magnetometer, dt);
        }

        /*if (!calibrated)
        {
            Vector3 gyroBias = Input.gyro.rotationRateUnbiased;
            float sqrMag = (gyroBias - currGyroBias).sqrMagnitude;
            debugText.text = "Debug: " + sqrMag.ToString("F6");
            if (sqrMag < 1f)
            {
                calibrated = true;
                filter.Calibrate(Quaternion.Euler(currGyroBias));
            }
            else
            {
                currGyroBias = gyroBias;
            }
        }
        else
        {
            Vector3 gyro = Input.gyro.rotationRateUnbiased;// - currGyroBias;
            Vector3 accel = Input.acceleration * 9.81f;
            //Vector3 mag = Input.compass.rawVector.normalized;
            filter.Update(gyro, accel, Time.fixedDeltaTime);
            //filter.Update(gyro, accel, mag, Time.fixedDeltaTime);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        if (!calibrated)
        {
            return;
        }

        transform.localRotation = filter.Orientation;

        /*Quaternion deviceRotation = Input.gyro.attitude;
        if (!calibrated)
        {
            //startingOrientation = new Quaternion(deviceRotation.x, deviceRotation.y, -deviceRotation.z, -deviceRotation.w);
            startingOrientation = deviceRotation;
            calibrated = true;
        }
        else
        {
            //Quaternion newRotation = new Quaternion(deviceRotation.x, deviceRotation.y, -deviceRotation.z, -deviceRotation.w);
            //transform.localRotation = new Quaternion(newRotation.x - startingOrientation.x, newRotation.y - startingOrientation.y, newRotation.z - startingOrientation.z, newRotation.w - startingOrientation.w);

            //transform.localRotation = filter.Orientation;
            transform.localRotation = Quaternion.Inverse(startingOrientation) * deviceRotation;
            orientationText.text = "Orientation: " + transform.localRotation.eulerAngles.ToString("F3");
        }*/
    }
}

public enum DegreesOfFreedom
{
    Dof6,
    Dof9,
}
