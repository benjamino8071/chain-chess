using Michsky.MUIP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElPlayerWonUISystem : ElDependency
{
    private ElRunInfoUISystem _runInfoUISystem;
    private ElArtefactsUISystem _artefactsUISystem;
    
    private Transform _playerWonUI;

    private PlayerDataTexts _playerDataTexts;
    
    private ButtonManager _confirmButton;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        _runInfoUISystem = elCreator.GetDependency<ElRunInfoUISystem>();
        _artefactsUISystem = elCreator.GetDependency<ElArtefactsUISystem>();

        _playerWonUI = elCreator.GetFirstObjectWithName(AllTagNames.PlayerWon);
        
        _playerDataTexts = _playerWonUI.GetComponent<PlayerDataTexts>();
        
        Transform confirmButtonTf = elCreator.GetChildObjectByName(_playerWonUI.gameObject, AllTagNames.Exit);

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
        
        _playerWonUI.gameObject.SetActive(true);
        
        _playerDataTexts.roomsEnteredText.text = $"{Creator.gameDataSo.roomsEntered}";
        _playerDataTexts.piecesCapturedText.text = $"{Creator.gameDataSo.piecesCaptured}";
        string hasSatEnd = Creator.gameDataSo.bestTurn == 1 ? "" : "s";
        _playerDataTexts.bestTurnText.text = $"{Creator.gameDataSo.bestTurn} piece{hasSatEnd}";
        _playerDataTexts.chainCompletesText.text = $"{Creator.gameDataSo.chainCompletes}";
        _playerDataTexts.seedUsedText.text = $"{Creator.gameDataSo.seedUsed}";
        _playerDataTexts.mostUsedPieceText.text = $"{Creator.gameDataSo.GetMostUsedPiece()}";
    }

    public void Hide()
    {
        _playerWonUI.gameObject.SetActive(false);
    }
}
