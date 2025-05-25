using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LevPlayerController : LevDependency
{
    public List<Piece> capturedPieces => _capturedPieces.ToList();
    public bool hasMoved => _capturedPieces.Count != _movesInThisTurn.Count;
    
    private LevGridSystem _gridSystem;
    private LevEnemiesSystem _enemiesSystem;
    private LevCinemachineSystem _cinemachineSystem;
    private LevChainUISystem _chainUISystem;
    private LevLevelCompleteUISystem _levelCompleteUISystem;
    private LevAudioSystem _audioSystem;
    private LevTurnSystem _turnSystem;
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
    
    public enum States
    {
        WaitingForTurn,
        Idle,
        Moving,
        OnTheSpotJump,
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
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
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
        
        AddPiece(piece, true);
        SetState(States.Idle);
    }

    public override void GameUpdate(float dt)
    {
        switch (_state)
        {
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

                    if (_enemiesSystem.TryGetEnemyAtPosition(_playerCharacter.position, out LevEnemyController enemyController))
                    {
                        //Add this player to the 'captured pieces' list
                        Piece enemyPiece = enemyController.GetPiece();
                        AddPiece(enemyPiece, false);
                        _movesInThisTurn.Enqueue(enemyPiece);
                        _enemiesSystem.PieceCaptured(enemyController);
                    }

                    SetToNextMove();
                }
                break;
            case States.OnTheSpotJump:
                //Wait 1 second for the jump animation to play out
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty"))
                {
                    SetToNextMove();
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
            case States.Captured:
                //Wait 1 second before we show the game over screen
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    SetState(States.EndGame);
                }
                break;
            case States.LevelComplete:
                //Wait 1 second before we show the level complete screen
                if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hidden"))
                {
                    SetState(States.EndGame);
                }
                break;
            case States.EndGame:
                break;
        }
    }
    
    private void AddPiece(Piece piece, bool addFirst)
    {
        if (addFirst)
            _capturedPieces.AddFirst(piece);
        else
            _capturedPieces.AddLast(piece);
    }

    private float Evaluate(float x)
    {
        return 0.5f * Mathf.Sin(x - Mathf.PI / 2f) + 0.5f;
    }
    
    private void SetToNextMove()
    {
        if (_movesInThisTurn.Count > 0)
        {
            //Allow player to make another move
            Piece nextPiece = _movesInThisTurn.Peek();
            UpdateSprite(nextPiece);
            
            List<Vector3> validMoves = GetAllValidMoves(_movesInThisTurn.Peek());
            
            if (validMoves.Count > 0)
            {
                SetState(States.Idle);
                _chainUISystem.HighlightNextPiece();
            }
            else
            {
                TriggerJumpAnimation();
                _audioSystem.PlayerPieceMoveSfx();
                _movesInThisTurn.Dequeue();
                if(_movesInThisTurn.Count > 0)
                    UpdateSprite(_movesInThisTurn.Peek());
                SetState(States.OnTheSpotJump);
            }
        }
        else
        {
            SetState(States.WaitingForTurn);
            _turnSystem.SwitchTurn(LevTurnSystem.Turn.Enemy);
        }
    }

    public bool TryMovePlayer(Vector3 positionRequested)
    {
        //Depending on the piece, we allow the player an additional move (player will always start with a king move)
        Piece piece = _movesInThisTurn.Peek();
        
        List<Vector3> validMoves = GetAllValidMoves(piece);

        if (validMoves.Contains(positionRequested))
        {
            // Set our position as a fraction of the distance between the markers.
            _jumpPosition = positionRequested;
            _moveSpeed = Creator.playerSystemSo.moveSpeed;
            SetState(States.Moving);
                
            TriggerJumpAnimation();
            _audioSystem.PlayerPieceMoveSfx();
            _movesInThisTurn.Dequeue();

            return true;
        }

        return false;
    }

    private List<Vector3> CheckValidDefiniteMoves(List<Vector3> moves)
    {
        List<Vector3> validMoves = new();
        foreach (Vector3 move in moves)
        {
            Vector3 positionFromPlayer = _playerCharacter.position + move;


            if (!_gridSystem.IsPositionValid(positionFromPlayer) 
                || _playerSystem.IsPlayerAtPosition(positionFromPlayer))
            {
                continue;
            }
            
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
                if (!_gridSystem.IsPositionValid(nextSpot) 
                    || _playerSystem.IsPlayerAtPosition(nextSpot))
                {
                    break;
                }
                
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
        
        List<Vector3> validMoves = GetAllValidMoves(_movesInThisTurn.Peek());
        
        for (int i = 0; i < validMoves.Count; i++)
        {
            _validPositionsVisuals[i].position = validMoves[i];
            _validPositionsVisuals[i].gameObject.SetActive(true);
        }
    }

    private List<Vector3> GetAllValidMoves(Piece piece)
    {
        List<Vector3> validMoves = new();
        switch (piece)
        {
            case Piece.Pawn:
                List<Vector3> pawnMoves = new();

                Vector3 defaultMove = new Vector3(0, 1, 0);
                if (!_enemiesSystem.IsEnemyAtThisPosition(_playerCharacter.position + defaultMove) && !_playerSystem.IsPlayerAtPosition(_playerCharacter.position + defaultMove))
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

        return validMoves;
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
        if (_state == States.EndGame)
        {
            return;
        }
        
        switch (state)
        {
            case States.Captured:
                _playerCharacter.gameObject.SetActive(false);
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
}
