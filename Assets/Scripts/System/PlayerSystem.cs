using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSystem : Dependency
{
    private GridSystem _gridSystem;
    private EnemiesSystem _enemiesSystem;
    private CinemachineSystem _cinemachineSystem;
    private TurnInfoUISystem _turnInfoUISystem;
    private TimerUISystem _timerUISystem;
    private CapturedPiecesUISystem _capturedPiecesUISystem;
    private EndGameUISystem _endGameUISystem;
    private AudioSystem _audioSystem;
    private GameOverUISystem _gameOverUISystem;

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

    public enum States
    {
        WaitingForDefaultPiece,
        WaitingForTurn,
        Idle,
        Moving,
        FadeOutBeforeDoorWalk,
        DoorWalk,
        FadeInAfterDoorWalk,
        Captured,
        EndGame
    }

    private States _state;
    
    private Vector3 _doorPositionOnOut;
    private int _roomNumberOnOut;

    private float _timeAtFadeIn;
    private float _fadeInAnimTimer;

    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        if (_creator.TryGetDependency("GridSystem", out GridSystem gridSystem))
        {
            _gridSystem = gridSystem;
        }
        if (_creator.TryGetDependency("EnemiesSystem", out EnemiesSystem enemiesSystem))
        {
            _enemiesSystem = enemiesSystem;
        }
        if (_creator.TryGetDependency("CinemachineSystem", out CinemachineSystem cinemachineSystem))
        {
            _cinemachineSystem = cinemachineSystem;
        }
        if (_creator.TryGetDependency("TurnInfoUISystem", out TurnInfoUISystem turnInfoUISystem))
        {
            _turnInfoUISystem = turnInfoUISystem;
        }
        if (_creator.TryGetDependency("TimerUISystem", out TimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        if (_creator.TryGetDependency("CapturedPiecesUISystem", out CapturedPiecesUISystem capturedPiecesUISystem))
        {
            _capturedPiecesUISystem = capturedPiecesUISystem;
        }
        if (_creator.TryGetDependency("EndGameUISystem", out EndGameUISystem endGameUISystem))
        {
            _endGameUISystem = endGameUISystem;
        }
        if (_creator.TryGetDependency("AudioSystem", out AudioSystem audioSystem))
        {
            _audioSystem = audioSystem;
        }
        if (_creator.TryGetDependency("GameOverUISystem", out GameOverUISystem gameOverUISystem))
        {
            _gameOverUISystem = gameOverUISystem;
        }
        
        Transform playerSpawnPosition = GameObject.FindWithTag("PlayerSpawnPosition").transform;
        Vector3 spawnPos = playerSpawnPosition.position + new Vector3(0, _creator.playerSystemSo.roomNumberSaved * 11f, 0);
        
        _playerCharacter = _creator.InstantiateGameObject(_creator.playerPrefab, spawnPos, Quaternion.identity).transform;

        _playerAnimator = _playerCharacter.GetComponentInChildren<Animator>();
        
        _jumpPosition = _playerCharacter.position;

        foreach (Animator playerAnimator in _playerCharacter.GetComponentsInChildren<Animator>())
        {
            if (playerAnimator.tag == "PlayerVisual")
            {
                _playerAnimator = playerAnimator;
                _playerSprite = playerAnimator.GetComponent<SpriteRenderer>();
            }
        }
        
        for (int i = 0; i < _validPositionsVisuals.Capacity; i++)
        {
            GameObject validPos =
                _creator.InstantiateGameObject(_creator.validPositionPrefab, Vector3.zero, Quaternion.identity);
            _validPositionsVisuals.Add(validPos.transform);
        }
        
        SetState(States.WaitingForDefaultPiece);
    }

    public void SetDefaultPiece(Piece piece)
    {
        _creator.playerSystemSo.startingPiece = piece;
        _capturedPieces.AddFirst(_creator.playerSystemSo.startingPiece);
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
            case States.WaitingForDefaultPiece:
                break;
            case States.WaitingForTurn:
                break;
            case States.Idle:
                if(_movesInThisTurn.Count == 0) return;
                
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
                                _endGameUISystem.Show();
                            }
                            else
                            {
                                _doorPositionOnOut = singleDoorPosition.GetPlayerPositionOnOut();
                                _roomNumberOnOut = singleDoorPosition.GetOtherDoorRoomNumber();
                                _moveSpeed = _creator.playerSystemSo.walkThroughDoorSpeed;
                                TriggerFadeOutAnimation();
                                SetState(States.FadeOutBeforeDoorWalk);
                                _timerUISystem.StopTimer();
                            }
                            break;
                        }
                    }

                    if (_movesInThisTurn.Count > 0)
                    {
                        //Allow player to make another move
                        UpdateSprite(_movesInThisTurn.Peek());
                        SetState(States.Idle);
                        _capturedPiecesUISystem.HighlightNextPiece();
                    }
                    else
                    {
                        SetState(States.WaitingForTurn);
                        _turnInfoUISystem.SwitchTurn(TurnInfoUISystem.Turn.Enemy);
                    }
                }
                break;
            case States.FadeOutBeforeDoorWalk:
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    _cinemachineSystem.SwitchState(_roomNumberOnOut);
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
                    //Experimenting with having the play lose the chain after going into a new room
                    _movesInThisTurn.Clear();
                    _capturedPieces.Clear();
                    _capturedPieces.AddFirst(_creator.playerSystemSo.startingPiece);
                    _capturedPiecesUISystem.InNewRoomReset();
                    _capturedPiecesUISystem.ShowNewPiece(_creator.playerSystemSo.startingPiece, true);
                    _creator.playerSystemSo.roomNumberSaved = GetRoomNumber();
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
                    _gameOverUISystem.ShowGameOver();
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
            
            if(!_gridSystem.IsPositionValid(newPos)) continue;
            
            if (_gridSystem.TryGetSingleDoorPosition(newPos, out SingleDoorPosition singleDoorPosition))
            {
                //If this door is locked, we cannot allow access
                if(!singleDoorPosition.isDoorOpen) continue;
            }
                        
            if (piece == Piece.Pawn && _enemiesSystem.IsEnemyAtThisPosition(newPos) && pieceMove == new Vector3(0, 1, 0))
            {
                //Cannot take a piece that is directly in front of pawn
                continue;
            }
                        
            if (_enemiesSystem.TryGetEnemyAtPosition(newPos, out EnemyController enemyController))
            {
                //Add this player to the 'captured pieces' list
                Piece enemyPiece = enemyController.GetPiece();
                _capturedPieces.AddLast(enemyPiece);
                _movesInThisTurn.Enqueue(enemyPiece);
                _enemiesSystem.PieceCaptured(enemyController, GetRoomNumber());
            }
                    
            // Set our position as a fraction of the distance between the markers.
            _jumpPosition = positionRequested;
            _moveSpeed = _creator.playerSystemSo.moveSpeed;
            SetState(States.Moving);
                    
            TriggerJumpAnimation();
            _audioSystem.PlayerPieceMoveSFX();
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
                
                //If we have found an invalid position, we have explored this path the furthest
                if(!_gridSystem.IsPositionValid(nextSpot)) break;
                
                //If, while exploring this path we find an enemy and the position of that enemy is NOT the position requested,
                //then it is impossible for the player to choose a position past this enemy.
                //then we have explored this path the furthest
                if (_enemiesSystem.TryGetEnemyAtPosition(nextSpot, out EnemyController eneCont))
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
                
                if (_enemiesSystem.TryGetEnemyAtPosition(nextSpot, out EnemyController enemyController))
                {
                    //Add this player to the 'captured pieces' list
                    Piece enemyPiece = enemyController.GetPiece();
                    _capturedPieces.AddLast(enemyPiece);
                    _movesInThisTurn.Enqueue(enemyPiece);
                    _enemiesSystem.PieceCaptured(enemyController, GetRoomNumber());
                }
                    
                // Set our position as a fraction of the distance between the markers.
                _jumpPosition = positionRequested;
                _moveSpeed = _creator.playerSystemSo.moveSpeed;
                SetState(States.Moving);
                
                TriggerJumpAnimation();
                _audioSystem.PlayerPieceMoveSFX();
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
                    
            if (_gridSystem.TryGetSingleDoorPosition(positionFromPlayer, out SingleDoorPosition knightSingDoorPos))
            {
                //If this door is locked, we cannot allow access
                if(!knightSingDoorPos.isDoorOpen) continue;
            }
                            
            if(!_gridSystem.IsPositionValid(positionFromPlayer)) continue;
                            
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
                    if(!knightSingDoorPos.isDoorOpen) break;
                }
                            
                if(!_gridSystem.IsPositionValid(nextSpot)) break;
                                
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
                _playerSprite.sprite = _creator.playerSystemSo.pawn;
                break;
            case Piece.Rook:
                _playerSprite.sprite = _creator.playerSystemSo.rook;
                break;
            case Piece.Knight:
                _playerSprite.sprite = _creator.playerSystemSo.knight;
                break;
            case Piece.Bishop:
                _playerSprite.sprite = _creator.playerSystemSo.bishop;
                break;
            case Piece.Queen:
                _playerSprite.sprite = _creator.playerSystemSo.queen;
                break;
            case Piece.King:
                _playerSprite.sprite = _creator.playerSystemSo.king;
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
                TriggerFadeOutAnimation();
                break;
            case States.Idle:
                if (_movesInThisTurn.Count == 0)
                {
                    UpdateSprite(_creator.playerSystemSo.startingPiece);
                    //Reset possible moves
                    foreach (Piece capturedPiece in _capturedPieces)
                    {
                        _movesInThisTurn.Enqueue(capturedPiece);
                    }
                    _capturedPiecesUISystem.Reset();
                }
                break;
            case States.WaitingForTurn:
                UpdateSprite(_creator.playerSystemSo.startingPiece);
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
        //11.5f is the halway point, along the y-axis, between each room
        return (int)(_playerCharacter.position.y / 11f);
    }
}
