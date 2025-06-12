using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LevPieceController : LevController
{
    protected LevValidMovesSystem _validMovesSystem;
    protected LevBoardSystem _boardSystem;
    protected LevChainUISystem _chainUISystem;
    protected LevAudioSystem _audioSystem;
    protected LevTurnSystem _turnSystem;
    
    public enum States
    {
        WaitingForTurn,
        FindingMove,
        ConfirmingMove,
        Moving,
        NotInUse,
        EndGame
    }

    public PieceColour pieceColour => _pieceColour;
    
    public States state => _state;
    
    public Vector3 piecePos => _pieceInstance.position;
    
    public List<Piece> capturedPieces => _capturedPieces;
    
    public bool hasMoved => _capturedPieces.Count != _movesInThisTurn.Count;
    
    public int movesUsed => _capturedPieces.Count - _movesInThisTurn.Count;
    
    public int piecesCapturedInThisTurn => _piecesCapturedInThisTurn;

    protected List<Piece> _capturedPieces = new(16);
    protected List<Piece> _movesInThisTurn = new(16);
    
    protected Transform _pieceInstance;

    protected SpriteRenderer _spriteRenderer;

    protected TMP_Text _captureAmountText;
    
    protected PieceColour _pieceColour;
    protected PieceColour _enemyColour;
    protected States _state;
    protected Vector3 _jumpPosition;
    protected int _piecesCapturedInThisTurn;
    protected float _moveSpeed;
    protected float _sinTime;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
        _boardSystem = levCreator.GetDependency<LevBoardSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
    }

    public virtual void Init(Vector3 position, Piece startingPiece, PieceColour pieceColour)
    {
        _pieceInstance = Creator.InstantiateGameObject(Creator.piecePrefab, position, Quaternion.identity).transform;
        
        _pieceInstance.GetComponentInChildren<Canvas>(true).gameObject.SetActive(false);
        
        _jumpPosition = _pieceInstance.position;
        
        _spriteRenderer = _pieceInstance.GetComponentInChildren<SpriteRenderer>();

        Transform captureAmountText = Creator.GetChildObjectByName(_pieceInstance.gameObject, AllTagNames.Text);
        _captureAmountText = captureAmountText.GetComponent<TMP_Text>();
        
        Color color = pieceColour == PieceColour.White ? Creator.piecesSo.whiteColor : Creator.piecesSo.blackColor;
        _spriteRenderer.color = color;
        
        _pieceColour = pieceColour;
        _enemyColour = pieceColour == PieceColour.White ? PieceColour.Black : PieceColour.White;
        
        _capturedPieces.Add(startingPiece);
        SetState(pieceColour == PieceColour.White ? States.FindingMove : States.WaitingForTurn);
        
        UpdateCaptureAmountText(1);
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
        UpdateCaptureAmountTextColour();
        
        if (_movesInThisTurn.Count > 0)
        {
            //Allow player to make another move
            UpdateSprite(_movesInThisTurn[0]);
            
            List<Vector3> validMoves = GetAllValidMovesOfCurrentPiece();
            if (validMoves.Count > 0)
            {
                _chainUISystem.HighlightNextPiece(movesUsed);
                _validMovesSystem.UpdateSelectedBackground(_pieceInstance.position);
                SetState(States.FindingMove);
            }
            else
            {
                /*
                 * TODO(1): As the player can have other pieces this may not be necessary.
                 * TODO(2): Will need to do a check on other pieces for this side, if there are non-pawn pieces then all good
                 * TODO(3): ALTERNATIVE: We just make this side lose the rest of their moves.
                 */
                _movesInThisTurn.RemoveAt(0);
                SetToNextMove();
            }
        }
        else
        {
            SetState(States.WaitingForTurn);
            _chainUISystem.UnsetChain();
            _validMovesSystem.HideAllValidMoves();
            _turnSystem.SwitchTurn(_enemyColour == PieceColour.White ? PieceColour.White : PieceColour.Black);
        }
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

    public List<Vector3> GetAllValidMovesOfCurrentPiece()
    {
        Piece piece = _movesInThisTurn[0];
        
        List<Vector3> validMoves = new(64);
        switch (piece)
        {
            case Piece.Pawn:
                List<Vector3> pawnMoves = new();

                Vector3 defaultMove = new Vector3(0, 1, 0);
                if (!_boardSystem.IsAllyAtPosition(_pieceInstance.position + defaultMove, _pieceColour) 
                    && !_boardSystem.IsEnemyAtPosition(_pieceInstance.position + defaultMove, _enemyColour))
                {
                    pawnMoves.Add(defaultMove);
                }
                Vector3 topLeft = new Vector3(-1, 1, 0);
                if (_boardSystem.IsAllyAtPosition(_pieceInstance.position + topLeft, _pieceColour))
                {
                    pawnMoves.Add(topLeft);
                }
                Vector3 topRight = new Vector3(1, 1, 0);
                if (_boardSystem.IsAllyAtPosition(_pieceInstance.position + topRight, _pieceColour))
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

    protected void UpdateSprite(Piece piece)
    {
        switch (piece)
        {
            case Piece.NotChosen:
                _spriteRenderer.sprite = default;
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

    public void SetState(States state)
    {
        if (_state == States.NotInUse || _state == States.EndGame)
        {
            return;
        }
        
        switch (state)
        {
            case States.NotInUse:
                _pieceInstance.gameObject.SetActive(false);
                break;
            case States.FindingMove:
                if (_movesInThisTurn.Count == 0)
                {
                    UpdateSprite(_capturedPieces[0]);
                    //Reset possible moves
                    foreach (Piece capturedPiece in _capturedPieces)
                    {
                        _movesInThisTurn.Add(capturedPiece);
                    }
                    _piecesCapturedInThisTurn = 0;
                }
                break;
            case States.WaitingForTurn:
                UpdateSprite(_capturedPieces[0]);
                break;
        }
        _state = state;
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
