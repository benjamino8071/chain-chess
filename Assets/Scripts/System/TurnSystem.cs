using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystem : Dependency
{
    public int turnsRemaining => _turnsRemaining;
    
    private WhiteSystem _whiteSystem;
    private BlackSystem _blackSystem;
    private SettingsUISystem _settingsUISystem;
    private ValidMovesSystem _validMovesSystem;
    private ChainUISystem _chainUISystem;
    private EndGameSystem _endGameSystem;
    private BoardSystem _boardSystem;
    private LevelSelectUISystem _levelSelectUISystem;
    private InvalidMovesSystem _invalidMovesSystem;

    private TextMeshProUGUI _activeSideText;
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
        
        _activeSideText = creator.GetChildComponentByName<TextMeshProUGUI>(guiBottom.gameObject, AllTagNames.TurnInfoText);
        
        _turnsRemainingText = creator.GetChildComponentByName<TextMeshProUGUI>(guiBottom.gameObject, AllTagNames.TurnsRemaining);
        
        //Add 1 because we will lose 1 when the player 
        _turnsRemaining = creator.levelsSo.GetLevelOnLoad().turns;
        
        UpdateTurnsRemainingText(_turnsRemaining);
        
        SwitchTurn(PieceColour.White);
    }
    
    public void SwitchTurn(PieceColour nextTurn)
    {
        _currentTurn = nextTurn;
        switch (nextTurn)
        {
            case PieceColour.White:
                if (Creator.blackControlledBy == ControlledBy.Player)
                {
                    if (DecrementTurnsRemaining())
                    {
                        _blackSystem.Lose(GameOverReason.NoTurns);
                        return;
                    }
                }
                
                if (Creator.whiteControlledBy == ControlledBy.Player)
                {
                    Creator.statsTurns++;
                }
                
                _chainUISystem.HideChain();
                _boardSystem.HideTapPoint();
                _blackSystem.DeselectPiece();
                if (Creator.whiteControlledBy == ControlledBy.Player)
                {
                    _whiteSystem.SetStateForAllPieces(PieceController.States.FindingMove);
                    _settingsUISystem.ShowButton();
                }
                else
                {
                    _settingsUISystem.HideButton();
                    _whiteSystem.AiSetup();
                }
                SetTurnText("White");
                break;
            case PieceColour.Black:
                if (Creator.whiteControlledBy == ControlledBy.Player)
                {
                    if (DecrementTurnsRemaining())
                    {
                        _whiteSystem.Lose(GameOverReason.NoTurns);
                        return;
                    }
                }
                
                //Player will ALWAYS be white in puzzles. So no need to check if we should increment Creator.statsTurns
                
                _chainUISystem.HideChain();
                _boardSystem.HideTapPoint();
                _whiteSystem.DeselectPiece();
                if (Creator.blackControlledBy == ControlledBy.Player)
                {
                    _blackSystem.SetStateForAllPieces(PieceController.States.FindingMove);
                    _settingsUISystem.ShowButton();
                }
                else
                {
                    _settingsUISystem.HideButton();
                    _blackSystem.AiSetup();
                }
                SetTurnText("Black");
                break;
        }
    }
    
    public void LoadLevelRuntime()
    {
        Creator.piecesSo.ResetSystemRandom();
        Random.InitState(42);
       
        _whiteSystem.Clean();
        _blackSystem.Clean();
        
        _validMovesSystem.HideAllValidMoves();
        _invalidMovesSystem.HideAll();
        _chainUISystem.HideChain();
        
        _levelSelectUISystem.UpdateLevelText(Creator.levelsSo.levelOnLoad);
        
        _turnsRemaining = Creator.levelsSo.GetLevelOnLoad().turns;
        UpdateTurnsRemainingText(_turnsRemaining);
        
        _whiteSystem.SpawnPieces();
        _blackSystem.SpawnPieces();
        
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

    private void SetTurnText(string turnText)
    {
        _activeSideText.text = turnText;
    }
}
