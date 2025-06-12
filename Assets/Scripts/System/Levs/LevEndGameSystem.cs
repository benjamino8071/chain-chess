using UnityEngine;

public class LevEndGameSystem : LevDependency
{
    private LevGameOverUISystem _gameOverUISystem;
    private LevLevelCompleteUISystem _levelCompleteUISystem;
    private LevSideWinsUISystem _sideWinsUISystem;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _gameOverUISystem = levCreator.GetDependency<LevGameOverUISystem>();
        _levelCompleteUISystem = levCreator.GetDependency<LevLevelCompleteUISystem>();
        _sideWinsUISystem = levCreator.GetDependency<LevSideWinsUISystem>();
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
    }
}
