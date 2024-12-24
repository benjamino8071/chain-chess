using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class ElEnemiesSystem : ElDependency
{
    private ElChainUISystem _capturedPiecesUISystem;
    private ElDoorsSystem _doorsSystem;
    private ElTurnSystem _turnSystem;
    private ElTimerUISystem _timerUISystem;
    private ElAudioSystem _audioSystem;
    private ElPlayerSystem _playerSystem;
    
    private List<ElEnemyController> _enemyControllers = new ();

    private List<Vector3> _positionsTakenByOtherEnemies = new();

    private List<Transform> _validPositionsVisuals = new(64);
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        if (Creator.TryGetDependency(out ElChainUISystem capturedPiecesUISystem))
        {
            _capturedPiecesUISystem = capturedPiecesUISystem;
        }
        if (Creator.TryGetDependency(out ElDoorsSystem doorsSystem))
        {
            _doorsSystem = doorsSystem;
        }
        if (Creator.TryGetDependency(out ElTurnSystem turnSystem))
        {
            _turnSystem = turnSystem;
        }
        if (Creator.TryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        if (Creator.TryGetDependency(out ElAudioSystem audioSystem))
        {
            _audioSystem = audioSystem;
        }
        if (Creator.TryGetDependency(out ElPlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }
        
        for (int i = 0; i < _validPositionsVisuals.Capacity; i++)
        {
            GameObject validPos =
                Creator.InstantiateGameObject(Creator.enemyValidPositionsPrefab, Vector3.zero, Quaternion.identity);
            _validPositionsVisuals.Add(validPos.transform);
        }

        if (Creator.enemySo.cachedSpawnPoints.Count == 0)
        {
            int numOfEnemies = -1;
            for (int roomNum = 0; roomNum < 9; roomNum++)
            {
                if (roomNum == Creator.shopSo.shopRoomNumber)
                    continue;
                numOfEnemies++;
                Dictionary<Vector3, (Piece, int, ElEnemyController.PieceEffectType)> positions = new();
                while (positions.Count < 3 + numOfEnemies + Creator.playerSystemSo.levelNumberSaved)
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

                        if (roomNum < 5)
                        {
                            selectedPiece.Remove(Piece.Queen);
                        }
                        else
                        {
                            int numOfQueens = 0;
                            foreach ((Piece piece, int roomNumber, ElEnemyController.PieceEffectType pieceEffectType) positionsValue in positions.Values)
                            {
                                if (positionsValue.roomNumber == roomNum && positionsValue.piece == Piece.Queen)
                                {
                                    numOfQueens++;
                                }
                            }

                            if (numOfQueens >= 2)
                            {
                                selectedPiece.Remove(Piece.Queen);
                            }
                        }

                        if (roomNum < 7)
                        {
                            selectedPiece.Remove(Piece.King);
                        }
                        else
                        {
                            bool kingInRoom = false;
                            foreach ((Piece piece, int roomNumber, ElEnemyController.PieceEffectType pieceEffectType) positionsValue in positions.Values)
                            {
                                if (positionsValue.roomNumber == roomNum && positionsValue.piece == Piece.King)
                                {
                                    kingInRoom = true;
                                    break;
                                }
                            }
                            if (!kingInRoom)
                            {
                                //MUST be a king in these rooms. If there isn't one then we force the king to appear
                                selectedPiece = new()
                                {
                                    Piece.King
                                };
                            }
                            else
                            {
                                selectedPiece.Remove(Piece.King);
                            }
                        }

                        int indexChosen = Random.Range(0, selectedPiece.Count);
                        Piece chosenPiece = selectedPiece[indexChosen];
                        
                        List<ElEnemyController.PieceEffectType> pieceEffectTypes = new()
                        {
                            ElEnemyController.PieceEffectType.None,
                        };

                        if (Creator.playerSystemSo.levelNumberSaved >= 1)
                        {
                            pieceEffectTypes.Add(ElEnemyController.PieceEffectType.Glitched);
                        }
                        if (Creator.playerSystemSo.levelNumberSaved >= 2)
                        {
                            pieceEffectTypes.Add(ElEnemyController.PieceEffectType.Capture);
                        }
                        if (Creator.playerSystemSo.levelNumberSaved >= 3)
                        {
                            pieceEffectTypes.Add(ElEnemyController.PieceEffectType.Chain);
                        }

                        // Generate a random index
                        int index = Creator.randomGenerator.Next(0, pieceEffectTypes.Count);
                        
                        positions.Add(chosenPos, (chosenPiece, roomNum, pieceEffectTypes[index]));

                    }
                }

                foreach (KeyValuePair<Vector3,(Piece, int, ElEnemyController.PieceEffectType)> kvp in positions)
                {
                    Creator.enemySo.cachedSpawnPoints[kvp.Key] = (kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3);
                }
            }
        }
        
        List<SingleDoorPosition> doors = _doorsSystem.GetDoorPositions().ToList();
        doors.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));

        foreach (KeyValuePair<Vector3, (Piece, int, ElEnemyController.PieceEffectType)> cachedSpawn in Creator.enemySo.cachedSpawnPoints)
        {
            if(cachedSpawn.Value.Item2 < Creator.playerSystemSo.roomNumberSaved)
                continue;
            
            ElEnemyController enemyController = new ElEnemyController();
            enemyController.GameStart(elCreator);
            
            enemyController.SetEnemyInstance(cachedSpawn.Key, cachedSpawn.Value.Item1, cachedSpawn.Value.Item3, doors);
            _enemyControllers.Add(enemyController);
        }

    }

    public override void GameUpdate(float dt)
    {
        foreach (ElEnemyController enemyController in _enemyControllers)
        {
            enemyController.GameUpdate(dt);
        }

        foreach (Transform validPositionsVisual in _validPositionsVisuals)
        {
            validPositionsVisual.gameObject.SetActive(false);
        }
        if (Creator.playerSystemSo.artefacts.Contains(ArtefactTypes.EnemyLineOfSight))
        {
            UpdateValidEnemyPositions(Creator.playerSystemSo.lineOfSightsChosen);
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
            _timerUISystem.StopTimer();
            _doorsSystem.SetRoomDoorsOpen(roomNumber);
        }
    }

    private Queue<ElEnemyController> _moveInRoomQueue = new();
    
    public void SetStateForAllEnemies(ElEnemyController.States state)
    {
        if (state == ElEnemyController.States.ChooseTile)
        {
            foreach (ElEnemyController enemyController in _enemyControllers)
            {
                if (enemyController.GetRoomNumber() == _playerSystem.GetRoomNumber())
                {
                    _moveInRoomQueue.Enqueue(enemyController);
                }
            }

            if (_moveInRoomQueue.TryDequeue(out ElEnemyController firstEnemy))
            {
                firstEnemy.SetState(ElEnemyController.States.ChooseTile);
            }
            return;
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
        if (_moveInRoomQueue.TryDequeue(out ElEnemyController nextEnemy))
        {
            nextEnemy.SetState(ElEnemyController.States.ChooseTile);
        }
        else
        {
            _turnSystem.SwitchTurn(ElTurnSystem.Turn.Player);
            _timerUISystem.StartTimer();
        }
    }
    
    public bool CheckIfEnemiesCanCapture()
    {
        List<ElEnemyController> captureTypePieces = new();
        
        foreach (ElEnemyController enemyController in _enemyControllers)
        {
            if (enemyController.GetPieceEffectType() == ElEnemyController.PieceEffectType.Capture)
            {
                captureTypePieces.Add(enemyController);
            }
        }

        bool foundPlayer = false;
        foreach (ElEnemyController enemyController in captureTypePieces)
        {
            if (enemyController.AllowMovementIfEffectTypeIsCapture())
                foundPlayer = true;
        }

        return foundPlayer;
    }

    private void UpdateValidEnemyPositions(List<Piece> piece)
    {
        List<Vector3> validMoves = new();
        
        foreach (ElEnemyController enemyController in _enemyControllers)
        {
            if(enemyController.GetRoomNumber() != Creator.playerSystemSo.roomNumberSaved || !piece.Contains(enemyController.GetPiece()))
                continue;

            List<Vector3> pieceTypeMoves = new();
            bool isDefiniteMoves = false;
            switch (enemyController.GetPiece())
            {
                case Piece.Pawn:
                    pieceTypeMoves = new()
                    {
                        new Vector3(-1, -1, 0),
                        new Vector3(1, -1, 0)
                    };
                    
                    isDefiniteMoves = true;
                    break;
                case Piece.Knight:
                    pieceTypeMoves = new()
                    {
                        new Vector3(1, 2, 0),
                        new Vector3(-1, 2, 0),
                        new Vector3(1, -2, 0),
                        new Vector3(-1, -2, 0),
                        new Vector3(-2, 1, 0),
                        new Vector3(-2, -1, 0),
                        new Vector3(2, 1, 0),
                        new Vector3(2, -1, 0)
                    };
                    isDefiniteMoves = true;
                    break;
                case Piece.Bishop:
                    pieceTypeMoves = new()
                    {
                        new Vector3(1, 1, 0),
                        new Vector3(-1, 1, 0),
                        new Vector3(1, -1, 0),
                        new Vector3(-1, -1, 0)
                    };
                    isDefiniteMoves = false;
                    break;
                case Piece.Rook:
                    pieceTypeMoves = new()
                    {
                        new Vector3(-1, 0, 0),
                        new Vector3(1, 0, 0),
                        new Vector3(0, 1, 0),
                        new Vector3(0, -1, 0)
                    };
                    isDefiniteMoves = false;
                    break;
                case Piece.Queen:
                    pieceTypeMoves = new()
                    {
                        new Vector3(-1, 0, 0),
                        new Vector3(1, 0, 0),
                        new Vector3(0, 1, 0),
                        new Vector3(0, -1, 0),
                        new Vector3(1, 1, 0),
                        new Vector3(-1, 1, 0),
                        new Vector3(1, -1, 0),
                        new Vector3(-1, -1, 0)
                    };
                    isDefiniteMoves = false;
                    break;
                case Piece.King:
                    pieceTypeMoves = new()
                    {
                        new Vector3(-1, 0, 0),
                        new Vector3(1, 0, 0),
                        new Vector3(0, 1, 0),
                        new Vector3(0, -1, 0),
                        new Vector3(1, 1, 0),
                        new Vector3(-1, 1, 0),
                        new Vector3(1, -1, 0),
                        new Vector3(-1, -1, 0)
                    };
                    isDefiniteMoves = true;
                    break;
            }

            if (isDefiniteMoves)
            {
                List<Vector3> possibleMoves = enemyController.CheckPossibleDefinitePositions(pieceTypeMoves);
                validMoves = validMoves.Concat(possibleMoves).ToList();
            }
            else
            {
                List<Vector3> possibleMoves = enemyController.CheckPossibleIndefinitePositions(pieceTypeMoves);
                validMoves = validMoves.Concat(possibleMoves).ToList();
            }
        }
        
        for (int i = 0; i < validMoves.Count; i++)
        {
            _validPositionsVisuals[i].position = validMoves[i];
            _validPositionsVisuals[i].gameObject.SetActive(true);
        }
    }
}
