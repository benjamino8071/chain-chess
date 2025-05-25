using TMPro;
using UnityEngine;

public class LevTurnSystem : LevDependency
{
    private LevPlayerSystem _playerSystem;
    private LevEnemiesSystem _enemiesSystem;
    private LevPauseUISystem _pauseUISystem;

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
        _pauseUISystem = levCreator.GetDependency<LevPauseUISystem>();

        Transform turnText = levCreator.GetFirstObjectWithName(AllTagNames.TurnInfoText);
        _turnText = turnText.GetComponent<TextMeshProUGUI>();
        
        SetTurnText("White");
    }
    
    public void SwitchTurn(Turn nextTurn)
    {
        switch (nextTurn)
        {
            case Turn.Player:
                _enemiesSystem.ClearPositionsTakenByOtherEnemiesForThisTurn();
                _playerSystem.SetStateForAllPlayers(LevPlayerController.States.Idle);
                _pauseUISystem.ShowButton();
                SetTurnText("White");
                break;
            case Turn.Enemy:
                _pauseUISystem.HideButton();
                _playerSystem.UnselectPiece();
                _enemiesSystem.SetStateForRandomEnemy(LevEnemyController.States.ChooseTile);
                SetTurnText("Black");
                break;
        }
        _currentTurn = nextTurn;
    }

    public Turn CurrentTurn()
    {
        return _currentTurn;
    }

    private void SetTurnText(string turnText)
    {
        _turnText.text = turnText;
    }
}
