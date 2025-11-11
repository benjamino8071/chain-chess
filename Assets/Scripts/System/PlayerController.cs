using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : Dependency
{
    private ValidMovesSystem _validMovesSystem;
    private BoardSystem _boardSystem;
    private UISystem _uiSystem;
    private AudioSystem _audioSystem;
    private BlackSystem _blackSystem;
    private WhiteSystem _whiteSystem;
    private TurnSystem _turnSystem;
    private PoolSystem _poolSystem;

    public Piece currentPiece => _currentPiece;
    
    public PieceState state => _state;
    
    public Vector3 piecePos => _model.position;

    public Vector3 jumpPos => _jumpPosition;
    
    public List<Piece> capturedPieces => _capturedPieces;
    
    public int movesUsed => _capturedPieces.Count - _movesInThisTurn.Count;

    public int movesRemaining => _movesInThisTurn.Count;
    
    public int piecesCapturedInThisTurn => _piecesCapturedInThisTurn;
    
    private List<Piece> _capturedPieces = new(16);
    private List<Piece> _movesInThisTurn = new(16);
    
    private Transform _model;
    
    private SpriteRenderer _spriteRenderer;

    private GameObject _background;

    private ScaleTween _scaleTween;

    private PieceState _state;
    private Piece _currentPiece;
    private Vector3 _startPosition;
    private Vector3 _jumpPosition;
    private int _piecesCapturedInThisTurn;
    private float _timer;
    
    private float _pitchAmount = 1;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _uiSystem = creator.GetDependency<UISystem>();
        _audioSystem = creator.GetDependency<AudioSystem>();
        _blackSystem = creator.GetDependency<BlackSystem>();
        _whiteSystem = creator.GetDependency<WhiteSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _poolSystem = creator.GetDependency<PoolSystem>();
    }
    
    public void Init(Vector3 position, Piece startingPiece)
    {
        _model = _poolSystem.GetPieceObjectFromPool(position).transform;
        
        _background = Creator.GetChildObjectByName(_model.gameObject, AllTagNames.Background).gameObject;
        
        _spriteRenderer = Creator.GetChildComponentByName<SpriteRenderer>(_model.gameObject, AllTagNames.PlayerSprite);

        _spriteRenderer.color = Creator.piecesSo.whiteColor;
        _spriteRenderer.material = Creator.piecesSo.noneMat;

        _scaleTween = _model.GetComponentInChildren<ScaleTween>();
        _scaleTween.Enlarge();
        
        //Player does not have an ability, so the text canvas can be disabled
        GameObject textCanvas = Creator.GetChildComponentByName<Transform>(_model.gameObject, AllTagNames.Text).parent.parent.gameObject;
        textCanvas.SetActive(false);

        UpdateSprite(startingPiece);
        
        _jumpPosition = _model.position;
        
        _capturedPieces.Add(startingPiece);
        _movesInThisTurn.Add(startingPiece);
        
        _state = PieceState.NotInUse;
    }

    public override void GameUpdate(float dt)
    {
        //State machine
        switch (_state)
        {
            case PieceState.WaitingForTurn:
            {
                break;
            }
            case PieceState.FindingMove:
            {
                FindingMove(dt);
                break;
            }
            case PieceState.DragNDrop:
            {
                DragNDrop(dt);
                break;
            }
            case PieceState.Moving:
            {
                Moving(dt);
                break;
            }
            case PieceState.NotInUse:
            {
                break;
            }
            case PieceState.Blocked:
            {
                Blocked(dt);
                break;
            }
            case PieceState.EndGame:
            {
                break;
            }
        }
    }

    private void FindingMove(float dt)
    {
        if (!Creator.inputSo.leftMouseButton.action.WasPressedThisFrame())
        {
            return;
        }
        if(_blackSystem.IsPieceMoving())
        {
           return; 
        }
        
        //The player will find the move when they are ready
        Vector3 positionRequested = _boardSystem.GetGridPointNearMouse();
        if (CanMove(positionRequested))
        {
            MovePiece(positionRequested);
        }
        else if (positionRequested == _model.position)
        {
            SetState(PieceState.DragNDrop);
        }
    }

    private void DragNDrop(float dt)
    {
        if (Creator.inputSo.leftMouseButton.action.WasReleasedThisFrame())
        {
            Vector3 positionRequested = _boardSystem.GetGridPointNearMouse();
            if (CanMove(positionRequested))
            {
                _model.position = positionRequested;
                MovePiece(_model.position);
            }
            else
            {
                SetState(PieceState.FindingMove);
            }
            _spriteRenderer.transform.localPosition = Vector3.zero;
        }
        else
        {
            _spriteRenderer.transform.position = math.lerp(_spriteRenderer.transform.position, _boardSystem.GetRawMousePosition(), Creator.piecesSo.dragSpeed * dt);
        }
    }
    
    private void Moving(float dt)
    {
        if (_model.position == _jumpPosition)
        {
            Creator.statsMoves++;
            List<UICurrentLevel> currentScores = _uiSystem.GetUI<UICurrentLevel>();
            foreach (UICurrentLevel currentScore in currentScores)
            {
                currentScore.SetCurrentScoreText(Creator.statsTurns, Creator.statsMoves);
            }
            
            _model.position = math.round(_model.position);
            _timer = 0;

            bool won = false;
            AIController enemyPieceController = _blackSystem.GetPieceAtPosition(_model.position);
            if (enemyPieceController is not null)
            {
                _boardSystem.FlickerEdge();
                
                switch (enemyPieceController.pieceAbility)
                {
                    case PieceAbility.Resetter:
                    {
                        _capturedPieces.Clear();
                        _movesInThisTurn.Clear();
                        _piecesCapturedInThisTurn = 0;
                    
                        AddCapturedPiece(enemyPieceController.piece);

                        List<UIChain> uiChains = _uiSystem.GetUI<UIChain>();
                        foreach (UIChain uiChain in uiChains)
                        {
                            uiChain.ShowChain(this);
                        }
                        break;   
                    }
                    case PieceAbility.Multiplier:
                    {
                        List<float3> spawnPositions = new()
                        {
                            enemyPieceController.piecePos + new Vector3(-1, 1, 0),
                            enemyPieceController.piecePos + new Vector3(1, 1, 0),
                            enemyPieceController.piecePos + new Vector3(-1, -1, 0),
                            enemyPieceController.piecePos + new Vector3(1, -1, 0)
                        };
                        foreach (float3 spawnPosition in spawnPositions)
                        {
                            if (_boardSystem.IsPositionValid(spawnPosition) && !_blackSystem.IsPieceAtPosition(spawnPosition))
                            {
                                _blackSystem.CreatePiece(new(spawnPosition.x, spawnPosition.y), enemyPieceController.piece, PieceAbility.None);
                            }
                        }
                        
                        AddCapturedPiece(enemyPieceController.piece);
                        
                        List<UIChain> uiChains = _uiSystem.GetUI<UIChain>();
                        foreach (UIChain uiChain in uiChains)
                        {
                            uiChain.AddToChain(enemyPieceController, movesUsed, _capturedPieces.Count);
                        }
                        
                        _movesInThisTurn.RemoveAt(0);
                        break;
                    }
                    case PieceAbility.StopTurn:
                    {
                        AddCapturedPiece(enemyPieceController.piece);
                        
                        List<UIChain> uiChains = _uiSystem.GetUI<UIChain>();
                        foreach (UIChain uiChain in uiChains)
                        {
                            uiChain.AddToChain(enemyPieceController, movesUsed, _capturedPieces.Count);
                        }
                        
                        _movesInThisTurn.Clear();
                        break;
                    }
                    default:
                    {
                        AddCapturedPiece(enemyPieceController.piece);
                        
                        List<UIChain> uiChains = _uiSystem.GetUI<UIChain>();
                        foreach (UIChain uiChain in uiChains)
                        {
                            uiChain.AddToChain(enemyPieceController, movesUsed, _capturedPieces.Count);
                        }
                        
                        _movesInThisTurn.RemoveAt(0);
                        break;
                    }
                }

                Creator.statsCaptures++;
                
                won = _blackSystem.PieceCaptured(enemyPieceController);

                if (!won)
                {
                    //On win we play the 'win' sfx
                    _pitchAmount += 0.02f;
                    _audioSystem.PlayPieceCapturedSfx(_pitchAmount);   
                }
            }
            else
            {
                _movesInThisTurn.RemoveAt(0);
            }
            
            if (won)
            {
                //We win!
                _blackSystem.Lose(GameOverReason.Captured);
            }
            else if (_blackSystem.TryGetCaptureLoverMovingToPosition(_model.position, out AIController captureLoverController))
            {
                _whiteSystem.FreezeSide();
                _blackSystem.SelectCaptureLoverPiece(captureLoverController, _model.position);
            }
            else
            {
                SetToNextMove();
            }
        }
        else
        {
            _timer += dt * Creator.piecesSo.pieceSpeed;
            _timer = Mathf.Clamp(_timer, 0f, Mathf.PI);
            float t = Evaluate(_timer);
            _model.position = Vector3.Lerp(_model.position, _jumpPosition, t);
        }
    }
    
    private bool CanMove(Vector3 positionRequested)
    {
        List<ValidMove> validMoves = GetAllValidMovesOfCurrentPiece();
        foreach (ValidMove validMove in validMoves)
        {
            if ((Vector3)validMove.position == positionRequested)
            {
                return true;
            }
        }
        
        return false;
    }

    public bool IsPieceAtPosition(Vector3 position)
    {
        float d1 = math.distance(_model.position, position);
        float d2 = math.distance(_jumpPosition, position);
        
        return d1 < 0.01f || d2 < 0.01f;
    }
    
    private void MovePiece(Vector3 positionRequested)
    {
        // Set our position as a fraction of the distance between the markers.
        _jumpPosition = positionRequested;
        SetState(PieceState.Moving);
                
        _audioSystem.PlayPieceMoveSfx(1);
            
        _validMovesSystem.HideAllValidMoves();
    }

    private void SetToNextMove()
    {
        if (_movesInThisTurn.Count > 0)
        {
            //Allow player to make another move
            UpdateSprite(_movesInThisTurn[0]);
            
            List<ValidMove> validMoves = GetAllValidMovesOfCurrentPiece();
            
            if (validMoves.Count > 0)
            {
                List<UIChain> uiChains = _uiSystem.GetUI<UIChain>();
                foreach (UIChain uiChain in uiChains)
                {
                    uiChain.HighlightNextPiece(this);
                }
                SetState(PieceState.FindingMove);

                if (!_blackSystem.TickAlwaysMovers())
                {
                    _validMovesSystem.UpdateValidMoves(validMoves);
                }
            }
            else
            {
                //Blocked!
                SetState(PieceState.Blocked);
            }
        }
        else
        {
            _blackSystem.TickAlwaysMovers();
            
            List<UIChain> uiChains = _uiSystem.GetUI<UIChain>();
            foreach (UIChain uiChain in uiChains)
            {
                uiChain.HideAllPieces();
            }
            Finish();
        }
    }
    
    private void Blocked(float dt)
    {
        _timer -= dt;
        if (_timer <= 0)
        {
            _whiteSystem.Lose(GameOverReason.Locked);
        }
    }

    public void AddCapturedPiece(Piece piece)
    {
        _piecesCapturedInThisTurn++;
        
        _capturedPieces.Add(piece);
        _movesInThisTurn.Add(piece);
    }
    
    private float Evaluate(float x)
    {
        return 0.5f * Mathf.Sin(x - Mathf.PI / 2f) + 0.5f;
    }

    private void Finish()
    {
        SetState(PieceState.WaitingForTurn);
        _validMovesSystem.HideAllValidMoves();
        _turnSystem.SwitchTurn(PieceColour.Black);
    }

    private List<ValidMove> CheckValidDefiniteMoves(List<Vector3> moves)
    {
        List<ValidMove> validMoves = new();
        foreach (Vector3 move in moves)
        {
            Vector3 positionFromPlayer = _model.position + move;

            if (!_boardSystem.IsPositionValid(positionFromPlayer))
            {
                continue;
            }
            
            validMoves.Add(new()
            {
                position = positionFromPlayer,
                enemyHere = _blackSystem.IsPieceAtPosition(positionFromPlayer)
            });
        }

        return validMoves;
    }

    private List<ValidMove> CheckValidIndefiniteMoves(List<Vector3> moves)
    {
        List<ValidMove> validMoves = new();
        foreach (Vector3 move in moves)
        {
            Vector3 furthestPointOfMoveLine = _model.position;
            while (true)
            {
                Vector3 nextSpot = furthestPointOfMoveLine + move;
                if (!_boardSystem.IsPositionValid(nextSpot))
                {
                    break;
                }
                
                bool enemyHere = _blackSystem.IsPieceAtPosition(nextSpot);
                
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

    public List<ValidMove> GetAllValidMovesOfFirstPiece()
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
                int direction = 1;
                
                List<ValidMove> pawnMoves = new();

                Vector3 defaultMove = _model.position + new Vector3(0, 1, 0) * direction;
                if (_boardSystem.IsPositionValid(defaultMove) && !_blackSystem.IsPieceAtPosition(defaultMove))
                {
                    pawnMoves.Add(new()
                    {
                        position = defaultMove,
                        enemyHere = false
                    });
                }
                
                Vector3 topLeft = _model.position + new Vector3(-1, 1, 0) * direction;
                if (_blackSystem.IsPieceAtPosition(topLeft))
                {
                    pawnMoves.Add(new()
                    {
                        position = topLeft,
                        enemyHere = true
                    });
                }
                
                Vector3 topRight = _model.position + new Vector3(1, 1, 0) * direction;
                if (_blackSystem.IsPieceAtPosition(topRight))
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

    private void UpdateSprite(Piece piece)
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

    public void SetState(PieceState state)
    {
        if (_state == PieceState.Blocked || _state == PieceState.EndGame)
        {
            return;
        }
        
        switch (state)
        {
            case PieceState.NotInUse:
            {
                _model.gameObject.SetActive(false);

                break;
            }
            case PieceState.FindingMove:
            {
                _background.SetActive(true);

                break;
            }
            case PieceState.Moving:
            {
                _background.SetActive(false);

                break;
            }
            case PieceState.WaitingForTurn:
            {
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
            }
            case PieceState.Blocked:
            {
                _timer = _scaleTween.phaseOutTime; //Shrink animation length
                _scaleTween.Shrink();
                
                _background.SetActive(false);
                
                break;
            }
        }
        
        _state = state;
    }
    
    public override void Destroy()
    {
        _poolSystem.ReturnObjectToPool(_model.gameObject);
    }
}
