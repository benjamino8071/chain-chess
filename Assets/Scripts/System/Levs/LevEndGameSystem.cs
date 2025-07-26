using UnityEngine;

public class LevEndGameSystem : LevDependency
{
    private LevGameOverUISystem _gameOverUISystem;
    private LevLevelCompleteUISystem _levelCompleteUISystem;
    private LevSideWinsUISystem _sideWinsUISystem;
    private LevTurnSystem _turnSystem;
    private LevSettingsUISystem _settingsUISystem;
    private LevChainUISystem _chainUISystem;
    private LevValidMovesSystem _validMovesSystem;
    private LevBoardSystem _boardSystem;

    public bool isEndGame => _isEndGame;
    
    private bool _isEndGame;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _gameOverUISystem = levCreator.GetDependency<LevGameOverUISystem>();
        _levelCompleteUISystem = levCreator.GetDependency<LevLevelCompleteUISystem>();
        _sideWinsUISystem = levCreator.GetDependency<LevSideWinsUISystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        _settingsUISystem = levCreator.GetDependency<LevSettingsUISystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
        _boardSystem = levCreator.GetDependency<LevBoardSystem>();
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
