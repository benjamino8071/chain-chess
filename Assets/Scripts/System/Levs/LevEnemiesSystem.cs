using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class LevEnemiesSystem : LevDependency
{
    private LevChainUISystem _chainUISystem;
    private LevLevelCompleteUISystem _levelCompleteUISystem;
    private LevPlayerSystem _playerSystem;
    
    private List<LevEnemyController> _enemyControllers = new ();

    private List<Vector3> _positionsTakenByOtherEnemies = new();
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _levelCompleteUISystem = levCreator.GetDependency<LevLevelCompleteUISystem>();
        _playerSystem = levCreator.GetDependency<LevPlayerSystem>();

        List<Transform> enemySpawnPositions = levCreator.GetObjectsByName(AllTagNames.EnemySpawnPosition);
        foreach (Transform enemySpawnPosition in enemySpawnPositions)
        {
            LevEnemyController enemyController = new LevEnemyController();
            enemyController.GameStart(levCreator);
            Piece piece = enemySpawnPosition.GetComponent<PieceType>().piece;
            
            enemyController.SetEnemyInstance(enemySpawnPosition.transform.position, piece);
            _enemyControllers.Add(enemyController);
        }
    }

    public override void GameUpdate(float dt)
    {
        foreach (LevEnemyController enemyController in _enemyControllers)
        {
            enemyController.GameUpdate(dt);
        }
    }

    public bool IsEnemyAtThisPosition(Vector3 position)
    {
        foreach (LevEnemyController enemyController in _enemyControllers)
        {
            if (enemyController.GetPosition() == position)
                return true;
        }

        return false;
    }

    public bool TryGetEnemyAtPosition(Vector3 position, out LevEnemyController enemyController)
    {
        foreach (LevEnemyController enemCont in _enemyControllers)
        {
            if (enemCont.GetPosition() == position)
            {
                enemyController = enemCont;
                return true;
            }
        }

        enemyController = default;
        return false;
    }

    public void PieceCaptured(LevEnemyController enemyController)
    {
        _chainUISystem.ShowNewPiece(enemyController.GetPiece());
        
        enemyController.PieceCaptured();
        
        _enemyControllers.Remove(enemyController);

        if (IsEnemiesInRoomCleared())
        {
            _playerSystem.SetStateForAllPlayers(LevPlayerController.States.EndGame);
            _levelCompleteUISystem.Show();
        }
    }

    public void SetStateForRandomEnemy(LevEnemyController.States state)
    {
        int enemySelectedIndex = Random.Range(0, _enemyControllers.Count);
        LevEnemyController enemySelected = _enemyControllers[enemySelectedIndex];
        enemySelected.SetState(state);
    }

    public void SetStateForAllEnemies(LevEnemyController.States state)
    {
        foreach (LevEnemyController enemyController in _enemyControllers)
        {
            if (enemyController.state != LevEnemyController.States.Captured)
            {
                enemyController.SetState(state);
            }
        }
    }

    public bool IsEnemiesInRoomCleared()
    {
        return _enemyControllers.Count == 0;
    }

    public void AddPositionTakenByEnemyForThisTurn(Vector3 positionTakenByEnemy)
    {
        _positionsTakenByOtherEnemies.Add(positionTakenByEnemy);
    }

    public bool IsPositionTakenByOtherEnemyForThisTurn(Vector3 position)
    {
        return _positionsTakenByOtherEnemies.Contains(position);
    }

    public void ClearPositionsTakenByOtherEnemiesForThisTurn()
    {
        _positionsTakenByOtherEnemies.Clear();
    }
}
