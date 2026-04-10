using UnityEngine;
using CNetworkingSolution;
using System.Collections.Generic;

[ServiceId("ControllerService")]
public class ControllerServerService : ServerService
{
    public Dictionary<UserData, ServerController> ServerControllers { get; private set; } = new Dictionary<UserData, ServerController>();

    [SerializeField] private ServerController serverControllerPrefab;

    public override void LateUserJoinedGame(UserData joinedUser)
    {
        base.LateUserJoinedGame(joinedUser);
        // Spawn new Controller
        InstantiateOnServerAsPlayer(serverControllerPrefab.gameObject, Vector3.up, Quaternion.identity, joinedUser.PlayerId);
    }

    public override void UserLeftGame(UserData leftUser)
    {
        base.UserLeftGame(leftUser);
        if (ServerControllers.TryGetValue(leftUser, out ServerController controller))
        {
            DestroyOnServer(controller);
        }
    }
}

public enum ControllerCommandType : ushort
{
    CONTROLLER_STATE = 1
}

public static class ControllerPacketBuilder
{
    public static NetPacket ControllerState(Quaternion rot, bool isHolding, float holdY)
    {
        NetPacket packet = new NetPacket();
        packet.Write(ControllerCommandType.CONTROLLER_STATE);
        packet.Write(rot);
        packet.Write(isHolding);
        packet.Write(holdY);
        return packet;
    }
}
