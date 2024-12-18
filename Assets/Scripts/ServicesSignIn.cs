using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class ServicesSignIn : MonoBehaviour
{
    public ScoreboardGUI scoreboardGUI;
    
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
        AuthenticationService.Instance.SignedIn += () =>
        {
            scoreboardGUI.ShowScoreboardButton();
        };
        AuthenticationService.Instance.SignInFailed += s =>
        {
            // Take some action here...
            Debug.Log(s);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
}
