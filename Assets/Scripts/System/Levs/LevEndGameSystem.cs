using UnityEngine;

public class LevEndGameSystem : LevDependency
{
    private LevGameOverUISystem _gameOverUISystem;
    private LevLevelCompleteUISystem _levelCompleteUISystem;
    private LevSideWinsUISystem _sideWinsUISystem;
    private LevTurnSystem _turnSystem;
    private LevPauseUISystem _pauseUISystem;
    private LevChainUISystem _chainUISystem;
    private LevValidMovesSystem _validMovesSystem;

    public bool isEndGame => _isEndGame;
    
    private bool _isEndGame;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _gameOverUISystem = levCreator.GetDependency<LevGameOverUISystem>();
        _levelCompleteUISystem = levCreator.GetDependency<LevLevelCompleteUISystem>();
        _sideWinsUISystem = levCreator.GetDependency<LevSideWinsUISystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        _pauseUISystem = levCreator.GetDependency<LevPauseUISystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
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
            if (winningColour == PieceColour.White)
            {
                if (Creator.whiteControlledBy == ControlledBy.Player)
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
                if (Creator.blackControlledBy == ControlledBy.Player)
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
        }
        else
        {
            _sideWinsUISystem.Show(winningColour);
        }
        _turnSystem.HideEndTurnButton();
        _pauseUISystem.HideButton();
        _chainUISystem.UnsetChain();
        _validMovesSystem.HideSelectedBackground();
        _validMovesSystem.HideAllValidMoves();
        _isEndGame = true;
    }

    public void ResetEndGame()
    {
        _isEndGame = false;
    }
}
