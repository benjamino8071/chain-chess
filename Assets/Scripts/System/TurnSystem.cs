using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystem : Dependency
{
    private WhiteSystem _whiteSystem;
    private BlackSystem _blackSystem;
    private SettingsUISystem _settingsUISystem;
    private ValidMovesSystem _validMovesSystem;
    private ChainUISystem _chainUISystem;
    private EndGameSystem _endGameSystem;
    private BoardSystem _boardSystem;
    private LevelSelectUISystem _levelSelectUISystem;
    private InvalidMovesSystem _invalidMovesSystem;

    private TextMeshProUGUI _turnsRemainingText;
    
    private PieceColour _currentTurn; //Will always start with White
    
    private int _turnsRemaining;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _whiteSystem = creator.GetDependency<WhiteSystem>();
        _blackSystem = creator.GetDependency<BlackSystem>();
        _settingsUISystem = creator.GetDependency<SettingsUISystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _chainUISystem = creator.GetDependency<ChainUISystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _levelSelectUISystem = creator.GetDependency<LevelSelectUISystem>();
        _invalidMovesSystem = creator.GetDependency<InvalidMovesSystem>();
        
        Transform guiBottom = creator.GetFirstObjectWithName(AllTagNames.GUIBottom);
        
        _turnsRemainingText = creator.GetChildComponentByName<TextMeshProUGUI>(guiBottom.gameObject, AllTagNames.TurnsRemaining);
        
        //Add 1 because we will lose 1 when the player 
        _turnsRemaining = creator.levelsSo.GetLevelOnLoad().turns;
        
        UpdateTurnsRemainingText(_turnsRemaining);
        
        LoadLevelRuntime();
    }
    
    public void SwitchTurn(PieceColour nextTurn)
    {
        _currentTurn = nextTurn;
        switch (nextTurn)
        {
            case PieceColour.White:
                if (DecrementTurnsRemaining())
                {
                    _blackSystem.Lose(GameOverReason.NoTurns, 1.1f);
                    return;
                }
                
                Creator.statsTurns++;
                
                _blackSystem.DeselectPiece();
                _validMovesSystem.UpdateValidMoves(_whiteSystem.pieceControllerSelected.GetAllValidMovesOfCurrentPiece());
                
                _whiteSystem.SetStateForAllPieces(PieceController.States.FindingMove);
                
                _chainUISystem.ShowChain(_whiteSystem.pieceControllerSelected);
                
                _settingsUISystem.ShowButton();
                break;
            case PieceColour.Black:
                _settingsUISystem.HideButton();
                
                _validMovesSystem.HideAllValidMoves();
                _blackSystem.AiSetup();
                break;
        }
    }
    
    public void LoadLevelRuntime()
    {
        Random.InitState(42);
       
        _whiteSystem.Clean();
        _blackSystem.Clean();
        
        _validMovesSystem.HideAllValidMoves();
        _invalidMovesSystem.HideAll();
        
        _levelSelectUISystem.UpdateLevelText(Creator.levelsSo.levelOnLoad);
        
        _turnsRemaining = Creator.levelsSo.GetLevelOnLoad().turns;
        UpdateTurnsRemainingText(_turnsRemaining);
        
        _whiteSystem.SpawnPieces();
        _blackSystem.SpawnPieces();
        _chainUISystem.ShowChain(_whiteSystem.pieceControllerSelected);
        _boardSystem.ShowTapPoint(_whiteSystem.pieceControllerSelected.piecePos);
        _validMovesSystem.UpdateValidMoves(_whiteSystem.pieceControllerSelected.GetAllValidMovesOfCurrentPiece());
        
        _endGameSystem.ResetEndGame();

        Creator.statsTurns = 0;
        Creator.statsMoves = 0;
        Creator.statsTime = 0;
        
        SwitchTurn(PieceColour.White);
    }

    private void UpdateTurnsRemainingText(int turnsRemaining)
    {
        string plural = turnsRemaining == 1 ? "" : "s";
        
        _turnsRemainingText.text = $"{turnsRemaining} Turn{plural}";
    }

    private bool DecrementTurnsRemaining()
    {
        _turnsRemaining--;
        UpdateTurnsRemainingText(_turnsRemaining);
        return _turnsRemaining == 0;
    }

    public PieceColour CurrentTurn()
    {
        return _currentTurn;
    }
}
