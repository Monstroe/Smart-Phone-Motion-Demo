using System;
using System.Collections;
using CNetworkingSolution;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MobileInitializer : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject calibrateMenu;
    [SerializeField] private GameObject gameMenu;
    [Space]
    [SerializeField] private TMP_InputField ipInputField;
    [Space]
    [SerializeField] private int lobbyId = 0;
    [SerializeField] private int calibrationTime = 5;
    [SerializeField] private TMP_Text calibrateText;

    private string currentIp;

    void Start()
    {
        ClientManager.Instance.OnConnectionAccepted += OnConnectionAccepted;
        ClientManager.Instance.OnConnectionRejected += OnConnectionRejected;
        ClientManager.Instance.OnConnectionLost += OnConnectionLost;
        ResetMenu();

        currentIp = PlayerPrefs.GetString("LastUsedIP", "");
        ipInputField.text = currentIp;
    }

    void OnDestroy()
    {
        if (ClientManager.Instance != null)
        {
            ClientManager.Instance.OnConnectionAccepted -= OnConnectionAccepted;
            ClientManager.Instance.OnConnectionRejected -= OnConnectionRejected;
            ClientManager.Instance.OnConnectionLost -= OnConnectionLost;
        }
    }

    public void Play()
    {
        ClientManager.Instance.SetConnectionData(new ConnectionData()
        {
            LobbyId = lobbyId,
            LobbyConnectionType = LobbyConnectionType.JoinIfExists
        });
        ClientManager.Instance.RegisterTransport<CNetTransport>();
        CNetTransport transport = FindFirstObjectByType<CNetTransport>();
        transport.Address = currentIp != "" ? currentIp : transport.Address;
        ClientManager.Instance.StartTransport();
        mainMenu.SetActive(false);
    }

    public void SetIp()
    {
        string ip = ipInputField.text;
        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogError("IP address cannot be empty!");
            return;
        }

        currentIp = ip;
        PlayerPrefs.SetString("LastUsedIP", currentIp);
    }

    private void OnConnectionAccepted(ConnectionAcceptedArgs args)
    {
        mainMenu.SetActive(false);
        calibrateMenu.SetActive(true);

        StartCoroutine(CalibrationSequence());
    }

    private void OnConnectionRejected(ConnectionRejectedArgs args)
    {
        ResetMenu();
    }

    private void OnConnectionLost(ConnectionLostArgs args)
    {
        SceneManager.LoadSceneAsync("Mobile");
        ResetMenu();
    }

    private void ResetMenu()
    {
        mainMenu.SetActive(true);
        calibrateMenu.SetActive(false);
        gameMenu.SetActive(false);
    }

    private IEnumerator CalibrationSequence()
    {
        for (int i = calibrationTime; i > 0; i--)
        {
            calibrateText.text = $"Calibrating in {i}...";
            yield return new WaitForSeconds(1);
        }

        ClientManager.Instance.CurrentLobby.GetService<GameClientService>().OnGameInitialized += OnGameInitialized;
        ClientManager.Instance.CurrentLobby.GetService<GameClientService>().JoinGame();
    }

    private void OnGameInitialized()
    {
        ClientManager.Instance.CurrentLobby.GetService<GameClientService>().OnGameInitialized -= OnGameInitialized;
        calibrateMenu.SetActive(false);
        gameMenu.SetActive(true);
    }
}
