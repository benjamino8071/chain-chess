using UnityEngine;

public class LevTurnSystem : LevDependency
{
    private LevPlayerSystem _playerSystem;
    private LevEnemiesSystem _enemiesSystem;
    
    public enum Turn
    {
        Player,
        Enemy
    }

    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _playerSystem = levCreator.GetDependency<LevPlayerSystem>();
        _enemiesSystem = levCreator.GetDependency<LevEnemiesSystem>();
    }

    public void SwitchTurn(Turn nextTurn)
    {
        switch (nextTurn)
        {
            case Turn.Player:
                _enemiesSystem.ClearPositionsTakenByOtherEnemiesForThisTurn();
                _playerSystem.SetStateForAllPlayers(LevPlayerController.States.Idle);
                break;
            case Turn.Enemy:
                _enemiesSystem.SetStateForRandomEnemy(LevEnemyController.States.ChooseTile);
                break;
        }
    }
}
