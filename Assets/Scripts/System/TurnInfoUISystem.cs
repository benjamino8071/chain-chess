using TMPro;
using UnityEngine;

public class TurnInfoUISystem : Dependency
{
    private PlayerSystem _playerSystem;
    private EnemiesSystem _enemiesSystem;
    private DoorsSystem _doorsSystem;
    
    public TextMeshProUGUI _turnInfoText;

    public enum Turn
    {
        Player,
        Enemy
    }
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        if (_creator.TryGetDependency("PlayerSystem", out PlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }
        if (_creator.TryGetDependency("EnemiesSystem", out EnemiesSystem enemiesSystem))
        {
            _enemiesSystem = enemiesSystem;
        }
        if (_creator.TryGetDependency("DoorsSystem", out DoorsSystem doorsSystem))
        {
            _doorsSystem = doorsSystem;
        }
        
        _turnInfoText = GameObject.FindWithTag("LevelText").GetComponent<TextMeshProUGUI>();
    }

    public override void GameUpdate(float dt)
    {
        int roomNumber = _playerSystem.GetRoomNumber();
        if (roomNumber < 8)
        {
            if (roomNumber == 7)
            {
                _turnInfoText.text = "FINAL ROOM";
            }
            else
            {
                _turnInfoText.text = "Room " + (_playerSystem.GetRoomNumber() + 1);
            }
        }
    }

    public void SwitchTurn(Turn nextTurn)
    {
        if(_playerSystem.GetState() == PlayerSystem.States.Captured)
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
                _playerSystem.SetState(PlayerSystem.States.Idle);
                break;
            case Turn.Enemy:
                _enemiesSystem.SetStateForAllEnemies(EnemyController.States.ChooseTile);
                break;
        }
    }
}
