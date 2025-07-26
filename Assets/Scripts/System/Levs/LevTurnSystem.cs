using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevTurnSystem : LevDependency
{
    public int turnsRemaining => _turnsRemaining;
    
    private LevWhiteSystem _whiteSystem;
    private LevBlackSystem _blackSystem;
    private LevSettingsUISystem _settingsUISystem;
    private LevValidMovesSystem _validMovesSystem;
    private LevChainUISystem _chainUISystem;
    private LevEndGameSystem _endGameSystem;
    private LevBoardSystem _boardSystem;

    private TextMeshProUGUI _turnText;
    
    private PieceColour _currentTurn; //Will always start with White
    
    private int _turnsRemaining;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _whiteSystem = levCreator.GetDependency<LevWhiteSystem>();
        _blackSystem = levCreator.GetDependency<LevBlackSystem>();
        _settingsUISystem = levCreator.GetDependency<LevSettingsUISystem>();
        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _endGameSystem = levCreator.GetDependency<LevEndGameSystem>();
        _boardSystem = levCreator.GetDependency<LevBoardSystem>();

        Transform turnText = levCreator.GetFirstObjectWithName(AllTagNames.TurnInfoText);
        _turnText = turnText.GetComponent<TextMeshProUGUI>();
        
        //Add 1 because we will lose 1 when the player 
        _turnsRemaining = levCreator.levelsSo.GetLevelOnLoad().turns;
         
        SwitchTurn(PieceColour.White);
    }
    
    public void SwitchTurn(PieceColour nextTurn)
    {
        _currentTurn = nextTurn;
        switch (nextTurn)
        {
            case PieceColour.White:
                if (Creator.blackControlledBy == ControlledBy.Player && Creator.isPuzzle)
                {
                    if (DecrementTurnsRemaining())
                    {
                        _blackSystem.Lose();
                        return;
                    }
                }
                
                if (Creator.whiteControlledBy == ControlledBy.Player && Creator.isPuzzle)
                {
                    Creator.statsTurns++;
                }
                
                _chainUISystem.HideChain();
                _boardSystem.HideTapPoint();
                _blackSystem.DeselectPiece();
                if (Creator.whiteControlledBy == ControlledBy.Player)
                {
                    _whiteSystem.SetStateForAllPieces(LevPieceController.States.FindingMove);
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
                if (Creator.whiteControlledBy == ControlledBy.Player && Creator.isPuzzle)
                {
                    if (DecrementTurnsRemaining())
                    {
                        _whiteSystem.Lose();
                        return;
                    }
                }
                
                //Player will ALWAYS be white in puzzles. So no need to check if we should increment Creator.statsTurns
                
                _chainUISystem.HideChain();
                _boardSystem.HideTapPoint();
                _whiteSystem.DeselectPiece();
                if (Creator.blackControlledBy == ControlledBy.Player)
                {
                    _blackSystem.SetStateForAllPieces(LevPieceController.States.FindingMove);
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
        Random.InitState(42);
        
        _whiteSystem.Clean();
        _blackSystem.Clean();
        
        _validMovesSystem.HideAllValidMoves();
        _chainUISystem.HideChain();
        
        Creator.UpdateLevelText();
        
        _turnsRemaining = Creator.levelsSo.GetLevelOnLoad().turns;
        Creator.UpdateTurnsRemainingText(_turnsRemaining);
        
        _whiteSystem.SpawnPieces();
        _blackSystem.SpawnPieces();
        
        _endGameSystem.ResetEndGame();

        Creator.statsTurns = 0;
        Creator.statsBestTurn = 0;
        Creator.playerFirstMoveMadeInLevel = false;
        Creator.statsTime = 0;
        
        SwitchTurn(PieceColour.White);
    }

    private bool DecrementTurnsRemaining()
    {
        _turnsRemaining--;
        Creator.UpdateTurnsRemainingText(_turnsRemaining);
        return _turnsRemaining == 0;
    }

    public PieceColour CurrentTurn()
    {
        return _currentTurn;
    }

    private void SetTurnText(string turnText)
    {
        _turnText.text = turnText;
    }
}
