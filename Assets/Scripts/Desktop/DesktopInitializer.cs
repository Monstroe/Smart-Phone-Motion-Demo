using CNetworkingSolution;
using UnityEngine;

public class DesktopInitializer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ServerManager.Instance.RegisterTransport<CNetTransport>();
        ServerManager.Instance.StartTransports();
    }
}
