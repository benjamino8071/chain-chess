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

    private int _playerTurnsRemaining;
    private int _maxPlayerTurns = 6;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();
        _enemiesSystem = elCreator.GetDependency<ElEnemiesSystem>();
        
        ResetPlayerTurnsAmount();
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
                _playerSystem.SetState(ElPlayerSystem.States.Idle);
                break;
            case Turn.Enemy:
                _playerTurnsRemaining--;
                Debug.Log("TURNS REMAINING: "+_playerTurnsRemaining);
                
                if (_playerTurnsRemaining <= 0)
                {
                    _playerSystem.SetState(ElPlayerSystem.States.Captured);

                }
                else
                {
                    _enemiesSystem.SetStateForAllEnemies(ElEnemyController.States.ChooseTile);
                }
                break;
        }
        _currentTurn = nextTurn;
    }

    public Turn GetTurn()
    {
        return _currentTurn;
    }

    public void ResetPlayerTurnsAmount()
    {
        _playerTurnsRemaining = _maxPlayerTurns;
    }
}
