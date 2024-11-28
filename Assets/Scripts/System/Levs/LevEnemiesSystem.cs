using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class LevEnemiesSystem : LevDependency
{
    private LevChainUISystem _chainUISystem;
    private LevDoorsSystem _doorsSystem;
    
    private List<LevEnemyController> _enemyControllers = new ();

    private List<Vector3> _positionsTakenByOtherEnemies = new();
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        if (levCreator.NewTryGetDependency(out LevChainUISystem levChainUISystem))
        {
            _chainUISystem = levChainUISystem;
        }
        if (Creator.NewTryGetDependency(out LevDoorsSystem levDoorsSystem))
        {
            _doorsSystem = levDoorsSystem;
        }

        foreach (GameObject enemySpawnPosition in GameObject.FindGameObjectsWithTag("EnemySpawnPosition"))
        {
            LevEnemyController enemyController = new LevEnemyController();
            enemyController.GameStart(levCreator);
            Piece piece = enemySpawnPosition.GetComponent<EnemyPiece>().piece;
            
            //Get the order of the doors, in ascending order based on y-value
            List<SingleDoorPosition> doors = _doorsSystem.GetDoorPositions().ToList();
            doors.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
            
            enemyController.SetEnemyInstance(enemySpawnPosition.transform.position, piece, doors);
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

    public void PieceCaptured(LevEnemyController enemyController, int roomNumber)
    {
        _chainUISystem.ShowNewPiece(enemyController.GetPiece());
        
        enemyController.PieceCaptured();
        
        _enemyControllers.Remove(enemyController);

        if (IsEnemiesInRoomCleared(roomNumber))
        {
            _doorsSystem.SetRoomDoorsOpen(roomNumber);
        }
    }

    public void SetStateForAllEnemies(LevEnemyController.States state)
    {
        foreach (LevEnemyController enemyController in _enemyControllers)
        {
            if (enemyController.GetState() != LevEnemyController.States.Captured)
            {
                enemyController.SetState(state);
            }
        }
    }

    public bool IsEnemiesInRoomCleared(int roomNumber)
    {
        int noOfEnemiesInRoom = 0;
        foreach (LevEnemyController enemyController in _enemyControllers)
        {
            if (enemyController.GetRoomNumber() == roomNumber)
            {
                noOfEnemiesInRoom++;
            }
        }

        return noOfEnemiesInRoom == 0;
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
