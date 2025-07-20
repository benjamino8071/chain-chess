using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevPieceController : LevController
{
    protected LevValidMovesSystem _validMovesSystem;
    protected LevBoardSystem _boardSystem;
    protected LevChainUISystem _chainUISystem;
    protected LevAudioSystem _audioSystem;
    
    public enum States
    {
        WaitingForTurn,
        FindingMove,
        ConfirmingMove,
        Moving,
        NotInUse,
        Paused,
        EndGame
    }

    public PieceAbility pieceAbility => _pieceAbility;
    
    public PieceColour pieceColour => _pieceColour;
    
    public States state => _state;
    
    public Vector3 piecePos => _pieceInstance.position;
    
    public List<Piece> capturedPieces => _capturedPieces;
    
    public bool hasMoved => _hasMoved;
    
    public int movesUsed => _capturedPieces.Count - _movesInThisTurn.Count;

    public int movesRemaining => _movesInThisTurn.Count;
    
    public int piecesCapturedInThisTurn => _piecesCapturedInThisTurn;

    public ControlledBy controlledBy => _controlledBy;
    
    protected List<Piece> _capturedPieces = new(16);
    protected List<Piece> _movesInThisTurn = new(16);
    
    protected Transform _pieceInstance;

    protected SpriteRenderer _spriteRenderer;

    protected TMP_Text _captureAmountText;

    protected LevSideSystem _allySideSystem;
    protected LevSideSystem _enemySideSystem;
    
    protected Piece _currentPiece;
    protected PieceAbility _pieceAbility;
    protected PieceColour _pieceColour;
    protected PieceColour _enemyColour;
    protected ControlledBy _controlledBy;
    protected States _state;
    protected Vector3 _jumpPosition;
    protected int _piecesCapturedInThisTurn;
    protected float _moveSpeed;
    protected float _sinTime;
    protected bool _hasMoved;
    protected bool _madeMove;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
        _boardSystem = levCreator.GetDependency<LevBoardSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
    }

    public virtual void Init(Vector3 position, List<Piece> startingPieces, PieceColour pieceColour, PieceAbility pieceAbility, 
        ControlledBy controlledBy, LevSideSystem allySideSystem, LevSideSystem enemySideSystem)
    {
        _pieceInstance = Creator.InstantiateGameObject(Creator.piecePrefab, position, Quaternion.identity).transform;
        
        _pieceInstance.GetComponentInChildren<Canvas>(true).gameObject.SetActive(false);

        
        _jumpPosition = _pieceInstance.position;
        
        _spriteRenderer = _pieceInstance.GetComponentInChildren<SpriteRenderer>();

        _pieceAbility = pieceAbility;

        _controlledBy = controlledBy;

        _allySideSystem = allySideSystem;

        _enemySideSystem = enemySideSystem;
        
        Transform captureAmountText = Creator.GetChildObjectByName(_pieceInstance.gameObject, AllTagNames.Text);
        _captureAmountText = captureAmountText.GetComponent<TMP_Text>();
        
        _pieceColour = pieceColour;
        _enemyColour = pieceColour == PieceColour.White ? PieceColour.Black : PieceColour.White;

        foreach (Piece startingPiece in startingPieces)
        {
            _capturedPieces.Add(startingPiece);
            _movesInThisTurn.Add(startingPiece);
        }

        switch (pieceAbility)
        {
            case PieceAbility.None:
            {
                _spriteRenderer.material = Creator.piecesSo.noneMat;
                _spriteRenderer.material.color = pieceColour == PieceColour.White ? Creator.piecesSo.whiteColor : Creator.piecesSo.blackColor;
                break;
            }
            case PieceAbility.MustMove:
            {
                _spriteRenderer.material = Creator.piecesSo.mustMoveMat;
                _spriteRenderer.material.color = Creator.piecesSo.mustMoveColor;
                break;
            }
            case PieceAbility.CaptureLover:
            {
                _spriteRenderer.material = Creator.piecesSo.captureLoverMat;
                _spriteRenderer.material.color = Creator.piecesSo.captureLoverColor;
                break;
            }
            case PieceAbility.Glitched:
            {
                _spriteRenderer.material = Creator.piecesSo.glitchedMat;
                _spriteRenderer.material.color = pieceColour == PieceColour.White ? Creator.piecesSo.whiteColor : Creator.piecesSo.blackColor;
                break;
            }
        }
        
        UpdateSprite(startingPieces[0]);
        
        SetState(pieceColour == PieceColour.White ? States.FindingMove : States.WaitingForTurn);
        
        UpdateCaptureAmountText(startingPieces.Count);
        UpdateCaptureAmountTextColour();
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
            case States.ConfirmingMove:
                ConfirmingMove(dt);
                break;
            case States.Moving:
                Moving(dt);
                break;
            case States.NotInUse:
                break;
            case States.EndGame:
                break;
        }
    }

    protected virtual void FindingMove(float dt)
    {
        
    }

    protected virtual void ConfirmingMove(float dt)
    {
        
    }
    
    protected virtual void Moving(float dt)
    {
        
    }

    public void AddCapturedPiece(Piece piece)
    {
        _piecesCapturedInThisTurn++;
        if (Creator.isPuzzle 
            && controlledBy == ControlledBy.Player
            && _piecesCapturedInThisTurn > Creator.statsBestTurn)
        {
            Creator.statsBestTurn = _piecesCapturedInThisTurn;
        }

        _capturedPieces.Add(piece);
        _movesInThisTurn.Add(piece);
        UpdateCaptureAmountText(_capturedPieces.Count);
    }
    
    protected float Evaluate(float x)
    {
        return 0.5f * Mathf.Sin(x - Mathf.PI / 2f) + 0.5f;
    }
    
    protected void SetToNextMove()
    {
        _hasMoved = true;
        _madeMove = true;
        
        UpdateCaptureAmountTextColour();
        
        if (_movesInThisTurn.Count > 0)
        {
            //Allow player to make another move
            UpdateSprite(_movesInThisTurn[0]);
            
            List<Vector3> validMoves = GetAllValidMovesOfCurrentPiece();
            if (validMoves.Count > 0)
            {
                _chainUISystem.HighlightNextPiece(movesUsed);
                SetState(States.FindingMove);
            }
            else
            {
                //Locked!
                _allySideSystem.PieceLocked(this);
                Finish();
            }
        }
        else
        {
            Finish();
        }
    }

    private void Finish()
    {
        SetState(States.WaitingForTurn);
        _validMovesSystem.HideAllValidMoves();
        _allySideSystem.PieceFinished(this);
    }

    protected List<Vector3> CheckValidDefiniteMoves(List<Vector3> moves)
    {
        List<Vector3> validMoves = new();
        foreach (Vector3 move in moves)
        {
            Vector3 positionFromPlayer = _pieceInstance.position + move;


            if (!_boardSystem.IsPositionValid(positionFromPlayer) 
                || _boardSystem.IsAllyAtPosition(positionFromPlayer, _pieceColour))
            {
                continue;
            }
            
            validMoves.Add(positionFromPlayer);
        }

        return validMoves;
    }

    protected List<Vector3> CheckValidIndefiniteMoves(List<Vector3> moves)
    {
        List<Vector3> validMoves = new();
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
                
                furthestPointOfMoveLine = nextSpot;
                validMoves.Add(furthestPointOfMoveLine);

                if (_boardSystem.IsEnemyAtPosition(nextSpot, _enemyColour))
                {
                    break;
                }
            }
        }

        return validMoves;
    }

    public List<Vector3> AllValidMovesOfFirstPiece()
    {
        return GetAllValidMovesOfPiece(_capturedPieces[0]); 
    }

    public List<Vector3> GetAllValidMovesOfCurrentPiece()
    {
        return GetAllValidMovesOfPiece(_movesInThisTurn[0]);
    }

    private List<Vector3> GetAllValidMovesOfPiece(Piece piece)
    {
        List<Vector3> validMoves = new(64);
        switch (piece)
        {
            case Piece.Pawn:
            {
                int direction = _pieceColour == PieceColour.White ? 1 : -1;
                
                List<Vector3> pawnMoves = new();

                Vector3 defaultMove = _pieceInstance.position + new Vector3(0, 1, 0) * direction;
                if (!_boardSystem.IsAllyAtPosition(defaultMove, _pieceColour) 
                    && !_boardSystem.IsEnemyAtPosition(defaultMove, _enemyColour)
                    && _boardSystem.IsPositionValid(defaultMove))
                {
                    pawnMoves.Add(defaultMove);
                    
                    Vector3 startingMove = _pieceInstance.position + new Vector3(0, 2, 0) * direction;
                    
                    bool atStartPoint = pieceColour == PieceColour.White 
                        ? (int)_pieceInstance.position.y == 2 
                        : (int)_pieceInstance.position.y == 7;
                    
                    if (atStartPoint
                        && !_boardSystem.IsAllyAtPosition(startingMove, _pieceColour) 
                        && !_boardSystem.IsEnemyAtPosition(startingMove, _enemyColour)
                        && _boardSystem.IsPositionValid(startingMove))
                    {
                        pawnMoves.Add(startingMove);
                    }
                }
                
                Vector3 topLeft = _pieceInstance.position + new Vector3(-1, 1, 0) * direction;
                if (_boardSystem.IsEnemyAtPosition(topLeft, _enemyColour) && _boardSystem.IsPositionValid(topLeft))
                {
                    pawnMoves.Add(topLeft);
                }
                
                Vector3 topRight = _pieceInstance.position + new Vector3(1, 1, 0) * direction;
                if (_boardSystem.IsEnemyAtPosition(topRight, _enemyColour) && _boardSystem.IsPositionValid(topRight))
                {
                    pawnMoves.Add(topRight);
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
        if (_state is States.NotInUse or States.EndGame)
        {
            return;
        }
        
        switch (state)
        {
            case States.NotInUse:
                _pieceInstance.gameObject.SetActive(false);
                break;
            case States.FindingMove:
                break;
            case States.WaitingForTurn:
                if (_movesInThisTurn.Count == 0)
                {
                    if (_pieceAbility == PieceAbility.Glitched)
                    {
                        for (int i = 0; i < _capturedPieces.Count; i++)
                        {
                            _capturedPieces[i] = GenerateOtherRandomPiece(_capturedPieces[i]);
                        }
                    }
                    
                    UpdateSprite(_capturedPieces[0]);
                    foreach (Piece capturedPiece in _capturedPieces)
                    {
                        _movesInThisTurn.Add(capturedPiece);
                    }
                }
                _piecesCapturedInThisTurn = 0; //For next time the player uses this piece
                _hasMoved = false;
                break;
        }
        _state = state;
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

    private void UpdateCaptureAmountText(int amount)
    {
        _captureAmountText.text = $"{amount}";
    }

    private void UpdateCaptureAmountTextColour()
    {
        int piecePosX = (int)_pieceInstance.position.x;
        int piecePosY = (int)_pieceInstance.position.y;
        int result = piecePosX + piecePosY;
        
        Color movesInTurnTextColor;
        if (result % 2 == 0)
        {
            movesInTurnTextColor = Color.white;
        }
        else
        {
            movesInTurnTextColor = Color.black;
        }
        _captureAmountText.color = movesInTurnTextColor;
    }
    
    public override void Destroy()
    {
        Object.Destroy(_pieceInstance.gameObject);
    }
}
