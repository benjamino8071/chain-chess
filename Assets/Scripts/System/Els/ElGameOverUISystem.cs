using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElGameOverUISystem : ElDependency
{
    private ElRunInfoUISystem _runInfoUISystem;
    private ElArtefactsUISystem _artefactsUISystem;
    
    private Transform _gameOverUI;

    private ButtonManager _confirmButton;
    
    private PlayerDataTexts _playerDataTexts;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _runInfoUISystem = elCreator.GetDependency<ElRunInfoUISystem>();
        _artefactsUISystem = elCreator.GetDependency<ElArtefactsUISystem>();

        _gameOverUI = elCreator.GetFirstObjectWithName(AllTagNames.GameOver);

        _playerDataTexts = _gameOverUI.GetComponent<PlayerDataTexts>();

        Transform confirmButtonTf = elCreator.GetChildObjectByName(_gameOverUI.gameObject, AllTagNames.Exit);

        _confirmButton = confirmButtonTf.GetComponent<ButtonManager>();
        _confirmButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenuScene");
        });
        Hide();
    }

    public void Show()
    {
        _runInfoUISystem.Hide();
        _artefactsUISystem.Hide();
        
        _gameOverUI.gameObject.SetActive(true);

        _playerDataTexts.roomsEnteredText.text = $"{Creator.gameDataSo.roomsEntered}";
        _playerDataTexts.piecesCapturedText.text = $"{Creator.gameDataSo.piecesCaptured}";
        string hasSatEnd = Creator.gameDataSo.bestTurn == 1 ? "" : "s";
        _playerDataTexts.bestTurnText.text = $"{Creator.gameDataSo.bestTurn} piece{hasSatEnd}";
        _playerDataTexts.chainCompletesText.text = $"{Creator.gameDataSo.chainCompletes}";
        _playerDataTexts.seedUsedText.text = $"{Creator.gameDataSo.seedUsed}";
        _playerDataTexts.mostUsedPieceText.text = $"{Creator.gameDataSo.GetMostUsedPiece()}";
        _playerDataTexts.capturedByText.text = $"{Creator.gameDataSo.capturedByPiece}";
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
