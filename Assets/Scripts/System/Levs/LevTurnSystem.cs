using TMPro;
using UnityEngine;

public class LevTurnSystem : LevDependency
{
    private LevPlayerSystem _playerSystem;
    private LevEnemiesSystem _enemiesSystem;

    private TextMeshProUGUI _turnText;
    
    public enum Turn
    {
        Player,
        Enemy
    }
    private Turn _currentTurn = Turn.Player; //Will always start with the player

    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _playerSystem = levCreator.GetDependency<LevPlayerSystem>();
        _enemiesSystem = levCreator.GetDependency<LevEnemiesSystem>();

        Transform turnText = levCreator.GetFirstObjectWithName(AllTagNames.TurnInfoText);
        _turnText = turnText.GetComponent<TextMeshProUGUI>();
    }

    public void SwitchTurn(Turn nextTurn)
    {
        switch (nextTurn)
        {
            case Turn.Player:
                _enemiesSystem.ClearPositionsTakenByOtherEnemiesForThisTurn();
                _playerSystem.SetStateForAllPlayers(LevPlayerController.States.Idle);
                _turnText.text = "White turn";
                break;
            case Turn.Enemy:
                _playerSystem.UnselectPiece();
                _enemiesSystem.SetStateForRandomEnemy(LevEnemyController.States.ChooseTile);
                _turnText.text = "Black turn";
                break;
        }
        _currentTurn = nextTurn;
    }

    public Turn CurrentTurn()
    {
        return _currentTurn;
    }
}
