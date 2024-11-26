using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class EnemiesSystem : ElDependency
{
    private CapturedPiecesUISystem _capturedPiecesUISystem;
    private DoorsSystem _doorsSystem;
    
    private List<EnemyController> _enemyControllers = new ();

    private List<Vector3> _positionsTakenByOtherEnemies = new();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        if (Creator.TryGetDependency("CapturedPiecesUISystem", out CapturedPiecesUISystem capturedPiecesUISystem))
        {
            _capturedPiecesUISystem = capturedPiecesUISystem;
        }
        if (Creator.TryGetDependency("DoorsSystem", out DoorsSystem doorsSystem))
        {
            _doorsSystem = doorsSystem;
        }

        foreach (GameObject enemySpawnPosition in GameObject.FindGameObjectsWithTag("EnemySpawnPosition"))
        {
            //Do NOT create enemy controllers for the enemies in rooms before us
            //To get the room number of the spawn position we divide by 11
            int roomNumber = (int)(enemySpawnPosition.transform.position.y / 11f);
            if(roomNumber < Creator.playerSystemSo.roomNumberSaved) continue;
            
            EnemyController enemyController = new EnemyController();
            enemyController.GameStart(elCreator);
            Piece piece = enemySpawnPosition.GetComponent<EnemyPiece>().piece;
            
            //Get the order of the doors, in ascending order based on y-value
            List<SingleDoorPosition> doors = _doorsSystem.GetDoorPositions().ToList();
            doors.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
            
            Debug.Log("Doors: "+doors.Count);
            
            enemyController.SetEnemyInstance(enemySpawnPosition.transform.position, piece, doors);
            _enemyControllers.Add(enemyController);
        }
    }

    public override void GameUpdate(float dt)
    {
        foreach (EnemyController enemyController in _enemyControllers)
        {
            enemyController.GameUpdate(dt);
        }
    }

    public bool IsEnemyAtThisPosition(Vector3 position)
    {
        foreach (EnemyController enemyController in _enemyControllers)
        {
            if (enemyController.GetPosition() == position)
                return true;
        }

        return false;
    }

    public bool TryGetEnemyAtPosition(Vector3 position, out EnemyController enemyController)
    {
        foreach (EnemyController enemCont in _enemyControllers)
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

    public void PieceCaptured(EnemyController enemyController, int roomNumber)
    {
        _capturedPiecesUISystem.ShowNewPiece(enemyController.GetPiece());
        
        enemyController.PieceCaptured();
        
        _enemyControllers.Remove(enemyController);

        if (IsEnemiesInRoomCleared(roomNumber))
        {
            _doorsSystem.SetRoomDoorsOpen(roomNumber);
        }
    }

    public void SetStateForAllEnemies(EnemyController.States state)
    {
        foreach (EnemyController enemyController in _enemyControllers)
        {
            if (enemyController.GetState() != EnemyController.States.Captured)
            {
                enemyController.SetState(state);
            }
        }
    }

    public bool IsEnemiesInRoomCleared(int roomNumber)
    {
        int noOfEnemiesInRoom = 0;
        foreach (EnemyController enemyController in _enemyControllers)
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
