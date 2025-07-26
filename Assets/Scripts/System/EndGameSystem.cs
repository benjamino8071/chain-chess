public class EndGameSystem : Dependency
{
    private GameOverUISystem _gameOverUISystem;
    private LevelCompleteUISystem _levelCompleteUISystem;
    private SideWinsUISystem _sideWinsUISystem;
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
        _sideWinsUISystem = creator.GetDependency<SideWinsUISystem>();
        _settingsUISystem = creator.GetDependency<SettingsUISystem>();
        _chainUISystem = creator.GetDependency<ChainUISystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
    }

    public void SetEndGame(PieceColour winningColour)
    {
        /*
         * Purpose of this system is to check who has won/lost and display the correct info
         *
         * If this is a puzzle then we say the player has won or lost
         */

        if (Creator.isPuzzle)
        {
            if ((Creator.whiteControlledBy == ControlledBy.Player && winningColour == PieceColour.White)
                || (Creator.blackControlledBy == ControlledBy.Player && winningColour == PieceColour.Black))
            {
                //Player has WON puzzle
                _levelCompleteUISystem.Show();
            }
            else
            {
                //Player has LOST puzzle
                _gameOverUISystem.Show();
            }
        }
        else
        {
            _sideWinsUISystem.Show(winningColour);
        }
        _settingsUISystem.HideButton();
        _chainUISystem.HideChain();
        _boardSystem.HideTapPoint();
        _validMovesSystem.HideAllValidMoves();
        _isEndGame = true;
    }

    public void ResetEndGame()
    {
        _isEndGame = false;
    }
}
