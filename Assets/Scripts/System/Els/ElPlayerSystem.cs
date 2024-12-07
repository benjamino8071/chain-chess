using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Here the Player is allowed to move until
/// A: they are captured by an Enemy piece
/// </summary>
public class ElPlayerSystem : ElDependency
{
    private ElGridSystem _gridSystem;
    private ElEnemiesSystem _enemiesSystem;
    private ElCinemachineSystem _cinemachineSystem;
    private ElTimerUISystem _timerUISystem;
    private ElChainUISystem _chainUISystem;
    private ElAudioSystem _audioSystem;
    private ElGameOverUISystem _gameOverUISystem;
    private ElTurnSystem _turnInfoUISystem;
    private ElRoomNumberUISystem _roomNumberUISystem;

    //Note: The first node is King
    private LinkedList<Piece> _capturedPieces = new();
    private Queue<Piece> _movesInThisTurn = new();
    
    private Transform _playerCharacter;
    
    private List<Transform> _validPositionsVisuals = new(64);
    
    private Vector3 _jumpPosition;

    private Animator _playerAnimator;
    private SpriteRenderer _playerSprite;
    
    private float _moveSpeed;
    private float _sinTime;

    private int _roomNumber;

    public enum States
    {
        WaitingForTurn,
        Idle,
        Moving,
        FadeOutBeforeDoorWalk,
        DoorWalk,
        FadeInAfterDoorWalk,
        Captured,
        TimerExpired,
        EndGame
    }

    private States _state;
    
    private Vector3 _doorPositionOnOut;
    private int _roomNumberOnOut;

    private float _timeAtFadeIn;
    private float _fadeInAnimTimer;

    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.NewTryGetDependency(out ElGridSystem gridSystem))
        {
            _gridSystem = gridSystem;
        }
        if (Creator.NewTryGetDependency(out ElEnemiesSystem enemiesSystem))
        {
            _enemiesSystem = enemiesSystem;
        }
        if (Creator.NewTryGetDependency(out ElCinemachineSystem cinemachineSystem))
        {
            _cinemachineSystem = cinemachineSystem;
        }
        if (Creator.NewTryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        if (Creator.NewTryGetDependency(out ElChainUISystem chainUISystem))
        {
            _chainUISystem = chainUISystem;
        }
        if (Creator.NewTryGetDependency(out ElAudioSystem audioSystem))
        {
            _audioSystem = audioSystem;
        }
        if (Creator.NewTryGetDependency(out ElGameOverUISystem gameOverUISystem))
        {
            _gameOverUISystem = gameOverUISystem;
        }
        if (Creator.NewTryGetDependency(out ElTurnSystem turnSystem))
        {
            _turnInfoUISystem = turnSystem;
        }
        if (Creator.NewTryGetDependency(out ElRoomNumberUISystem roomNumberUISystem))
        {
            _roomNumberUISystem = roomNumberUISystem;
        }

        _roomNumber = Creator.playerSystemSo.roomNumberSaved;
        
        Transform playerSpawnPosition = GameObject.FindWithTag("PlayerSpawnPosition").transform;
        Vector3 spawnPos = playerSpawnPosition.position + new Vector3(0, Creator.playerSystemSo.roomNumberSaved * 11f, 0);
        
        _playerCharacter = Creator.InstantiateGameObject(Creator.playerPrefab, spawnPos, Quaternion.identity).transform;

        _playerAnimator = _playerCharacter.GetComponentInChildren<Animator>();
        
        _jumpPosition = _playerCharacter.position;

        foreach (Animator playerAnimator in _playerCharacter.GetComponentsInChildren<Animator>())
        {
            if (playerAnimator.CompareTag("PlayerVisual"))
            {
                _playerAnimator = playerAnimator;
                _playerSprite = playerAnimator.GetComponent<SpriteRenderer>();
            }
        }
        
        for (int i = 0; i < _validPositionsVisuals.Capacity; i++)
        {
            GameObject validPos =
                Creator.InstantiateGameObject(Creator.validPositionPrefab, Vector3.zero, Quaternion.identity);
            _validPositionsVisuals.Add(validPos.transform);
        }
        
        SetDefaultPiece(Creator.startingPiece);
        SetState(States.Idle);
    }

    public void SetDefaultPiece(Piece piece)
    {
        Creator.startingPiece = piece;
        _capturedPieces.AddFirst(Creator.startingPiece);
    }

    private float Evaluate(float x)
    {
        return 0.5f * Mathf.Sin(x - Mathf.PI / 2f) + 0.5f;
    }
    
    public override void GameUpdate(float dt)
    {
        HideAllValidMoves();
        
        UpdateValidMoves();
        
        switch (_state)
        {
            case States.WaitingForTurn:
                break;
            case States.Idle:
                if(_movesInThisTurn.Count == 0) return;
                
                if (Creator.inputSo._leftMouseButton.action.WasPerformedThisFrame())
                {
                    UpdatePlayerPosition();
                }
                
                /*
                Piece piece = _movesInThisTurn.Peek();
                bool isPawn = piece == Piece.Pawn;
                Vector3 posInFrontOfPlayer = _playerCharacter.position + new Vector3(0, 1, 0);

                bool isInFrontOfClosedDoor = false;
                if (_gridSystem.TryGetSingleDoorPosition(posInFrontOfPlayer, out SingleDoorPosition checkDoorOpen))
                {
                    isInFrontOfClosedDoor = !checkDoorOpen.isDoorOpen;
                }
                
                bool stuckAsPawn = isPawn &&
                                   (_enemiesSystem.IsEnemyAtThisPosition(posInFrontOfPlayer) || !_gridSystem.IsPositionValid(posInFrontOfPlayer) || isInFrontOfClosedDoor);
                if (!stuckAsPawn)
                {
                    if (_creator.inputSo._leftMouseButton.action.WasPerformedThisFrame())
                    {
                        UpdatePlayerPosition();
                    }
                }
                else
                {
                    _jumpPosition = _playerCharacter.position;
                    _moveSpeed = _creator.playerSystemSo.moveSpeed;
                    SetState(States.Moving);
                    
                    TriggerJumpAnimation();
                    
                    _movesInThisTurn.Dequeue();
                }
                */
                break;
            case States.Moving:
                if (_playerCharacter.position != _jumpPosition)
                {
                    _sinTime += dt * _moveSpeed;
                    _sinTime = Mathf.Clamp(_sinTime, 0f, Mathf.PI);
                    float t = Evaluate(_sinTime);
                    _playerCharacter.position = Vector3.Lerp(_playerCharacter.position, _jumpPosition, t);
                }
                
                if (_playerCharacter.position == _jumpPosition)
                {
                    _playerCharacter.position = new Vector3(((int)_playerCharacter.position.x) + 0.5f, ((int)_playerCharacter.position.y) + 0.5f, 0);
                    _sinTime = 0;

                    //Check if the player's position is a door position
                    if (_gridSystem.TryGetSingleDoorPosition(_playerCharacter.position, out SingleDoorPosition singleDoorPosition))
                    {
                        if (singleDoorPosition.isDoorOpen)
                        {
                            if (singleDoorPosition.isFinalDoor)
                            {
                                //Player has won!!
                                _timerUISystem.StopTimer();
                                TriggerFadeOutAnimation();
                                SetState(States.EndGame);
                                //TODO: Figure out what happens when the player clears a level _gameOverUISystem.Show();
                            }
                            else
                            {
                                _timerUISystem.StopTimer();
                                _doorPositionOnOut = singleDoorPosition.GetPlayerPositionOnOut();
                                _roomNumberOnOut = singleDoorPosition.GetOtherDoorRoomNumber();
                                _roomNumber = _roomNumberOnOut;
                                Creator.playerSystemSo.roomNumberSaved = _roomNumberOnOut;
                                _moveSpeed = Creator.playerSystemSo.walkThroughDoorSpeed;
                                TriggerFadeOutAnimation();
                                SetState(States.FadeOutBeforeDoorWalk);
                            }
                            break;
                        }
                    }

                    if (_movesInThisTurn.Count > 0)
                    {
                        //Allow player to make another move
                        UpdateSprite(_movesInThisTurn.Peek());
                        SetState(States.Idle);
                        _chainUISystem.HighlightNextPiece();
                    }
                    else
                    {
                        SetState(States.WaitingForTurn);
                        _turnInfoUISystem.SwitchTurn(ElTurnSystem.Turn.Enemy);
                    }
                }
                break;
            case States.FadeOutBeforeDoorWalk:
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    _cinemachineSystem.SwitchState(_roomNumberOnOut);
                    _roomNumberUISystem.UpdateRoomNumberText();
                    SetState(States.DoorWalk);
                }
                break;
            case States.DoorWalk:
                if (_playerCharacter.position != _doorPositionOnOut)
                {
                    _sinTime += dt * _moveSpeed;
                    _sinTime = Mathf.Clamp(_sinTime, 0f, Mathf.PI);
                    float t = Evaluate(_sinTime);
                    _playerCharacter.position = Vector3.Lerp(_playerCharacter.position, _doorPositionOnOut, t);
                }
                
                if (_playerCharacter.position == _doorPositionOnOut)
                {
                    _playerCharacter.position = new Vector3(((int)_playerCharacter.position.x) + 0.5f, ((int)_playerCharacter.position.y) + 0.5f, 0);
                    _sinTime = 0;

                    TriggerFadeInAnimation();
                    _fadeInAnimTimer = dt;
                    _timeAtFadeIn = dt;
                    SetState(States.FadeInAfterDoorWalk);
                }
                break;
            case States.FadeInAfterDoorWalk:
                //Fade in animation is 1 second
                if (_fadeInAnimTimer >= _timeAtFadeIn + 1)
                {
                    //Chain resets when player goes into new room
                    _movesInThisTurn.Clear();
                    _capturedPieces.Clear();
                    _capturedPieces.AddFirst(Creator.startingPiece);
                    _chainUISystem.InNewRoomReset();
                    _chainUISystem.ShowNewPiece(Creator.startingPiece, true);
                    //Creator.playerSystemSo.roomNumberSaved = GetRoomNumber();
                    SetState(States.Idle);
                    _timerUISystem.StartTimer();
                }
                else
                {
                    _fadeInAnimTimer += dt;
                }
                break;
            case States.Captured:
                //Wait 1 second before we show the game over screen
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    SetState(States.EndGame);
                    _gameOverUISystem.Show("Captured");
                }
                break;
            case States.TimerExpired:
                //Wait 1 second before we show the game over screen
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    SetState(States.EndGame);
                    _gameOverUISystem.Show("Timer Expired");
                }
                break;
            case States.EndGame:
                break;
        }
    }

    private void CheckDefiniteMoves(Piece piece, List<Vector3> pieceMoves, Vector3 positionRequested)
    {
        foreach (Vector3 pieceMove in pieceMoves)
        {
            Vector3 newPos = _playerCharacter.position + pieceMove;
            
            if(positionRequested != newPos) continue;
            Debug.Log("Found position requested");
            
            if (_gridSystem.TryGetSingleDoorPosition(newPos, out SingleDoorPosition singleDoorPosition))
            {
                //If this door is locked, we cannot allow access
                Debug.Log("Door requested. is this door open? "+singleDoorPosition.isDoorOpen);
                if(!singleDoorPosition.isDoorOpen) continue;
                Debug.Log("Player wants to go through door ");
            }
            else if(!_gridSystem.IsPositionValid(newPos))
                continue;
            
            if (piece == Piece.Pawn && _enemiesSystem.IsEnemyAtThisPosition(newPos) && pieceMove == new Vector3(0, 1, 0))
            {
                //Cannot take a piece that is directly in front of pawn
                continue;
            }
                        
            if (_enemiesSystem.TryGetEnemyAtPosition(newPos, out ElEnemyController enemyController))
            {
                //Add this player to the 'captured pieces' list
                Piece enemyPiece = enemyController.GetPiece();
                _capturedPieces.AddLast(enemyPiece);
                _movesInThisTurn.Enqueue(enemyPiece);
                _enemiesSystem.PieceCaptured(enemyController, GetRoomNumber());
            }
            
            // Set our position as a fraction of the distance between the markers.
            _jumpPosition = positionRequested;
            _moveSpeed = Creator.playerSystemSo.moveSpeed;
            _timerUISystem.StartTimer();
            SetState(States.Moving);
            
            TriggerJumpAnimation();
            _audioSystem.PlayerPieceMoveSfx();
            _movesInThisTurn.Dequeue();
        }
    }

    private void CheckIndefiniteMoves(List<Vector3> pieceMoves, Vector3 positionRequested)
    {
        //Here we check all possible positions of the player's current piece
        //If we find a possible position that is equal to the position requested, we move the player to this piece and break out of the loop
        //When using a pieceMove, if we run into an invalid position or a closed door, we have explored that direction as far as possible 
        //So we get the next pieceMove or the loop finishes
        
        foreach (Vector3 pieceMove in pieceMoves)
        {
            Vector3 newPos = _playerCharacter.position;
            bool foundSpot = false;

            while (true)
            {
                Vector3 nextSpot = newPos + pieceMove;
                
                //If we have found a closed door, we have explored this path the furthest
                if (_gridSystem.TryGetSingleDoorPosition(nextSpot, out SingleDoorPosition singleDoorPosition))
                {
                    //If this door is locked, we cannot allow access
                    //canMoveToNextSpot = canMoveToNextSpot && singleDoorPosition.isDoorOpen;
                    if(!singleDoorPosition.isDoorOpen) break;
                }
                else if(!_gridSystem.IsPositionValid(nextSpot))
                    break;
                //If we have found an invalid position, we have explored this path the furthest
                
                //If, while exploring this path we find an enemy and the position of that enemy is NOT the position requested,
                //then it is impossible for the player to choose a position past this enemy.
                //then we have explored this path the furthest
                if (_enemiesSystem.TryGetEnemyAtPosition(nextSpot, out ElEnemyController eneCont))
                {
                    if(eneCont.GetPosition() != positionRequested) break;
                }
                
                
                //If the position requested isn't nextSpot, then we haven't found the position.
                //So we must continue the search
                if (positionRequested != nextSpot)
                {
                    newPos = nextSpot;
                    continue;
                }
                
                if (_enemiesSystem.TryGetEnemyAtPosition(nextSpot, out ElEnemyController enemyController))
                {
                    //Add this player to the 'captured pieces' list
                    Piece enemyPiece = enemyController.GetPiece();
                    _capturedPieces.AddLast(enemyPiece);
                    _movesInThisTurn.Enqueue(enemyPiece);
                    _enemiesSystem.PieceCaptured(enemyController, GetRoomNumber());
                }
                    
                // Set our position as a fraction of the distance between the markers.
                _jumpPosition = positionRequested;
                _moveSpeed = Creator.playerSystemSo.moveSpeed;
                _timerUISystem.StartTimer();
                SetState(States.Moving);
                
                TriggerJumpAnimation();
                _audioSystem.PlayerPieceMoveSfx();
                _movesInThisTurn.Dequeue();

                foundSpot = true;
                break;
            }
            if(foundSpot)
                break;
        }
    }
    
    private void UpdatePlayerPosition()
    {
        Vector3 positionRequested = _gridSystem.GetHighlightPosition();
        
        //Depending on the piece, we allow the player an additional move (player will always start with a king move)
        Piece piece = _movesInThisTurn.Peek();
        
        switch (piece)
        {
            case Piece.Pawn:
                //Allow player to move up one space, if that space is available
                List<Vector3> pawnMoves = new();
                
                Vector3 defaultMove = new Vector3(0, 1, 0);
                if (!_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + defaultMove))
                {
                    pawnMoves.Add(defaultMove);
                }
                
                //Check if there is an enemy to the top left or top right. If so, these moves are allowed
                Vector3 topLeft = new Vector3(-1, 1, 0);
                if (_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + topLeft))
                {
                    pawnMoves.Add(topLeft);
                }
                
                Vector3 topRight = new Vector3(1, 1, 0);
                if (_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + topRight))
                {
                    pawnMoves.Add(topRight);
                }
                
                CheckDefiniteMoves(piece, pawnMoves, positionRequested);
                break;
            case Piece.Rook:
                List<Vector3> rookMoves = new()
                {
                    new Vector3(-1, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0)
                };
                
                CheckIndefiniteMoves(rookMoves, positionRequested);
                break;
            case Piece.Knight:
                List<Vector3> knightMoves = new()
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
                
                CheckDefiniteMoves(piece, knightMoves, positionRequested);
                break;
            case Piece.Bishop:
                List<Vector3> bishopMoves = new()
                {
                    new Vector3(1, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                
                CheckIndefiniteMoves(bishopMoves, positionRequested);
                break;
            case Piece.Queen:
                List<Vector3> queenMoves = new()
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
                
                CheckIndefiniteMoves(queenMoves, positionRequested);
                break;
            case Piece.King:
                List<Vector3> kingMoves = new()
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
                
                CheckDefiniteMoves(piece, kingMoves, positionRequested);
                break;
        }
    }
    
    private List<Vector3> CheckValidDefiniteMoves(List<Vector3> moves)
    {
        List<Vector3> validMoves = new();
        foreach (Vector3 move in moves)
        {
            Vector3 positionFromPlayer = _playerCharacter.position + move;
                    
            if (_gridSystem.TryGetSingleDoorPosition(positionFromPlayer, out SingleDoorPosition singleDoorPos))
            {
                //If this door is locked, we cannot allow access
                if(!singleDoorPos.isDoorOpen) continue;
            }
            else if(!_gridSystem.IsPositionValid(positionFromPlayer))
                continue;
            
            validMoves.Add(positionFromPlayer);
        }

        return validMoves;
    }

    private List<Vector3> CheckValidIndefiniteMoves(List<Vector3> moves)
    {
        List<Vector3> validMoves = new();
        foreach (Vector3 diagonal in moves)
        {
            Vector3 furthestPointOfDiagonal = _playerCharacter.position;
            while (true)
            {
                Vector3 nextSpot = furthestPointOfDiagonal + diagonal;
                if (_gridSystem.TryGetSingleDoorPosition(nextSpot, out SingleDoorPosition knightSingDoorPos))
                {
                    //If this door is locked, we cannot allow access
                    if(!knightSingDoorPos.isDoorOpen) 
                        break;
                }
                else if(!_gridSystem.IsPositionValid(nextSpot))
                    break;
                
                furthestPointOfDiagonal = nextSpot;
                validMoves.Add(furthestPointOfDiagonal);
                        
                if(_enemiesSystem.IsEnemyAtThisPosition(nextSpot)) break;
            }
        }

        return validMoves;
    }

    private void UpdateValidMoves()
    {
        if(_state != States.Idle || _movesInThisTurn.Count == 0) return;
        
        Piece piece = _movesInThisTurn.Peek();
        List<Vector3> validMoves = new();
        switch (piece)
        {
            case Piece.Pawn:
                List<Vector3> pawnMoves = new();

                Vector3 defaultMove = new Vector3(0, 1, 0);
                if (!_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + defaultMove))
                {
                    pawnMoves.Add(defaultMove);
                }
                Vector3 topLeft = new Vector3(-1, 1, 0);
                if (_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + topLeft))
                {
                    pawnMoves.Add(topLeft);
                }
                Vector3 topRight = new Vector3(1, 1, 0);
                if (_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + topRight))
                {
                    pawnMoves.Add(topRight);
                }

                List<Vector3> pawnValidMoves = CheckValidDefiniteMoves(pawnMoves);
                validMoves = validMoves.Concat(pawnValidMoves).ToList();
                break;
            case Piece.Rook:
                List<Vector3> rookMoves = new()
                {
                    new Vector3(-1, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0)
                };

                List<Vector3> rookValidMoves = CheckValidIndefiniteMoves(rookMoves);
                validMoves = validMoves.Concat(rookValidMoves).ToList();
                break;
            case Piece.Knight:
                List<Vector3> knightMoves = new()
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
                
                List<Vector3> knightValidMoves = CheckValidDefiniteMoves(knightMoves);
                validMoves = validMoves.Concat(knightValidMoves).ToList();
                break;
            case Piece.Bishop:
                List<Vector3> bishopMoves = new()
                {
                    new Vector3(1, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                
                List<Vector3> bishopValidMoves = CheckValidIndefiniteMoves(bishopMoves);
                validMoves = validMoves.Concat(bishopValidMoves).ToList();
                break;
            case Piece.Queen:
                List<Vector3> queenMoves = new()
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
                
                List<Vector3> queenValidMoves = CheckValidIndefiniteMoves(queenMoves);
                validMoves = validMoves.Concat(queenValidMoves).ToList();
                break;
            case Piece.King:
                List<Vector3> kingMoves = new()
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
                
                List<Vector3> kingValidMoves = CheckValidDefiniteMoves(kingMoves);
                validMoves = validMoves.Concat(kingValidMoves).ToList();
                break;
        }

        for (int i = 0; i < validMoves.Count; i++)
        {
            _validPositionsVisuals[i].position = validMoves[i];
            _validPositionsVisuals[i].gameObject.SetActive(true);
        }
    }
    
    private void HideAllValidMoves()
    {
        foreach (Transform validPositionsVisual in _validPositionsVisuals)
        {
            validPositionsVisual.gameObject.SetActive(false);
        }
    }

    private void UpdateSprite(Piece piece)
    {
        switch (piece)
        {
            case Piece.NotChosen:
                _playerSprite.sprite = default;
                break;
            case Piece.Pawn:
                _playerSprite.sprite = Creator.playerSystemSo.pawn;
                break;
            case Piece.Rook:
                _playerSprite.sprite = Creator.playerSystemSo.rook;
                break;
            case Piece.Knight:
                _playerSprite.sprite = Creator.playerSystemSo.knight;
                break;
            case Piece.Bishop:
                _playerSprite.sprite = Creator.playerSystemSo.bishop;
                break;
            case Piece.Queen:
                _playerSprite.sprite = Creator.playerSystemSo.queen;
                break;
            case Piece.King:
                _playerSprite.sprite = Creator.playerSystemSo.king;
                break;
        }
    }
    
    private void TriggerJumpAnimation()
    {
        _playerAnimator.SetTrigger("Jump");
    }
    
    private void TriggerFadeOutAnimation()
    {
        _playerAnimator.SetTrigger("FadeOut");
    }

    private void TriggerFadeInAnimation()
    {
       _playerAnimator.SetTrigger("FadeIn");
    }
    
    public void SetState(States state)
    {
        switch (state)
        {
            case States.Captured:
            case States.TimerExpired:
                TriggerFadeOutAnimation();
                break;
            case States.Idle:
                if (_movesInThisTurn.Count == 0)
                {
                    UpdateSprite(Creator.startingPiece);
                    //Reset possible moves
                    foreach (Piece capturedPiece in _capturedPieces)
                    {
                        _movesInThisTurn.Enqueue(capturedPiece);
                    }
                    _chainUISystem.Reset();
                }
                break;
            case States.WaitingForTurn:
                UpdateSprite(Creator.startingPiece);
                break;
        }
        _state = state;
    }

    public States GetState()
    {
        return _state;
    }
    
    public Vector3 GetPlayerPosition()
    {
        return _playerCharacter.position;
    }

    public int GetRoomNumber()
    {
        return _roomNumber;
    }
}
