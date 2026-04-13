using CNetworkingSolution;
using UnityEngine;
using TMPro;

public class ClientController : ClientObject
{
    private Quaternion calibration;
    private Quaternion correctionQuaternion;

    private HoldButton holdButton;

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

        Screen.orientation = ScreenOrientation.Portrait;
        if (!SystemInfo.supportsGyroscope)
        {
            Debug.LogError("Gyroscope not supported on this device!");
            return;
        }
        Input.gyro.enabled = true;
        calibration = Quaternion.identity;
        correctionQuaternion = Quaternion.Euler(90f, 0f, 0f);

        holdButton = FindFirstObjectByType<HoldButton>();

        sendInterval = 1f / sendRate;
    }

    protected override void UpdateOnOwner()
    {
        base.UpdateOnOwner();

        if (!Input.gyro.enabled) return;

        Quaternion deviceRotation = GyroToUnity(Input.gyro.attitude);
        Quaternion calculatedRotation = correctionQuaternion * deviceRotation;

        Quaternion finalRotation = calibration * calculatedRotation;

        timer += Time.deltaTime;
        if (timer >= sendInterval)
        {
            timer = 0f;
            SendToServerObject(ControllerPacketBuilder.ControllerState(finalRotation, holdButton.IsHolding, holdButton.DeltaY / Screen.height), TransportMethod.Unreliable);
            Debug.Log("Updating ClientController with Gyroscope data: " + finalRotation.eulerAngles.ToString("F3") + " | " + deviceRotation.eulerAngles.ToString("F3") + " | " + calibration.eulerAngles.ToString("F3"));
        }
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
