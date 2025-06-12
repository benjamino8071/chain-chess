using TMPro;
using UnityEngine;

public class LevTurnSystem : LevDependency
{
    private LevWhiteSystem _whiteSystem;
    private LevBlackSystem _blackSystem;
    private LevPauseUISystem _pauseUISystem;
    private LevValidMovesSystem _validMovesSystem;
    private LevChainUISystem _chainUISystem;

    private TextMeshProUGUI _turnText;
    
    private PieceColour _currentTurn; //Will always start with White

    private int _turnsRemaining;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _whiteSystem = levCreator.GetDependency<LevWhiteSystem>();
        _blackSystem = levCreator.GetDependency<LevBlackSystem>();
        _pauseUISystem = levCreator.GetDependency<LevPauseUISystem>();
        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();

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
                
                _blackSystem.UnselectPiece();
                if (Creator.whiteControlledBy == ControlledBy.Player)
                {
                    _whiteSystem.SetStateForAllPieces(LevPieceController.States.FindingMove);
                    _pauseUISystem.ShowButton();
                }
                else
                {
                    _pauseUISystem.HideButton();
                    _whiteSystem.SelectRandomPiece();
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
                
                _whiteSystem.UnselectPiece();
                if (Creator.blackControlledBy == ControlledBy.Player)
                {
                    _blackSystem.SetStateForAllPieces(LevPieceController.States.FindingMove);
                    _pauseUISystem.ShowButton();
                }
                else
                {
                    _pauseUISystem.HideButton();
                    _blackSystem.SelectRandomPiece();
                }
                SetTurnText("Black");
                break;
        }
    }
    
    public void LoadLevelRuntime()
    {
        _whiteSystem.Clean();
        _blackSystem.Clean();
        
        _validMovesSystem.HideAllValidMoves();
        _chainUISystem.UnsetChain();
        
        Creator.UpdateLevelText();
        
        _turnsRemaining = Creator.levelsSo.GetLevelOnLoad().turns;
        Creator.UpdateTurnsRemainingText(_turnsRemaining);
        
        _whiteSystem.SpawnPieces();
        _blackSystem.SpawnPieces();
        
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
