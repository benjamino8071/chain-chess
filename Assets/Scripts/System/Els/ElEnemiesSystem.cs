using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class ElEnemiesSystem : ElDependency
{
    private ElChainUISystem _capturedPiecesUISystem;
    private ElDoorsSystem _doorsSystem;
    
    private List<ElEnemyController> _enemyControllers = new ();

    private List<Vector3> _positionsTakenByOtherEnemies = new();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        if (Creator.NewTryGetDependency(out ElChainUISystem capturedPiecesUISystem))
        {
            _capturedPiecesUISystem = capturedPiecesUISystem;
        }
        if (Creator.NewTryGetDependency(out ElDoorsSystem doorsSystem))
        {
            _doorsSystem = doorsSystem;
        }

        foreach (GameObject enemySpawnPosition in GameObject.FindGameObjectsWithTag("EnemySpawnPosition"))
        {
            //Do NOT create enemy controllers for the enemies in rooms before us
            //To get the room number of the spawn position we divide by 11
            int roomNumber = (int)(enemySpawnPosition.transform.position.y / 11f);
            if(roomNumber < Creator.playerSystemSo.roomNumberSaved) continue;
            
            ElEnemyController enemyController = new ElEnemyController();
            enemyController.GameStart(elCreator);
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
        foreach (ElEnemyController enemyController in _enemyControllers)
        {
            enemyController.GameUpdate(dt);
        }
    }

    public bool IsEnemyAtThisPosition(Vector3 position)
    {
        foreach (ElEnemyController enemyController in _enemyControllers)
        {
            if (enemyController.GetPosition() == position)
                return true;
        }

        return false;
    }

    public bool TryGetEnemyAtPosition(Vector3 position, out ElEnemyController enemyController)
    {
        foreach (ElEnemyController enemCont in _enemyControllers)
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

    public void PieceCaptured(ElEnemyController enemyController, int roomNumber)
    {
        _capturedPiecesUISystem.ShowNewPiece(enemyController.GetPiece());
        
        enemyController.PieceCaptured();
        
        _enemyControllers.Remove(enemyController);

        if (IsEnemiesInRoomCleared(roomNumber))
        {
            _doorsSystem.SetRoomDoorsOpen(roomNumber);
        }
    }

    public void SetStateForAllEnemies(ElEnemyController.States state)
    {
        foreach (ElEnemyController enemyController in _enemyControllers)
        {
            if (enemyController.GetState() != ElEnemyController.States.Captured)
            {
                enemyController.SetState(state);
            }
        }
    }

    public bool IsEnemiesInRoomCleared(int roomNumber)
    {
        int noOfEnemiesInRoom = 0;
        foreach (ElEnemyController enemyController in _enemyControllers)
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
