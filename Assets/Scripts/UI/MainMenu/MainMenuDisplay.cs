using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MainMenuDisplay : MonoBehaviour
{
    [Header("References")]
    //[SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField]
    private float bossSpeed = 50f;


    private async void Start()
    {

		try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Player Id: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        //connectingPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void StartHost()
    {
        HostManager.Instance.StartHost();
    }

    public void StartClient()
    {
        ClientManager.Instance.StartClient(joinCodeInputField.text);
    }
}
