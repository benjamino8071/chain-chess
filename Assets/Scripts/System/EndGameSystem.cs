public class EndGameSystem : Dependency
{
    private GameOverUISystem _gameOverUISystem;
    private LevelCompleteUISystem _levelCompleteUISystem;
    private SettingsUISystem _settingsUISystem;
    private ChainUISystem _chainUISystem;
    private ValidMovesSystem _validMovesSystem;
    private BoardSystem _boardSystem;

    public bool isEndGame => _isEndGame;
    
    private bool _isEndGame;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _gameOverUISystem = creator.GetDependency<GameOverUISystem>();
        _levelCompleteUISystem = creator.GetDependency<LevelCompleteUISystem>();
        _settingsUISystem = creator.GetDependency<SettingsUISystem>();
        _chainUISystem = creator.GetDependency<ChainUISystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
    }

    public void SetEndGame(PieceColour winningColour, GameOverReason gameOverReason, float delayTimer)
    {
        _boardSystem.activeSideSystem.ForceDeselectPiece();
        _boardSystem.inactiveSideSystem.ForceDeselectPiece();

        if (winningColour == PieceColour.White)
        {
            _levelCompleteUISystem.Show(delayTimer);
        }
        else
        {
            _gameOverUISystem.Show(gameOverReason);
        }
        
        _settingsUISystem.HideButton();
        _boardSystem.HideTapPoint();
        _validMovesSystem.HideAllValidMoves();
        _isEndGame = true;
    }

    public void ResetEndGame()
    {
        _isEndGame = false;
    }
}
