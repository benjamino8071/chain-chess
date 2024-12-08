using UnityEngine;

public class ElTurnSystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    private ElEnemiesSystem _enemiesSystem;
    
    public enum Turn
    {
        Player,
        Enemy
    }
    private Turn _currentTurn;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.NewTryGetDependency(out ElPlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }

        if (Creator.NewTryGetDependency(out ElEnemiesSystem enemiesSystem))
        {
            _enemiesSystem = enemiesSystem;
        }
    }

    public void SwitchTurn(Turn nextTurn)
    {
        if(_playerSystem.GetState() == ElPlayerSystem.States.Captured)
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
                _playerSystem.SetState(ElPlayerSystem.States.Idle);
                break;
            case Turn.Enemy:
                _enemiesSystem.SetStateForAllEnemies(ElEnemyController.States.ChooseTile);
                break;
        }
        _currentTurn = nextTurn;
    }

    public Turn GetTurn()
    {
        return _currentTurn;
    }
}
