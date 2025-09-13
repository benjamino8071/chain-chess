using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PieceController : Controller
{
    protected ValidMovesSystem _validMovesSystem;
    protected BoardSystem _boardSystem;
    protected ChainUISystem _chainUISystem;
    protected AudioSystem _audioSystem;
    
    public enum States
    {
        WaitingForTurn,
        FindingMove,
        Moving,
        NotInUse,
        Paused,
        Blocked,
        EndGame
    }

    public Piece currentPiece => _currentPiece;
    
    public PieceAbility pieceAbility => _pieceAbility;
    
    public PieceColour pieceColour => _pieceColour;
    
    public States state => _state;
    
    public Vector3 piecePos => _pieceInstance.position;

    public Vector3 jumpPos => _jumpPosition;
    
    public List<Piece> capturedPieces => _capturedPieces;
    
    public int movesUsed => _capturedPieces.Count - _movesInThisTurn.Count;

    public int movesRemaining => _movesInThisTurn.Count;
    
    public int piecesCapturedInThisTurn => _piecesCapturedInThisTurn;
    
    protected List<Piece> _capturedPieces = new(16);
    protected List<Piece> _movesInThisTurn = new(16);
    
    protected Transform _pieceInstance;
    
    protected SpriteRenderer _spriteRenderer;

    protected GameObject _background;
    
    protected Animator _animator;
    
    protected SideSystem _allySideSystem;
    protected SideSystem _enemySideSystem;
    
    protected Piece _currentPiece;
    protected PieceAbility _pieceAbility;
    protected PieceColour _pieceColour;
    protected PieceColour _enemyColour;
    protected States _state;
    protected Vector3 _startPosition;
    protected Vector3 _jumpPosition;
    protected int _piecesCapturedInThisTurn;
    protected float _timer;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _chainUISystem = creator.GetDependency<ChainUISystem>();
        _audioSystem = creator.GetDependency<AudioSystem>();
    }

    public virtual void Init(Vector3 position, Piece startingPiece, PieceColour pieceColour,
        PieceAbility pieceAbility, SideSystem allySideSystem, SideSystem enemySideSystem)
    {
        _pieceInstance = Creator.InstantiateGameObject(Creator.piecePrefab, position, Quaternion.identity).transform;
        
        _background = Creator.GetChildObjectByName(_pieceInstance.gameObject, AllTagNames.Background).gameObject;
        
        _spriteRenderer = Creator.GetChildComponentByName<SpriteRenderer>(_pieceInstance.gameObject, AllTagNames.PlayerSprite);
        
        _animator = _pieceInstance.GetComponentInChildren<Animator>();
        
        _jumpPosition = _pieceInstance.position;
        
        _pieceAbility = pieceAbility;
        
        _allySideSystem = allySideSystem;

        _enemySideSystem = enemySideSystem;
        
        _pieceColour = pieceColour;
        _enemyColour = pieceColour == PieceColour.White ? PieceColour.Black : PieceColour.White;
        
        _capturedPieces.Add(startingPiece);
        _movesInThisTurn.Add(startingPiece);
        
        switch (pieceAbility)
        {
            case PieceAbility.None:
            {
                _spriteRenderer.material = Creator.piecesSo.noneMat;
                _spriteRenderer.material.color = pieceColour == PieceColour.White ? Creator.piecesSo.whiteColor : Creator.piecesSo.blackColor;
                break;
            }
            case PieceAbility.Resetter:
            {
                _spriteRenderer.material = Creator.piecesSo.resetterMat;
                _spriteRenderer.material.color = Creator.piecesSo.resetterColor;
                break;
            }
            case PieceAbility.MustMove:
            {
                _spriteRenderer.material = Creator.piecesSo.mustMoveMat;
                _spriteRenderer.material.color = Creator.piecesSo.mustMoveColor;
                break;
            }
            case PieceAbility.Multiplier:
            {
                _spriteRenderer.material = Creator.piecesSo.multiplierMat;
                _spriteRenderer.material.color = Creator.piecesSo.multiplierColor;
                break;
            }
            case PieceAbility.CaptureLover:
            {
                _spriteRenderer.material = Creator.piecesSo.captureLoverMat;
                _spriteRenderer.material.color = Creator.piecesSo.captureLoverColor;
                break;
            }
            case PieceAbility.StopTurn:
            {
                _spriteRenderer.material = Creator.piecesSo.stopTurnMat;
                _spriteRenderer.material.color = Creator.piecesSo.stopTurnColor;
                break;
            }
            case PieceAbility.AlwaysMove:
            {
                _spriteRenderer.material = Creator.piecesSo.alwaysMoveMat;
                _spriteRenderer.material.color = Creator.piecesSo.alwaysMoveColor;
                break;
            }
        }
        UpdateSprite(startingPiece);
        
        SetState(pieceColour == PieceColour.White ? States.FindingMove : States.WaitingForTurn);
    }

    public override void GameUpdate(float dt)
    {
        //State machine
        switch (_state)
        {
            case States.WaitingForTurn:
                break;
            case States.FindingMove:
                FindingMove(dt);
                break;
            case States.Moving:
                Moving(dt);
                break;
            case States.NotInUse:
                break;
            case States.Blocked:
                Blocked(dt);
                break;
            case States.EndGame:
                break;
        }
    }

    protected virtual void FindingMove(float dt)
    {
        
    }
    
    protected virtual void Moving(float dt)
    {
        
    }

    protected void Blocked(float dt)
    {
        _timer -= dt;
        if (_timer <= 0)
        {
            _allySideSystem.PieceBlocked(this);
            SetState(States.NotInUse);
        }
    }

    public void AddCapturedPiece(Piece piece)
    {
        _piecesCapturedInThisTurn++;
        
        _capturedPieces.Add(piece);
        _movesInThisTurn.Add(piece);
    }
    
    protected float Evaluate(float x)
    {
        return 0.5f * Mathf.Sin(x - Mathf.PI / 2f) + 0.5f;
    }
    
    protected virtual void SetToNextMove()
    {
        
    }

    protected void Finish()
    {
        SetState(States.WaitingForTurn);
        _validMovesSystem.HideAllValidMoves();
        _allySideSystem.PieceFinished(this);
    }

    protected List<ValidMove> CheckValidDefiniteMoves(List<Vector3> moves)
    {
        List<ValidMove> validMoves = new();
        foreach (Vector3 move in moves)
        {
            Vector3 positionFromPlayer = _pieceInstance.position + move;

            if (!_boardSystem.IsPositionValid(positionFromPlayer) 
                || _boardSystem.IsAllyAtPosition(positionFromPlayer, _pieceColour))
            {
                continue;
            }
            
            validMoves.Add(new()
            {
                position = positionFromPlayer,
                enemyHere = _boardSystem.IsEnemyAtPosition(positionFromPlayer, _enemyColour)
            });
        }

        return validMoves;
    }

    protected List<ValidMove> CheckValidIndefiniteMoves(List<Vector3> moves)
    {
        List<ValidMove> validMoves = new();
        foreach (Vector3 move in moves)
        {
            Vector3 furthestPointOfMoveLine = _pieceInstance.position;
            while (true)
            {
                Vector3 nextSpot = furthestPointOfMoveLine + move;
                if (!_boardSystem.IsPositionValid(nextSpot) 
                    || _boardSystem.IsAllyAtPosition(nextSpot, _pieceColour))
                {
                    break;
                }
                
                bool enemyHere = _boardSystem.IsEnemyAtPosition(nextSpot, _enemyColour);
                
                furthestPointOfMoveLine = nextSpot;
                validMoves.Add(new()
                {
                    position = furthestPointOfMoveLine,
                    enemyHere = enemyHere
                });

                if (enemyHere)
                {
                    break;
                }
            }
        }

        return validMoves;
    }

    public List<ValidMove> AllValidMovesOfFirstPiece()
    {
        return GetAllValidMovesOfPiece(_capturedPieces[0]); 
    }

    public List<ValidMove> GetAllValidMovesOfCurrentPiece()
    {
        return GetAllValidMovesOfPiece(_movesInThisTurn[0]);
    }

    private List<ValidMove> GetAllValidMovesOfPiece(Piece piece)
    {
        List<ValidMove> validMoves = new(64);
        switch (piece)
        {
            case Piece.Pawn:
            {
                int direction = _pieceColour == PieceColour.White ? 1 : -1;
                
                List<ValidMove> pawnMoves = new();

                Vector3 defaultMove = _pieceInstance.position + new Vector3(0, 1, 0) * direction;
                if (!_boardSystem.IsAllyAtPosition(defaultMove, _pieceColour) 
                    && !_boardSystem.IsEnemyAtPosition(defaultMove, _enemyColour)
                    && _boardSystem.IsPositionValid(defaultMove))
                {
                    pawnMoves.Add(new()
                    {
                        position = defaultMove,
                        enemyHere = false
                    });
                }
                
                Vector3 topLeft = _pieceInstance.position + new Vector3(-1, 1, 0) * direction;
                if (_boardSystem.IsEnemyAtPosition(topLeft, _enemyColour) && _boardSystem.IsPositionValid(topLeft))
                {
                    pawnMoves.Add(new()
                    {
                        position = topLeft,
                        enemyHere = true
                    });
                }
                
                Vector3 topRight = _pieceInstance.position + new Vector3(1, 1, 0) * direction;
                if (_boardSystem.IsEnemyAtPosition(topRight, _enemyColour) && _boardSystem.IsPositionValid(topRight))
                {
                    pawnMoves.Add(new()
                    {
                        position = topRight,
                        enemyHere = true
                    });
                }

                validMoves = pawnMoves;
                break;
            }
            case Piece.Rook:
                List<Vector3> rookMoves = new()
                {
                    new Vector3(-1, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0)
                };

                List<ValidMove> rookValidMoves = CheckValidIndefiniteMoves(rookMoves);
                validMoves = rookValidMoves.ToList();
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
                
                List<ValidMove> knightValidMoves = CheckValidDefiniteMoves(knightMoves);
                validMoves = knightValidMoves.ToList();
                break;
            case Piece.Bishop:
                List<Vector3> bishopMoves = new()
                {
                    new Vector3(1, 1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                
                List<ValidMove> bishopValidMoves = CheckValidIndefiniteMoves(bishopMoves);
                validMoves = bishopValidMoves.ToList();
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
                
                List<ValidMove> queenValidMoves = CheckValidIndefiniteMoves(queenMoves);
                validMoves = queenValidMoves.ToList();
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
                
                List<ValidMove> kingValidMoves = CheckValidDefiniteMoves(kingMoves);
                validMoves = kingValidMoves.ToList();
                break;
        }

        return validMoves;
    }

    protected void UpdateSprite(Piece piece)
    {
        switch (piece)
        {
            case Piece.NotChosen:
                _spriteRenderer.sprite = null;
                break;
            case Piece.Pawn:
                _spriteRenderer.sprite = Creator.piecesSo.pawn;
                break;
            case Piece.Rook:
                _spriteRenderer.sprite = Creator.piecesSo.rook;
                break;
            case Piece.Knight:
                _spriteRenderer.sprite = Creator.piecesSo.knight;
                break;
            case Piece.Bishop:
                _spriteRenderer.sprite = Creator.piecesSo.bishop;
                break;
            case Piece.Queen:
                _spriteRenderer.sprite = Creator.piecesSo.queen;
                break;
            case Piece.King:
                _spriteRenderer.sprite = Creator.piecesSo.king;
                break;
        }
        _currentPiece = piece;
    }

    public void SetState(States state)
    {
        switch (_state)
        {
            case States.WaitingForTurn:
            case States.FindingMove:
            case States.Moving:
            case States.Paused:
            {
                switch (state)
                {
                    case States.NotInUse:
                        _pieceInstance.gameObject.SetActive(false);
                        break;
                    case States.FindingMove:
                        _background.SetActive(_pieceColour == PieceColour.White);
                        break;
                    case States.Moving:
                        _background.SetActive(false);
                        break;
                    case States.WaitingForTurn:
                        if (_movesInThisTurn.Count == 0)
                        {
                            UpdateSprite(_capturedPieces[0]);
                            foreach (Piece capturedPiece in _capturedPieces)
                            {
                                _movesInThisTurn.Add(capturedPiece);
                            }
                        }
                        _piecesCapturedInThisTurn = 0; //For next time the player uses this piece
                        _background.SetActive(false);
                        break;
                    case States.Blocked:
                        _timer = 1.1f; //Shrink animation length
                        _animator.SetTrigger("shrink");
                        _background.SetActive(false);
                        break;
                }
                break;
            }
            case States.Blocked:
            {
                switch (state)
                {
                    case States.NotInUse:
                    {
                        _pieceInstance.gameObject.SetActive(false);
                        break;
                    }
                    case States.EndGame:
                    {
                        break;
                    }
                }
                break;
            }
            case States.NotInUse:
            case States.EndGame:
            {
                break;
            }
        }
        
        _state = state;
    }

    public void PlayEnlargeAnimation()
    {
        _animator?.SetTrigger("enlarge");
    }

    public virtual void ForceMove(float3 movePosition)
    {
        _jumpPosition = movePosition;
        SetState(States.Moving);
    }

    private Piece GenerateOtherRandomPiece(Piece piece)
    {
        List<Piece> glitchedPieceChanges = new()
        {
            Piece.Knight,
            Piece.Knight,
            Piece.Rook,
            Piece.Rook,
            Piece.Bishop,
            Piece.Bishop,
            Piece.Queen
        };

        switch (piece)
        {
            case Piece.Knight:
                glitchedPieceChanges.Remove(Piece.Knight);
                glitchedPieceChanges.Remove(Piece.Knight);
                break;
            case Piece.Rook:
                glitchedPieceChanges.Remove(Piece.Rook);
                glitchedPieceChanges.Remove(Piece.Rook);
                break;
            case Piece.Bishop:
                glitchedPieceChanges.Remove(Piece.Bishop);
                glitchedPieceChanges.Remove(Piece.Bishop);
                break;
            case Piece.Queen:
                glitchedPieceChanges.Remove(Piece.Queen);
                break;
        }
                            
        // Generate a random index
        int index = Random.Range(0, glitchedPieceChanges.Count);

        // Use the index to set the piece
        return glitchedPieceChanges[index];
    }
    
    public override void Destroy()
    {
        Object.Destroy(_pieceInstance.gameObject);
    }
}
