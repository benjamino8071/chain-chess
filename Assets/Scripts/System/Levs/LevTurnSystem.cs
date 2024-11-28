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

        if (Creator.NewTryGetDependency(out LevPlayerSystem levPlayerSystem))
        {
            _playerSystem = levPlayerSystem;
        }

        if (Creator.NewTryGetDependency(out LevEnemiesSystem levEnemiesSystem))
        {
            _enemiesSystem = levEnemiesSystem;
        }
    }

    public void SwitchTurn(Turn nextTurn)
    {
        if(_playerSystem.GetState() == LevPlayerSystem.States.Captured)
            return;
        
        //If there are no enemies left in room, we force turn to stay on player
        //Also we open doors
        int roomNumber = _playerSystem.GetRoomNumber();
        if (_enemiesSystem.IsEnemiesInRoomCleared(roomNumber))
        {
            nextTurn = Turn.Player;
        }
        
        switch (nextTurn)
        {
            case Turn.Player:
                _enemiesSystem.ClearPositionsTakenByOtherEnemiesForThisTurn();
                _playerSystem.SetState(LevPlayerSystem.States.Idle);
                break;
            case Turn.Enemy:
                _enemiesSystem.SetStateForAllEnemies(LevEnemyController.States.ChooseTile);
                break;
        }
    }
}
