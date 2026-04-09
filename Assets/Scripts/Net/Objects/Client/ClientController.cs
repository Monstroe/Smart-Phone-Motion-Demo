using CNetworkingSolution;
using UnityEngine;

public class ClientController : ClientObject
{
    private Gyroscope gyro;
    private Quaternion initialRotation;
    private bool isCalibrated = false;

    [Header("Send Settings")]
    [SerializeField] private float sendRate = 60f; // Hz
    private float sendInterval;
    private float timer;

    public override void Init(ushort id, ClientLobby lobby)
    {
        base.Init(id, lobby);
        lobby.GetService<ControllerClientService>().ClientControllers.Add(Owner, this);
    }

    public override void Remove()
    {
        base.Remove();
        lobby.GetService<ControllerClientService>().ClientControllers.Remove(Owner);
    }

    protected override void StartOnOwner()
    {
        base.StartOnOwner();
        if (!SystemInfo.supportsGyroscope)
        {
            Debug.LogError("Gyroscope not supported on this device!");
            return;
        }

        gyro = Input.gyro;
        gyro.enabled = true;
        sendInterval = 1f / sendRate;
    }

    protected override void UpdateOnOwner()
    {
        base.UpdateOnOwner();

        if (!gyro.enabled) return;

        Quaternion deviceRotation = gyro.attitude;
        deviceRotation *= Quaternion.Euler(90f, 0f, 180f);

        // Calibrate initial orientation (so "forward" feels natural)
        /*if (!isCalibrated)
        {
            initialRotation = Quaternion.Inverse(deviceRotation);
            isCalibrated = true;
        }

        Quaternion calibratedRotation = initialRotation * deviceRotation;*/

        timer += Time.deltaTime;
        if (timer >= sendInterval)
        {
            timer = 0f;
            SendToServerObject(ControllerPacketBuilder.ControllerState(deviceRotation, Input.touchCount > 0), TransportMethod.Unreliable);
            Debug.Log("Updating ClientController with Gyroscope data: " + deviceRotation.eulerAngles.ToString("F3") + " | " + deviceRotation.eulerAngles.ToString("F3"));
        }
    }
}
