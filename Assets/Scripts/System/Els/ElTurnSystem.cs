
public class ElTurnSystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    private ElEnemiesSystem _enemiesSystem;
    private ElLivesUISystem _livesUISystem;
    
    public enum Turn
    {
        Player,
        Enemy
    }
    private Turn _currentTurn;

    private int _playerTurnsInRoomCount;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();
        _enemiesSystem = elCreator.GetDependency<ElEnemiesSystem>();
        _livesUISystem = elCreator.GetDependency<ElLivesUISystem>();
        
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
            if (_playerTurnsInRoomCount == 0)
            {
                Creator.gameDataSo.chainCompletes++;
            }
            _playerSystem.SetNotInvincible();
            nextTurn = Turn.Player;
        }

        switch (nextTurn)
        {
            case Turn.Player:
                if (HasPlayerHadOneTurn())
                {
                    _playerSystem.SetNotInvincible();
                    _livesUISystem.HideInvincibleLife();
                }
                _enemiesSystem.ClearPositionsTakenByOtherEnemiesForThisTurn();
                _playerSystem.SetState(ElPlayerSystem.States.Idle);
                break;
            case Turn.Enemy:
                _playerTurnsInRoomCount++;
                
                if (_playerTurnsInRoomCount <= 0)
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
        _playerTurnsInRoomCount = 0;
        if (Creator.playerSystemSo.roomNumberSaved < 9)
        {
            _playerSystem.SetInvincible(true);
            _livesUISystem.ShowInvincibleLife(true);
        }
    }

    public bool HasPlayerHadOneTurn()
    {
        return _playerTurnsInRoomCount >= 1;
    }

    public bool HasPlayerHadTwoTurns()
    {
        return _playerTurnsInRoomCount >= 2;
    }
}
