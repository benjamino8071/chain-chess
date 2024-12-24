using UnityEngine;

public class ElTurnSystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    private ElEnemiesSystem _enemiesSystem;
    private ElTimerUISystem _timerUISystem;
    
    public enum Turn
    {
        Player,
        Enemy
    }
    private Turn _currentTurn;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.TryGetDependency(out ElPlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }
        if (Creator.TryGetDependency(out ElEnemiesSystem enemiesSystem))
        {
            _enemiesSystem = enemiesSystem;
        }
        if (Creator.TryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
    }

    public void SwitchTurn(Turn nextTurn)
    {
        if(_playerSystem.GetState() != ElPlayerSystem.States.WaitingForTurn)
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
                _timerUISystem.ResetTimerChangedAmount(false);
                _playerSystem.SetState(ElPlayerSystem.States.Idle);
                break;
            case Turn.Enemy:
                _timerUISystem.StopTimer();
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
