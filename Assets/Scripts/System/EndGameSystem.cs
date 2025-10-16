using System.Collections.Generic;

public class EndGameSystem : Dependency
{
    private TurnSystem _turnSystem;
    private ValidMovesSystem _validMovesSystem;
    private BoardSystem _boardSystem;
    private UISystem _uiSystem;
    
    public bool isEndGame => _isEndGame;
    
    private bool _isEndGame;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _turnSystem = creator.GetDependency<TurnSystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _uiSystem = creator.GetDependency<UISystem>();
    }

    public void SetEndGame(PieceColour winningColour, GameOverReason gameOverReason)
    {
        if (winningColour == PieceColour.White)
        {
            _uiSystem.ShowRightTopSideUI(AllTagNames.UILevelComplete);
        }
        else
        {
            List<UIGameOver> uiGameOver =_uiSystem.GetUI<UIGameOver>();
            foreach (UIGameOver gameOver in uiGameOver)
            {
                gameOver.SetUI(gameOverReason);
            }
            
            _uiSystem.ShowRightTopSideUI(AllTagNames.UIGameOver);
        }
        
        _boardSystem.HideTapPoint();
        _validMovesSystem.HideAllValidMoves();
        _isEndGame = true;
    }

    public void ResetEndGame()
    {
        _isEndGame = false;
    }
}
