using System.Collections.Generic;
using CNetworkingSolution;
using UnityEngine;

[ServiceId("ControllerService")]
public class ControllerClientService : ClientService
{
    public Dictionary<UserData, ClientController> ClientControllers { get; private set; } = new Dictionary<UserData, ClientController>();
}