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
        
        SwitchTurn(PieceColour.White);
    }
    
    public void SwitchTurn(PieceColour nextTurn)
    {
        _currentTurn = nextTurn;
        switch (nextTurn)
        {
            case PieceColour.White:
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
        
        _whiteSystem.SpawnPieces();
        _blackSystem.SpawnPieces();
        
        SwitchTurn(PieceColour.White);
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
