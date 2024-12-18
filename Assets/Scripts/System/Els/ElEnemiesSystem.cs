using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class ElEnemiesSystem : ElDependency
{
    private ElChainUISystem _capturedPiecesUISystem;
    private ElDoorsSystem _doorsSystem;
    private ElTurnSystem _turnSystem;
    
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
        if (Creator.NewTryGetDependency(out ElTurnSystem turnSystem))
        {
            _turnSystem = turnSystem;
        }

        if (Creator.enemySo.cachedSpawnPoints.Count == 0)
        { 
            for (int roomNum = 0; roomNum < 8; roomNum++)
            {
                Dictionary<Vector3, (Piece, int)> positions = new();
                while (positions.Count < 3 + roomNum + Creator.playerSystemSo.levelNumberSaved)
                {
                    int xPosTemp = Random.Range(2, 10);
                    //An x-value of 5 or 6 is directly in front of a door0.
                    //These spaces MUST be free for the player when they walk into a new room
                    while (xPosTemp == 5 || xPosTemp == 6)
                    {
                        xPosTemp = Random.Range(2, 10);
                    }
                    float xPos = xPosTemp + 0.5f;
                    
                    float yPos = Random.Range(3, 11) + 0.5f + roomNum * 11;

                    Vector3 chosenPos = new Vector3(xPos, yPos, 0);

                    //Each enemy should start on its own piece
                    if (!positions.ContainsKey(chosenPos))
                    {
                        List<Piece> selectedPiece = new()
                        {
                            Piece.Pawn,
                            Piece.Rook,
                            Piece.Knight,
                            Piece.Bishop,
                            Piece.Queen,
                            Piece.King
                        };
                        
                        //We do NOT want pawns to be in the last row
                        //So if chosenPos.y = last row y-value, then we find another piece
                        if (yPos == 3.5f + roomNum * 11)
                        {
                            selectedPiece.Remove(Piece.Pawn);
                        }
                        //We don't want queens to appear until room 3
                        if (Creator.playerSystemSo.roomNumberSaved < 3)
                        {
                            selectedPiece.Remove(Piece.Queen);
                        }
                        //We don't want kings to appear until room 6
                        if (Creator.playerSystemSo.roomNumberSaved < 6)
                        {
                            selectedPiece.Remove(Piece.King);
                        }

                        int indexChosen = Random.Range(0, selectedPiece.Count);
                        Piece chosenPiece = selectedPiece[indexChosen];
                        
                        positions.Add(chosenPos, (chosenPiece, roomNum));

                    }
                }

                foreach (KeyValuePair<Vector3,(Piece, int)> kvp in positions)
                {
                    Creator.enemySo.cachedSpawnPoints[kvp.Key] = (kvp.Value.Item1, kvp.Value.Item2);
                }
            }
        }
        
        List<SingleDoorPosition> doors = _doorsSystem.GetDoorPositions().ToList();
        doors.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));

        foreach (KeyValuePair<Vector3, (Piece, int)> cachedSpawn in Creator.enemySo.cachedSpawnPoints)
        {
            if(cachedSpawn.Value.Item2 < Creator.playerSystemSo.roomNumberSaved)
                continue;
            
            ElEnemyController enemyController = new ElEnemyController();
            enemyController.GameStart(elCreator);
            
            enemyController.SetEnemyInstance(cachedSpawn.Key, cachedSpawn.Value.Item1, doors);
            _enemyControllers.Add(enemyController);
            _haveEnemyControllersFinishedMove.Add(enemyController, false);
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
        _haveEnemyControllersFinishedMove.Remove(enemyController);

        if (IsEnemiesInRoomCleared(roomNumber))
        {
            _doorsSystem.SetRoomDoorsOpen(roomNumber);
        }
    }

    private Dictionary<ElEnemyController, bool> _haveEnemyControllersFinishedMove = new();

    public void SetStateForAllEnemies(ElEnemyController.States state)
    {
        if (state == ElEnemyController.States.ChooseTile)
        {
            foreach (ElEnemyController enemyController in _enemyControllers)
            {
                _haveEnemyControllersFinishedMove[enemyController] = false;
            }
        }
        
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

    public void EnemyControllerMoved(ElEnemyController enemyController)
    {
        _haveEnemyControllersFinishedMove[enemyController] = true;
        
        bool allEnemiesMoved = true;
        foreach (KeyValuePair<ElEnemyController, bool> hasFinishedMove in _haveEnemyControllersFinishedMove)
        {
            if(hasFinishedMove.Key.GetRoomNumber() != Creator.playerSystemSo.roomNumberSaved)
                continue;
                
            if (!hasFinishedMove.Value)
            {
                allEnemiesMoved = false;
                break;
            }
        }
        if (allEnemiesMoved)
        {
            _turnSystem.SwitchTurn(ElTurnSystem.Turn.Player);
        }
    }
}
