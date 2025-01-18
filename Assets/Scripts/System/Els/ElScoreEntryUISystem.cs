using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElScoreEntryUISystem : ElDependency
{
    private ElRunInfoUISystem _runInfoUISystem;
    private ElArtefactsUISystem _artefactsUISystem;
    
    private Transform _scoreEntryUI;

    private ElLetterChoiceController _letterOne, _letterTwo, _letterThree;

    private double _playerScore = 1;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _runInfoUISystem = elCreator.GetDependency<ElRunInfoUISystem>();
        _artefactsUISystem = elCreator.GetDependency<ElArtefactsUISystem>();

        _scoreEntryUI = elCreator.GetFirstObjectWithName(AllTagNames.Entry);

        Transform choiceOne = elCreator.GetChildObjectByName(_scoreEntryUI.gameObject, AllTagNames.ChoiceOne);
        SortOutLetterChoice(1, choiceOne);
        
        Transform choiceTwo = elCreator.GetChildObjectByName(_scoreEntryUI.gameObject, AllTagNames.ChoiceTwo);
        SortOutLetterChoice(2, choiceTwo);
        
        Transform choiceThree = elCreator.GetChildObjectByName(_scoreEntryUI.gameObject, AllTagNames.ChoiceThree);
        SortOutLetterChoice(3, choiceThree);

        Transform submitButtonGo = elCreator.GetFirstObjectWithName(AllTagNames.Submit);
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
            
            await LeaderboardsService.Instance
                .AddPlayerScoreAsync(
                    Creator.scoreboardSo.ScoreboardID,
                    _playerScore,
                    new AddPlayerScoreOptions{Metadata = new Dictionary<string, string>{ {"team", Creator.gridSystemSo.seed.ToString()}}}
                    );
            
            Creator.enemySo.ResetData();
            Creator.playerSystemSo.levelNumberSaved = 0;
            Creator.playerSystemSo.roomNumberSaved = 0;
                    
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
        Transform upButt = Creator.GetChildObjectByName(letterChoice.gameObject, AllTagNames.Up);
        Button upButton = upButt.GetComponent<Button>();

        Transform outputText = Creator.GetChildObjectByName(letterChoice.gameObject, AllTagNames.Output);
        TextMeshProUGUI choiceText = outputText.GetComponent<TextMeshProUGUI>();
        
        Transform downButt = Creator.GetChildObjectByName(letterChoice.gameObject, AllTagNames.Down);
        Button downButton = downButt.GetComponent<Button>();

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
        _runInfoUISystem.Hide();
        _artefactsUISystem.Hide();
        _scoreEntryUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _scoreEntryUI.gameObject.SetActive(false);
    }
}
