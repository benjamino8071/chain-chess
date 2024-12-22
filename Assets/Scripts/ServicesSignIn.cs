using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class ServicesSignIn : MonoBehaviour
{
    public ScoreboardGUI scoreboardGUI;

    public TextMeshProUGUI onlineConnectionStatusText;
    
    private async void Awake()
    {
        if (UnityServices.Instance.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        
            if (!AuthenticationService.Instance.IsSignedIn)
            {

                await SignInAnonymously();
            }
            else
            {
                scoreboardGUI.ShowScoreboardButton();
            }
        }
        else
        {
            scoreboardGUI.ShowScoreboardButton();
        }
    }
    
    async Task SignInAnonymously()
    {
        try
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                onlineConnectionStatusText.text = "You are Online";
                scoreboardGUI.ShowScoreboardButton();
            };
            AuthenticationService.Instance.SignInFailed += s =>
            {
                // Take some action here...
                //Player is in OFFLINE mode
                onlineConnectionStatusText.text = "You are Offline";
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not connect to Authentication Service. Error: "+e);
        }

    }
}
