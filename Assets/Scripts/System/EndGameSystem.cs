public class EndGameSystem : Dependency
{
    private ValidMovesSystem _validMovesSystem;
    private BoardSystem _boardSystem;
    private UISystem _uiSystem;
    
    public bool isEndGame => _isEndGame;
    
    private bool _isEndGame;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _uiSystem = creator.GetDependency<UISystem>();
    }

    public void SetEndGame(PieceColour winningColour, GameOverReason gameOverReason, float delayTimer)
    {
        if (winningColour == PieceColour.White)
        {
            _uiSystem.ShowRightSideUI(AllTagNames.UILevelComplete);
        }
        else
        {
            UIGameOver uiGameOver =_uiSystem.GetUI<UIGameOver>();
            uiGameOver.SetUI(gameOverReason);
            
            _uiSystem.ShowRightSideUI(AllTagNames.UIGameOver);
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
