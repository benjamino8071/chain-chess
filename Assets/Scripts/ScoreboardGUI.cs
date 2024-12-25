using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardGUI : MonoBehaviour
{
    public GameObject canvasToShowOnGoBack;
    public GameObject scoreboardCanvas;
    public Button scoreboardButton;
    
    public MainMenu_SO mainMenuSo;
    public Scoreboard_SO scoreboardSo;
    
    private LinkedList<TextMeshProUGUI> _nameTexts = new();
    
    string VersionId { get; set; }
    int Offset { get; set; }
    int Limit { get; set; }
    int RangeLimit { get; set; }
    List<string> FriendIds { get; set; }

    private void Awake()
    {
        scoreboardButton.gameObject.SetActive(false);
        
        Transform[] childObjs = scoreboardCanvas.GetComponentsInChildren<Transform>();
        foreach (Transform childObj in childObjs)
        {
            if (childObj.CompareTag("Name"))
            {
                _nameTexts.AddLast(childObj.GetComponent<TextMeshProUGUI>());
            }
        }
        
        Hide();
    }

    public async void AddScore()
    {
        var scoreResponse = await LeaderboardsService.Instance.AddPlayerScoreAsync(scoreboardSo.ScoreboardID, 102);
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
    }

    public async void UpdateScoreboard()
    {
        Offset = 0;
        Limit = _nameTexts.Count;
        var scoresResponse =
            await LeaderboardsService.Instance.GetScoresAsync(scoreboardSo.ScoreboardID, new GetScoresOptions{Offset = Offset, Limit = Limit});
        
        LeaderboardScoresPage scores = scoresResponse;
        LinkedListNode<TextMeshProUGUI> currentNameText = _nameTexts.First;
        int i = 0;

        while (currentNameText != null && i < _nameTexts.Count && i < scores.Results.Count)
        {
            string playerName = scores.Results[i].PlayerName;
            int hashIndex = playerName.IndexOf('#');
            if (hashIndex >= 0)
            {
                playerName = playerName.Substring(0, hashIndex);
            }

            double roomsCleared = scores.Results[i].Score;
            
            currentNameText.Value.text = $"{i+1}. {playerName} - {roomsCleared}";
            i++;
            if (i >= _nameTexts.Count)
                break;
            currentNameText = currentNameText.Next;
        }
        
        Debug.Log(JsonConvert.SerializeObject(scoresResponse));
    }

    public async void GetPlayerScore()
    {
        var scoreResponse = 
            await LeaderboardsService.Instance.GetPlayerScoreAsync(scoreboardSo.ScoreboardID);
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
    }

    public async void GetVersionScores()
    {
        var versionScoresResponse =
            await LeaderboardsService.Instance.GetVersionScoresAsync(scoreboardSo.ScoreboardID, VersionId);
    Debug.Log(JsonConvert.SerializeObject(versionScoresResponse));
    }

    public void Show()
    {
        mainMenuSo.isOtherMainMenuCanvasShowing = true;
        canvasToShowOnGoBack.gameObject.SetActive(false);
        scoreboardCanvas.gameObject.SetActive(true);
        UpdateScoreboard();
    }

    public void ShowScoreboardButton()
    {
        scoreboardButton.gameObject.SetActive(true);
    }

    public void Hide()
    {
        mainMenuSo.isOtherMainMenuCanvasShowing = false;
        scoreboardCanvas.gameObject.SetActive(false);
        canvasToShowOnGoBack.gameObject.SetActive(true);
    }

    [Button]
    public void AddScoreTest()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AddScore();
        }
    }

    [Button]
    public void UpdateScoreboardTest()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            UpdateScoreboard();
        }
    }
}
