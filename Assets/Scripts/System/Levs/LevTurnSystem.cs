using TMPro;
using UnityEngine;

public class LevTurnSystem : LevDependency
{
    private LevWhiteSystem _whiteSystem;
    private LevBlackSystem _blackSystem;
    private LevPauseUISystem _pauseUISystem;

    private TextMeshProUGUI _turnText;
    
    public enum Turn
    {
        White,
        Black
    }
    private Turn _currentTurn = Turn.White; //Will always start with White

    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _whiteSystem = levCreator.GetDependency<LevWhiteSystem>();
        _blackSystem = levCreator.GetDependency<LevBlackSystem>();
        _pauseUISystem = levCreator.GetDependency<LevPauseUISystem>();

        Transform turnText = levCreator.GetFirstObjectWithName(AllTagNames.TurnInfoText);
        _turnText = turnText.GetComponent<TextMeshProUGUI>();
        
        SwitchTurn(Turn.White);
        //SetTurnText("White");
    }
    
    public void SwitchTurn(Turn nextTurn)
    {
        _currentTurn = nextTurn;
        switch (nextTurn)
        {
            case Turn.White:
                if (Creator.whiteControlledBy == ControlledBy.Player)
                {
                    _whiteSystem.SetStateForAllPieces(LevPieceController.States.FindingMove);
                    _pauseUISystem.ShowButton();
                }
                else
                {
                    _pauseUISystem.HideButton();
                    _blackSystem.UnselectPiece();
                    _whiteSystem.SelectRandomPiece();
                }
                SetTurnText("White");
                break;
            case Turn.Black:
                if (Creator.blackControlledBy == ControlledBy.Player)
                {
                    _blackSystem.SetStateForAllPieces(LevPieceController.States.FindingMove);
                    _pauseUISystem.ShowButton();
                }
                else
                {
                    _pauseUISystem.HideButton();
                    _whiteSystem.UnselectPiece();
                    _blackSystem.SelectRandomPiece();
                }
                SetTurnText("Black");
                break;
        }
    }

    public Turn CurrentTurn()
    {
        return _currentTurn;
    }

    private void SetTurnText(string turnText)
    {
        _turnText.text = turnText;
    }
}
