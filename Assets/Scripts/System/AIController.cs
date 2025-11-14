using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using TMPro;

public class AIController : Dependency
{
    private TurnSystem _turnSystem;
    
    private ValidMovesSystem _validMovesSystem;
    private BoardSystem _boardSystem;
    private AudioSystem _audioSystem;
    private WhiteSystem _whiteSystem;
    private BlackSystem _blackSystem;
    private PoolSystem _poolSystem;

    public Piece piece => _piece;
    
    public PieceAbility pieceAbility => _pieceAbility;
    
    public PieceState state => _state;
    
    public Vector3 piecePos => _model.position;

    public Vector3 jumpPos => _jumpPosition;
    
    private Transform _model;
    
    private SpriteRenderer _spriteRenderer;
    
    private ScaleTween _scaleTween;

    private Transform _abilityTextParent;
    
    private Piece _piece;
    private PieceAbility _pieceAbility;
    private PieceState _state;
    private Vector3 _jumpPosition;
    private float _timer;
    
    private float _thinkingTimer;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _turnSystem = creator.GetDependency<TurnSystem>();
        _validMovesSystem = creator.GetDependency<ValidMovesSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();
        _audioSystem = creator.GetDependency<AudioSystem>();
        _whiteSystem = creator.GetDependency<WhiteSystem>();
        _blackSystem = creator.GetDependency<BlackSystem>();
        _poolSystem = creator.GetDependency<PoolSystem>();
    }

    public void Init(Vector3 position, Piece piece, PieceAbility pieceAbility)
    {
        _model = _poolSystem.GetPieceObjectFromPool(position).transform;
        
        GameObject background = Creator.GetChildObjectByName(_model.gameObject, AllTagNames.Background).gameObject;
        
        _spriteRenderer = Creator.GetChildComponentByName<SpriteRenderer>(_model.gameObject, AllTagNames.PlayerSprite);

        _scaleTween = _model.GetComponentInChildren<ScaleTween>();
        _scaleTween.Enlarge();

        //Player does not have an ability, so the text canvas can be disabled
        TMP_Text abilityText = Creator.GetChildComponentByName<TMP_Text>(_model.gameObject, AllTagNames.Text);
        abilityText.text = Creator.piecesSo.GetPieceAbilityText(pieceAbility);

        _abilityTextParent = abilityText.transform.parent.parent;

        _jumpPosition = _model.position;
        
        _pieceAbility = pieceAbility;

        _piece = piece;
        
        UpdateSprite(piece);
        _spriteRenderer.color = Color.white; //Colour is defined on the material
        switch (pieceAbility)
        {
            case PieceAbility.None:
            {
                _spriteRenderer.material = Creator.piecesSo.noneMat;
                _spriteRenderer.material.color = Creator.piecesSo.blackColor;
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

        background.SetActive(false);

        SetAbilityText(Creator.saveDataSo.showAbilityText);
        
        _state = PieceState.NotInUse;
        
        SetThinkingTime();
    }

    public void SetAbilityText(bool show)
    {
        _abilityTextParent.gameObject.SetActive(show);
    }

    private void FindingMove(float dt)
    {
        _thinkingTimer -= dt;
        if (_thinkingTimer > 0)
        {
            return;
        }

        List<ValidMove> possiblePositions = GetAllValidMovesOfPiece();
                
        if (possiblePositions.Count > 0)
        {
            //Go through each position and see if the player is at that position. If they are, capture it!
            bool foundPlayer = false;
            foreach (ValidMove possiblePosition in possiblePositions)
            {
                if (_whiteSystem.playerController.IsPieceAtPosition(possiblePosition.position))
                {
                    _jumpPosition = possiblePosition.position;
                    foundPlayer = true;
                    break;
                }
            }
            if (!foundPlayer)
            {
                int chosenPositionIndex = Random.Range(0, possiblePositions.Count);
                _jumpPosition = possiblePositions[chosenPositionIndex].position;
            }
            
            _audioSystem.PlayPieceMoveSfx(1);
            SetState(PieceState.Moving);
        }
        else
        {
            SetState(PieceState.Blocked);
        }
    }

    private void Moving(float dt)
    {
        _thinkingTimer -= dt;
        if (_thinkingTimer > 0)
        {
            return;
        }

        if (_model.position == _jumpPosition)
        {
            _model.position = math.round(_model.position);
            _timer = 0;
            
            if (math.distance(_whiteSystem.playerController.piecePos, _model.position) < 0.01f)
            {
                _whiteSystem.Lose(GameOverReason.Captured);
            }
            else
            {
                Finish();
                SetThinkingTime();
            }
        }
        else
        {
            float speed = _pieceAbility == PieceAbility.AlwaysMove
                ? Creator.piecesSo.alwaysMoveSpeed
                : Creator.piecesSo.pieceSpeed;
            _timer += dt * speed;
            _timer = Mathf.Clamp(_timer, 0f, Mathf.PI);
            float t = Evaluate(_timer);
            _model.position = Vector3.Lerp(_model.position, _jumpPosition, t);
        }
    }

    public void ForceMove(float3 movePosition)
    {
        SetThinkingTime();
        
        _jumpPosition = movePosition;

        SetState(PieceState.Moving);
    }

    private void SetThinkingTime()
    {
        _thinkingTimer = _pieceAbility == PieceAbility.AlwaysMove 
            ? Creator.piecesSo.alwaysMoveThinkingTime 
            : Creator.piecesSo.aiThinkingTime;
    }
    
    public override void GameUpdate(float dt)
    {
        //State machine
        switch (_state)
        {
            case PieceState.WaitingForTurn:
                break;
            case PieceState.FindingMove:
                FindingMove(dt);
                break;
            case PieceState.Moving:
                Moving(dt);
                break;
            case PieceState.NotInUse:
                break;
            case PieceState.Blocked:
                Blocked(dt);
                break;
            case PieceState.EndGame:
                break;
        }
    }

    private void Blocked(float dt)
    {
        _timer -= dt;
        if (_timer <= 0)
        {
            _blackSystem.PieceBlocked(this);
            
            if (_whiteSystem.playerController != null && _pieceAbility == PieceAbility.AlwaysMove && _turnSystem.CurrentTurn() == PieceColour.White)
            {
                _validMovesSystem.UpdateValidMoves(_whiteSystem.playerController.GetAllValidMovesOfCurrentPiece());

                _whiteSystem.UnfreezeSide();
            }
            
            SetState(PieceState.NotInUse);
        }
    }
    
    private float Evaluate(float x)
    {
        return 0.5f * Mathf.Sin(x - Mathf.PI / 2f) + 0.5f;
    }

    private void Finish()
    {
        if (_pieceAbility == PieceAbility.AlwaysMove && _turnSystem.CurrentTurn() == PieceColour.White)
        {
            SetState(PieceState.WaitingForTurn);
            _validMovesSystem.UpdateValidMoves(_whiteSystem.playerController.GetAllValidMovesOfCurrentPiece());

            _whiteSystem.UnfreezeSide();
        }
        else
        {
            SetState(PieceState.WaitingForTurn);
            _validMovesSystem.HideAllValidMoves();
            
            _blackSystem.PieceFinished(this);
        }
    }

    private List<ValidMove> CheckValidDefiniteMoves(List<Vector3> moves)
    {
        List<ValidMove> validMoves = new();
        foreach (Vector3 move in moves)
        {
            Vector3 positionFromPlayer = _model.position + move;

            if (!_boardSystem.IsPositionValid(positionFromPlayer) 
                || _blackSystem.IsPieceAtPosition(positionFromPlayer))
            {
                continue;
            }
            
            bool enemyHere = math.distance(_whiteSystem.playerController.piecePos, positionFromPlayer) < 0.01f;
            
            validMoves.Add(new()
            {
                position = positionFromPlayer,
                enemyHere = enemyHere
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
                if (!_boardSystem.IsPositionValid(nextSpot) 
                    || _blackSystem.IsPieceAtPosition(nextSpot))
                {
                    break;
                }

                bool enemyHere = math.distance(_whiteSystem.playerController.piecePos, nextSpot) < 0.01f;
                
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

    public List<ValidMove> GetAllValidMovesOfPiece()
    {
        List<ValidMove> validMoves = new(64);
        switch (piece)
        {
            case Piece.Pawn:
            {
                int direction = -1;
                
                List<ValidMove> pawnMoves = new();

                Vector3 defaultMove = _model.position + new Vector3(0, 1, 0) * direction;
                if (_boardSystem.IsPositionValid(defaultMove)
                    && !_blackSystem.IsPieceAtPosition(defaultMove)
                    && math.distance(_whiteSystem.playerController.piecePos, defaultMove) > 0.01f)
                {
                    pawnMoves.Add(new()
                    {
                        position = defaultMove,
                        enemyHere = false
                    });
                }
                
                Vector3 topLeft = _model.position + new Vector3(-1, 1, 0) * direction;
                if (math.distance(_whiteSystem.playerController.piecePos, topLeft) < 0.01f 
                    && _boardSystem.IsPositionValid(topLeft))
                {
                    pawnMoves.Add(new()
                    {
                        position = topLeft,
                        enemyHere = true
                    });
                }
                
                Vector3 topRight = _model.position + new Vector3(1, 1, 0) * direction;
                if (math.distance(_whiteSystem.playerController.piecePos, topRight) < 0.01f
                    && _boardSystem.IsPositionValid(topRight))
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
            case PieceState.Blocked:
            {
                _timer = _scaleTween.phaseOutTime; //Shrink animation length
                _scaleTween.Shrink();
                SetAbilityText(false);
                break;
            }
        }
        
        _state = state;
    }

    public void PlaySelectedAnimation()
    {
        _scaleTween.Selected();
    }
    
    public override void Destroy()
    {
        _poolSystem.ReturnObjectToPool(_model.gameObject);
    }
}
