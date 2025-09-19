public class EndGameSystem : Dependency
{
    private ValidMovesSystem _validMovesSystem;
    private BoardSystem _boardSystem;
    private UISystem _uiSystem;
    private UILevelComplete _uiLevelComplete;
    private UIGameOver _uiGameOver;
    
    public bool isEndGame => _isEndGame;
    
    private bool _isEndGame;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _uiSystem = creator.GetDependency<UISystem>();
        _uiLevelComplete = _uiSystem.GetUI<UILevelComplete>();
        _uiGameOver = _uiSystem.GetUI<UIGameOver>();
    }

    public void SetEndGame(PieceColour winningColour, GameOverReason gameOverReason, float delayTimer)
    {
        if (winningColour == PieceColour.White)
        {
            _uiLevelComplete.Show();
        }
        else
        {
            _uiGameOver.Show(gameOverReason);
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
