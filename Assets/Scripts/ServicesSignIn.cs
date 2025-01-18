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

    public Scoreboard_SO scoreboardSo;
    
    private async void Awake()
    {
        if (scoreboardSo.useScoreboard)
        {
            try
            {
                if (UnityServices.Instance.State == ServicesInitializationState.Uninitialized)
                {
                    await UnityServices.InitializeAsync();
                }
            
                if (!AuthenticationService.Instance.IsSignedIn)
                {

                    await SignInAnonymously();
                }
                else
                {
                    onlineConnectionStatusText.text = "You are Online";
                    scoreboardGUI.ShowScoreboardButton();
                }
            }
            catch (Exception e)
            {
                onlineConnectionStatusText.text = "You are Offline";
                Debug.LogWarning("Could not connect to Authentication Service. Error: "+e);
            }
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
                onlineConnectionStatusText.text = "You are Offline";
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            onlineConnectionStatusText.text = "You are Offline";
            Debug.LogWarning("Could not connect to Authentication Service. Error: "+e);
        }

    }
}
