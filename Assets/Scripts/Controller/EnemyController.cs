using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : Controller
{
    private GridSystem _gridSystem;
    private PlayerSystem _playerSystem;
    private EnemiesSystem _enemiesSystem;
    private TurnInfoUISystem _turnInfoUISystem;
    private GameOverUISystem _gameOverUISystem;
    private TimerUISystem _timerUISystem;
    private DoorsSystem _doorsSystem;
    
    private Transform _enemyInstance;

    private SpriteRenderer _spriteRenderer;
    
    private Animator _playerAnimator;
    
    private Piece _piece;
    
    public enum States
    {
        WaitingForTurn,
        ChooseTile,
        MoveToTile,
        Captured,
        Won
        
    }
    private States _state;
    
    private Vector3 _positionChosen;

    private int _roomNumber;
    
    private float _moveSpeed;
    private float _sinTime;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.TryGetDependency("GridSystem", out GridSystem gridSystem))
        {
            _gridSystem = gridSystem;
        }
        if (Creator.TryGetDependency("PlayerSystem", out PlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }
        if (Creator.TryGetDependency("EnemiesSystem", out EnemiesSystem enemiesSystem))
        {
            _enemiesSystem = enemiesSystem;
        }
        if (Creator.TryGetDependency("TurnInfoUISystem", out TurnInfoUISystem turnInfoUISystem))
        {
            _turnInfoUISystem = turnInfoUISystem;
        }
        if (Creator.TryGetDependency("GameOverUISystem", out GameOverUISystem gameOverUISystem))
        {
            _gameOverUISystem = gameOverUISystem;
        }
        if (Creator.TryGetDependency("TimerUISystem", out TimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        if (Creator.TryGetDependency("DoorsSystem", out DoorsSystem doorsSystem))
        {
            _doorsSystem = doorsSystem;
        }
        
        SetState(States.WaitingForTurn);
    }
    
    public void SetEnemyInstance(Vector3 position, Piece piece, List<SingleDoorPosition> doorPositionsOrdered)
    {
        _enemyInstance =
            Creator.InstantiateGameObject(Creator.enemyPrefab, position, Quaternion.identity).transform;
        
        _spriteRenderer = _enemyInstance.GetComponentInChildren<SpriteRenderer>(); //Should only be ONE SpriteRenderer under _enemyInstance

        _playerAnimator = _enemyInstance.GetComponentInChildren<Animator>();
        
        for (int i = 0; i < doorPositionsOrdered.Count; i++)
        {
            float currentDoorYpos = doorPositionsOrdered[i].transform.position.y;
            float nextDoorYpos = doorPositionsOrdered[i+1].transform.position.y;

            if (position.y > currentDoorYpos && position.y < nextDoorYpos)
            {
                int currentRoomNumber = doorPositionsOrdered[i].roomNumber;
                int nextRoomNumber = doorPositionsOrdered[i+1].roomNumber;
                if (currentRoomNumber == nextRoomNumber)
                {
                    _roomNumber = currentRoomNumber;
                    break;
                }
            }
        }
        
        SetPiece(piece);
    }

    private void SetPiece(Piece piece)
    {
        switch (piece)
        {
            case Piece.NotChosen:
                _spriteRenderer.sprite = default;
                break;
            case Piece.Pawn:
                _spriteRenderer.sprite = Creator.enemySo.pawn;
                break;
            case Piece.Rook:
                _spriteRenderer.sprite = Creator.enemySo.rook;
                break;
            case Piece.Knight:
                _spriteRenderer.sprite = Creator.enemySo.knight;
                break;
            case Piece.Bishop:
                _spriteRenderer.sprite = Creator.enemySo.bishop;
                break;
            case Piece.Queen:
                _spriteRenderer.sprite = Creator.enemySo.queen;
                break;
            case Piece.King:
                _spriteRenderer.sprite = Creator.enemySo.king;
                break;
        }
        
        _piece = piece;
    }

    public Piece GetPiece()
    {
        return _piece;
    }

    private List<Vector3> CheckPossibleDefinitePositions(List<Vector3> moves)
    {
        List<Vector3> possibleMoves = new();
        foreach (Vector3 knightMove in moves)
        {
            Vector3 positionFromEnemy = _enemyInstance.position + knightMove;
                            
            if(_enemiesSystem.IsEnemyAtThisPosition(positionFromEnemy)) continue;
                            
            if(_gridSystem.IsDoorPosition(positionFromEnemy)) continue;
                            
            if(!_gridSystem.IsPositionValid(positionFromEnemy)) continue;

            if (_enemiesSystem.IsPositionTakenByOtherEnemyForThisTurn(positionFromEnemy))
            {
                continue;
            }
            
            possibleMoves.Add(positionFromEnemy);
        }

        return possibleMoves;
    }

    private List<Vector3> CheckPossibleIndefinitePositions(List<Vector3> moves)
    {
        List<Vector3> possibleMoves = new ();
        
        foreach (Vector3 move in moves)
        {
            Vector3 furthestPointOfDiagonal = _enemyInstance.position;

            while (true)
            {
                Vector3 nextSpot = furthestPointOfDiagonal + move;
                
                if(_enemiesSystem.IsEnemyAtThisPosition(nextSpot) && nextSpot != _enemyInstance.position) break;
                        
                if(_gridSystem.IsDoorPosition(nextSpot)) break;
                            
                if(!_gridSystem.IsPositionValid(nextSpot)) break;
                                
                furthestPointOfDiagonal = nextSpot;
                
                if (_playerSystem.GetPlayerPosition() == furthestPointOfDiagonal)
                    break;
            }

            if (furthestPointOfDiagonal != _enemyInstance.position && !_enemiesSystem.IsPositionTakenByOtherEnemyForThisTurn(furthestPointOfDiagonal))
            {
                possibleMoves.Add(furthestPointOfDiagonal);
            }
        }

        return possibleMoves;
    }

    public override void GameUpdate(float dt)
    {
        if (_playerSystem.GetRoomNumber() != GetRoomNumber())
        {
            //In a different room - so don't move this enemy!
            SetState(States.WaitingForTurn);
            return;
        }
        
        //State machine
        List<Vector3> possiblePositions = new();
        switch (_state)
        {
            case States.WaitingForTurn:
                break;
            case States.ChooseTile:
                switch (_piece)
                {
                    case Piece.Pawn:
                        List<Vector3> pawnMoves = new();
                        
                        Vector3 defaultMove = new Vector3(0, -1, 0);
                        if (_enemyInstance.position + defaultMove != _playerSystem.GetPlayerPosition())
                        {
                            pawnMoves.Add(defaultMove);
                        }
                        
                        Vector3 bottomLeft = new Vector3(-1, -1, 0);
                        if (_enemyInstance.position + bottomLeft == _playerSystem.GetPlayerPosition())
                        {
                            pawnMoves.Add(bottomLeft);
                        }
                        
                        Vector3 bottomRight = new Vector3(1, -1, 0);
                        if (_enemyInstance.position + bottomRight == _playerSystem.GetPlayerPosition())
                        {
                            pawnMoves.Add(bottomRight);
                        }
                        
                        List<Vector3> pawnPossPosi = CheckPossibleDefinitePositions(pawnMoves);
                        possiblePositions = possiblePositions.Concat(pawnPossPosi).ToList();
                        break;
                    case Piece.Rook:
                        List<Vector3> rookMoves = new()
                        {
                            new Vector3(-1, 0, 0),
                            new Vector3(1, 0, 0),
                            new Vector3(0, 1, 0),
                            new Vector3(0, -1, 0)
                        };

                        List<Vector3> rookPossPosi = CheckPossibleIndefinitePositions(rookMoves);
                        possiblePositions = possiblePositions.Concat(rookPossPosi).ToList();
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
                        
                        List<Vector3> knightPossPosi = CheckPossibleDefinitePositions(knightMoves);
                        possiblePositions = possiblePositions.Concat(knightPossPosi).ToList();
                        break;
                    case Piece.Bishop:
                        List<Vector3> bishopMoves = new()
                        {
                            new Vector3(1, 1, 0),
                            new Vector3(-1, 1, 0),
                            new Vector3(1, -1, 0),
                            new Vector3(-1, -1, 0)
                        };
                        
                        List<Vector3> bishopPossPosi = CheckPossibleIndefinitePositions(bishopMoves);
                        possiblePositions = possiblePositions.Concat(bishopPossPosi).ToList();
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
                        
                        List<Vector3> queenPossPosi = CheckPossibleIndefinitePositions(queenMoves);
                        possiblePositions = possiblePositions.Concat(queenPossPosi).ToList();
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
                        
                        List<Vector3> kingPossPosi = CheckPossibleDefinitePositions(kingMoves);
                        possiblePositions = possiblePositions.Concat(kingPossPosi).ToList();
                        break;
                }
                
                if (possiblePositions.Count > 0)
                {
                    //Go through each position and see if the player is at that position. If they are, capture it!
                    bool foundPlayer = false;
                    foreach (Vector3 possiblePosition in possiblePositions)
                    {
                        if (_playerSystem.GetPlayerPosition() == possiblePosition)
                        {
                            _positionChosen = possiblePosition;
                            foundPlayer = true;
                            break;
                        }
                    }
                    if (!foundPlayer)
                    {
                        int chosenPositionIndex = Random.Range(0, possiblePositions.Count);
                        _enemiesSystem.AddPositionTakenByEnemyForThisTurn(possiblePositions[chosenPositionIndex]);
                        _positionChosen = possiblePositions[chosenPositionIndex];
                    }
                    
                    _moveSpeed = Creator.playerSystemSo.moveSpeed;
                    SetState(States.MoveToTile);
                    TriggerJumpAnimation();
                }
                else
                {
                    SetState(States.WaitingForTurn);
                    _turnInfoUISystem.SwitchTurn(TurnInfoUISystem.Turn.Player);
                }
                break;
            case States.MoveToTile:
                if (_enemyInstance.position != _positionChosen)
                {
                    _sinTime += dt * _moveSpeed;
                    _sinTime = Mathf.Clamp(_sinTime, 0f, Mathf.PI);
                    float t = Evaluate(_sinTime);
                    _enemyInstance.position = Vector3.Lerp(_enemyInstance.position, _positionChosen, t);
                }
                
                if (_enemyInstance.position == _positionChosen)
                {
                    _enemyInstance.position = new Vector3(((int)_enemyInstance.position.x) + 0.5f, ((int)_enemyInstance.position.y) + 0.5f, 0);
                    _sinTime = 0;
                    
                    if (_enemyInstance.position == _playerSystem.GetPlayerPosition())
                    {
                        _timerUISystem.StopTimer();
                        _playerSystem.SetState(PlayerSystem.States.Captured);
                        _enemiesSystem.SetStateForAllEnemies(States.Won);
                    }
                    else
                    {
                        SetState(States.WaitingForTurn);
                        _turnInfoUISystem.SwitchTurn(TurnInfoUISystem.Turn.Player);
                    }
                }
                break;
            case States.Captured:
                break;
            case States.Won:
                break;
        }
    }
    
    private void TriggerJumpAnimation()
    {
        _playerAnimator.SetTrigger("Jump");
    }
    
    private float Evaluate(float x)
    {
        return 0.5f * Mathf.Sin(x - Mathf.PI / 2f) + 0.5f;
    }

    public void SetState(States newState)
    {
        _state = newState;
    }

    public States GetState()
    {
        return _state;
    }

    public Vector3 GetPosition()
    {
        return _enemyInstance.position;
    }

    public void PieceCaptured()
    {
        SetState(States.Captured);
        
        _enemyInstance.gameObject.SetActive(false);
    }

    public Sprite GetSprite()
    {
        return _spriteRenderer.sprite;
    }

    public int GetRoomNumber()
    {
        //11.5f is the halfway point, along the y-axis, between each room
        return _roomNumber;
    }
}
