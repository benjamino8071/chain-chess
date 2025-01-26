using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevPlayerController : LevDependency
{
    private LevGridSystem _gridSystem;
    private LevEnemiesSystem _enemiesSystem;
    private LevCinemachineSystem _cinemachineSystem;
    private LevChainUISystem _chainUISystem;
    private LevLevelCompleteUISystem _levelCompleteUISystem;
    private LevAudioSystem _audioSystem;
    private LevDoorsSystem _doorsSystem;
    private LevTurnSystem _turnSystem;
    private LevGameOverUISystem _gameOverUISystem;
    private LevPlayerSystem _playerSystem;
    
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
        LevelComplete,
        EndGame
    }

    private States _state;
    
    private Vector3 _doorPositionOnOut;
    private int _roomNumberOnOut;

    private float _timeAtFadeIn;
    private float _fadeInAnimTimer;
    private float _timeAtFadeOut;
    private float _fadeOutAnimTimer;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _gridSystem = levCreator.GetDependency<LevGridSystem>();
        _enemiesSystem = levCreator.GetDependency<LevEnemiesSystem>();
        _cinemachineSystem = levCreator.GetDependency<LevCinemachineSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _levelCompleteUISystem = levCreator.GetDependency<LevLevelCompleteUISystem>();
        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        _doorsSystem = levCreator.GetDependency<LevDoorsSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        _gameOverUISystem = levCreator.GetDependency<LevGameOverUISystem>();
        _playerSystem = levCreator.GetDependency<LevPlayerSystem>();
    }

    public void SetPiece(Transform spawnPoint, Piece piece)
    {
        _playerCharacter = Creator.InstantiateGameObject(Creator.playerPrefab, spawnPoint.position, Quaternion.identity).transform;
        
        _playerCharacter.GetComponentInChildren<Canvas>(true).gameObject.SetActive(false);
        
        _playerAnimator = _playerCharacter.GetComponentInChildren<Animator>();
        
        _jumpPosition = _playerCharacter.position;
        
        _playerAnimator = _playerCharacter.GetComponentInChildren<Animator>();
        _playerSprite = _playerAnimator.GetComponent<SpriteRenderer>();
        
        for (int i = 0; i < _validPositionsVisuals.Capacity; i++)
        {
            GameObject validPos =
                Creator.InstantiateGameObject(Creator.validPositionPrefab, Vector3.zero, Quaternion.identity);
            _validPositionsVisuals.Add(validPos.transform);
        }
        
        SetDefaultPiece(piece);
        SetState(States.Idle);
    }

    public override void GameUpdate(float dt)
    {
        switch (_state)
        {
            case States.WaitingForTurn:
                break;
            case States.Idle:
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
                    if (_doorsSystem.TryGetSingleDoorPosition(_playerCharacter.position, out SingleDoorPosition singleDoorPosition))
                    {
                        if (singleDoorPosition.isDoorOpen)
                        {
                            if (singleDoorPosition.isFinalDoor)
                            {
                                //Player has won!!
                                TriggerFadeOutAnimation();
                                _fadeOutAnimTimer = dt;
                                _timeAtFadeOut = dt;
                                SetState(States.LevelComplete);
                            }
                            else
                            {
                                _doorPositionOnOut = singleDoorPosition.GetPlayerPositionOnOut();
                                _roomNumberOnOut = singleDoorPosition.GetOtherDoorRoomNumber();
                                _roomNumber = _roomNumberOnOut;
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
                        _turnSystem.SwitchTurn(LevTurnSystem.Turn.Enemy);
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
                    _capturedPieces.AddFirst(_capturedPieces.First.Value);
                    _chainUISystem.NewRoomClearChain();
                    _chainUISystem.ShowNewPiece(_capturedPieces.First.Value, true);
                    SetState(States.Idle);
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
                    _gameOverUISystem.Show();
                }
                break;
            case States.LevelComplete:
                //Wait 1 second before we show the level complete screen
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    SetState(States.EndGame);
                    _levelCompleteUISystem.Show();
                }
                break;
            case States.EndGame:
                break;
        }
    }
    
    private void SetDefaultPiece(Piece piece)
    {
        _capturedPieces.AddFirst(piece);
    }

    private float Evaluate(float x)
    {
        return 0.5f * Mathf.Sin(x - Mathf.PI / 2f) + 0.5f;
    }

    private void CheckDefiniteMoves(Piece piece, List<Vector3> pieceMoves, Vector3 positionRequested)
    {
        foreach (Vector3 pieceMove in pieceMoves)
        {
            Vector3 newPos = _playerCharacter.position + pieceMove;
            
            if(positionRequested != newPos) 
                continue;
            
            if (_doorsSystem.TryGetSingleDoorPosition(newPos, out SingleDoorPosition singleDoorPosition))
            {
                //If this door is locked, we cannot allow access
                if(!singleDoorPosition.isDoorOpen) 
                    continue;
            }
            else if(!_gridSystem.IsPositionValid(newPos))
                continue;
            
            if(_playerSystem.IsPlayerAtPosition(positionRequested))
                continue;
            
            if (piece == Piece.Pawn && _enemiesSystem.IsEnemyAtThisPosition(newPos) && pieceMove == new Vector3(0, 1, 0))
            {
                //Cannot take a piece that is directly in front of pawn
                continue;
            }
                        
            if (_enemiesSystem.TryGetEnemyAtPosition(newPos, out LevEnemyController enemyController))
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
            SetState(States.Moving);
                    
            TriggerJumpAnimation();
            _audioSystem.PlayerPieceMoveSfx();
            _movesInThisTurn.Dequeue();
            
            return;
        }
        
        //If we get here then a valid position has not been found
        _playerSystem.UnselectPiece();
    }

    private void CheckIndefiniteMoves(List<Vector3> pieceMoves, Vector3 positionRequested)
    {
        foreach (Vector3 pieceMove in pieceMoves)
        {
            Vector3 newPos = _playerCharacter.position;

            while (true)
            {
                Vector3 nextSpot = newPos + pieceMove;
                
                if (_doorsSystem.TryGetSingleDoorPosition(nextSpot, out SingleDoorPosition singleDoorPosition))
                {
                    if(!singleDoorPosition.isDoorOpen) break;
                }
                else if(!_gridSystem.IsPositionValid(nextSpot))
                    break;
                
                if (_enemiesSystem.TryGetEnemyAtPosition(nextSpot, out LevEnemyController eneCont))
                {
                    if(eneCont.GetPosition() != positionRequested) break;
                }
                
                if(_playerSystem.IsPlayerAtPosition(positionRequested))
                    break;
                
                if (positionRequested != nextSpot)
                {
                    newPos = nextSpot;
                    continue;
                }
                
                if (_enemiesSystem.TryGetEnemyAtPosition(nextSpot, out LevEnemyController enemyController))
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
                SetState(States.Moving);
                
                TriggerJumpAnimation();
                _audioSystem.PlayerPieceMoveSfx();
                _movesInThisTurn.Dequeue();
                
                return;
            }
        }
        
        //If we get here then a valid position has not been found
        _playerSystem.UnselectPiece();
    }

    public void UpdatePlayerPosition(Vector3 positionRequested)
    {
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
                    
            if (_doorsSystem.TryGetSingleDoorPosition(positionFromPlayer, out SingleDoorPosition singleDoorPos))
            {
                //If this door is locked, we cannot allow access
                if(!singleDoorPos.isDoorOpen) continue;
            }
            else if(!_gridSystem.IsPositionValid(positionFromPlayer))
                continue;
            else if(_playerSystem.IsPlayerAtPosition(positionFromPlayer))
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
                if (_doorsSystem.TryGetSingleDoorPosition(nextSpot, out SingleDoorPosition knightSingDoorPos))
                {
                    //If this door is locked, we cannot allow access
                    if(!knightSingDoorPos.isDoorOpen) 
                        break;
                }
                else if(!_gridSystem.IsPositionValid(nextSpot))
                    break;
                else if(_playerSystem.IsPlayerAtPosition(nextSpot))
                    break;
                
                furthestPointOfDiagonal = nextSpot;
                validMoves.Add(furthestPointOfDiagonal);
                        
                if(_enemiesSystem.IsEnemyAtThisPosition(nextSpot)) break;
            }
        }

        return validMoves;
    }

    public void UpdateValidMoves()
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

    public void HideAllValidMoves()
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
                TriggerFadeOutAnimation();
                break;
            case States.Idle:
                if (_movesInThisTurn.Count == 0)
                {
                    UpdateSprite(_capturedPieces.First.Value);
                    //Reset possible moves
                    foreach (Piece capturedPiece in _capturedPieces)
                    {
                        _movesInThisTurn.Enqueue(capturedPiece);
                    }
                    _chainUISystem.ResetPosition();
                }
                break;
            case States.WaitingForTurn:
                UpdateSprite(_capturedPieces.First.Value);
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
