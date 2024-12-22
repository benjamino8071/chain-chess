using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElScoreEntryUISystem : ElDependency
{
    private ElPauseUISystem _pauseUISystem;
    
    private Transform _scoreEntryUI;

    private ElLetterChoiceController _letterOne, _letterTwo, _letterThree;

    private double _playerScore = 1;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.TryGetDependency(out ElPauseUISystem pauseUISystem))
        {
            _pauseUISystem = pauseUISystem;
        }

        _scoreEntryUI = GameObject.FindWithTag("Entry").transform;

        GameObject[] letterChoices = GameObject.FindGameObjectsWithTag("Choice");

        foreach (GameObject letterChoice in letterChoices)
        {
            int letterChoiceNumber = 0;
            if (letterChoice.name.Contains("1"))
            {
                letterChoiceNumber = 1;
            }
            else if (letterChoice.name.Contains("2"))
            {
                letterChoiceNumber = 2;
            }
            else if (letterChoice.name.Contains("3"))
            {
                letterChoiceNumber = 3;
            }
            
            SortOutLetterChoice(letterChoiceNumber, letterChoice.transform);
        }

        GameObject submitButtonGo = GameObject.FindWithTag("Submit");
        Button submitButton = submitButtonGo.GetComponent<Button>();
        submitButton.onClick.AddListener(SubmitScore);
        
        Hide();
    }

    private async void SubmitScore()
    {
        try
        {
            string name = _letterOne.LetterChosen() + _letterTwo.LetterChosen().ToString() + _letterThree.LetterChosen();
        
            await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
        
            var scoreResponse = await LeaderboardsService.Instance.AddPlayerScoreAsync(Creator.scoreboardSo.ScoreboardID, _playerScore);
            Debug.Log(JsonConvert.SerializeObject(scoreResponse));
            
            Creator.enemySo.ResetData();
            Creator.playerSystemSo.levelNumberSaved = 0;
            Creator.playerSystemSo.roomNumberSaved = 0;
            Creator.timerSo.currentTime = Creator.timerSo.startingTime;
                    
            SceneManager.LoadScene("MainMenuScene");
        }
        catch (Exception e)
        {
            Debug.Log("Could not submit score. Error "+e);
            throw;
        }
    }

    private void SortOutLetterChoice(int choiceNum, Transform letterChoice)
    {
        Button upButton = null, downButton = null;
        TextMeshProUGUI choiceText= null;
        foreach (Transform choiceObj in letterChoice)
        {
            if (choiceObj.CompareTag("Up"))
            {
                upButton = choiceObj.GetComponent<Button>();
            }
            else if (choiceObj.CompareTag("Down"))
            {
                downButton = choiceObj.GetComponent<Button>();
            }
            else if (choiceObj.CompareTag("Output"))
            {
                choiceText = choiceObj.GetComponent<TextMeshProUGUI>();
            }
        }

        ElLetterChoiceController letterChoiceController = new ElLetterChoiceController();
        letterChoiceController.GameStart(Creator);
        letterChoiceController.Initialise(choiceText, upButton, downButton);
        switch (choiceNum)
        {
            case 1:
                _letterOne = letterChoiceController;
                break;
            case 2:
                _letterTwo = letterChoiceController;
                break;
            case 3:
                _letterThree = letterChoiceController;
                break;
        }
    }

    public void Show(double playerScore)
    {
        _playerScore = playerScore;
        _pauseUISystem.Hide();
        _scoreEntryUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _scoreEntryUI.gameObject.SetActive(false);
    }
}
