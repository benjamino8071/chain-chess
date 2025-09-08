using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : PieceController
{
    private Vector3 _positionRequested;
    
    protected override void FindingMove(float dt)
    {
        //The player will find the move when they are ready
        Vector3 positionRequested = _boardSystem.GetGridPointNearMouse();
        if (Creator.inputSo.leftMouseButton.action.WasPerformedThisFrame() && CanMove(positionRequested))
        {
            if (Creator.settingsSo.doubleTap)
            {
                SetState(States.ConfirmingMove);
                _validMovesSystem.ShowSingleValidMove(positionRequested);
                _positionRequested = positionRequested;
                _audioSystem.PlayPieceDoubleTapSelectedSfx(0.8f);
            }
            else
            {
                MovePiece(positionRequested);
            }
        }
    }

    protected override void ConfirmingMove(float dt)
    {
        //The player will find the move when they are ready
        Vector3 positionRequested = _boardSystem.GetGridPointNearMouse();
        if (Creator.inputSo.leftMouseButton.action.WasPerformedThisFrame())
        {
            if (_positionRequested == positionRequested)
            {
                MovePiece(positionRequested);
            }
            else
            {
                _allySideSystem.SelectPiece(this);
            }
            
            _positionRequested = Vector3.zero; //For next time the player double taps
        }
    }

    protected override void Moving(float dt)
    {
        if (_pieceInstance.position == _jumpPosition)
        {
            Creator.statsMoves++;
            
            _pieceInstance.position = math.round(_pieceInstance.position);
            _timer = 0;

            bool won = false;
            PieceController enemyPieceController = _enemySideSystem.GetPieceAtPosition(_pieceInstance.position);
            if (enemyPieceController is not null)
            {
                switch (enemyPieceController.pieceAbility)
                {
                    case PieceAbility.Resetter:
                        _capturedPieces.Clear();
                        _movesInThisTurn.Clear();
                        _piecesCapturedInThisTurn = 0;
                    
                        AddCapturedPiece(enemyPieceController.capturedPieces[0]);
                    
                        _chainUISystem.ShowChain(this, true);
                        break;
                    case PieceAbility.Multiplier:
                        float3 topLeft = enemyPieceController.piecePos + new Vector3(-1, 1, 0);
                        float3 topRight = enemyPieceController.piecePos + new Vector3(1, 1, 0);
                        float3 botLeft = enemyPieceController.piecePos + new Vector3(-1, -1, 0);
                        float3 botRight = enemyPieceController.piecePos + new Vector3(1, -1, 0);
                        if (_boardSystem.IsPositionValid(topLeft))
                        {
                            _enemySideSystem.CreatePiece(new(topLeft.x, topLeft.y), Piece.Knight, PieceAbility.None);
                        }
                        if (_boardSystem.IsPositionValid(topRight))
                        {
                            _enemySideSystem.CreatePiece(new(topRight.x, topRight.y), Piece.Bishop, PieceAbility.None);
                        }
                        if (_boardSystem.IsPositionValid(botLeft))
                        {
                            _enemySideSystem.CreatePiece(new(botLeft.x, botLeft.y), Piece.King, PieceAbility.None);
                        }
                        if (_boardSystem.IsPositionValid(botRight))
                        {
                            _enemySideSystem.CreatePiece(new(botRight.x, botRight.y), Piece.Queen, PieceAbility.None);
                        }
                        
                        
                        /*List<float3> newPiecePosses = new()
                        {
                            enemyPieceController.piecePos + new Vector3(-1, 1, 0),
                            enemyPieceController.piecePos + new Vector3(1, 1, 0),
                            enemyPieceController.piecePos + new Vector3(-1, -1, 0),
                            enemyPieceController.piecePos + new Vector3(1, -1, 0)
                        };
                        foreach (float3 newPiecePos in newPiecePosses)
                        {
                            if (_boardSystem.IsPositionValid(newPiecePos))
                            {
                                _enemySideSystem.CreatePiece(new(newPiecePos.x, newPiecePos.y), enemyPieceController.currentPiece, PieceAbility.None);
                            }
                        }*/
                        
                        AddCapturedPiece(enemyPieceController.capturedPieces[0]);
                        _chainUISystem.AddToChain(enemyPieceController, _capturedPieces.Count);
                        
                        _movesInThisTurn.RemoveAt(0);
                        break;
                    case PieceAbility.StopTurn:
                    {
                        AddCapturedPiece(enemyPieceController.capturedPieces[0]);
                        _chainUISystem.AddToChain(enemyPieceController, _capturedPieces.Count);
                        
                        _movesInThisTurn.Clear();
                        break;
                    }
                    default:
                        AddCapturedPiece(enemyPieceController.capturedPieces[0]);
                        _chainUISystem.AddToChain(enemyPieceController, _capturedPieces.Count);
                    
                        _movesInThisTurn.RemoveAt(0);
                        break;
                }
                
                won = _enemySideSystem.PieceCaptured(enemyPieceController);
                
                float pitchAmount = 1 + 0.02f * _piecesCapturedInThisTurn;
                _audioSystem.PlayPieceCapturedSfx(pitchAmount);
            }
            else
            {
                _movesInThisTurn.RemoveAt(0);
            }
            
            if (won)
            {
                //We win!
                _enemySideSystem.Lose(GameOverReason.Captured, 0);
            }
            else if (_enemySideSystem.TryGetCaptureLoverMovingToPosition(_pieceInstance.position, out PieceController captureLoverController))
            {
                _allySideSystem.FreezeSide();
                _enemySideSystem.SelectCaptureLoverPiece(captureLoverController, _pieceInstance.position);
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
            _pieceInstance.position = Vector3.Lerp(_pieceInstance.position, _jumpPosition, t);
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

    private void MovePiece(Vector3 positionRequested)
    {
        // Set our position as a fraction of the distance between the markers.
        _jumpPosition = positionRequested;
        SetState(States.Moving);
                
        _audioSystem.PlayPieceMoveSfx(1);
            
        _validMovesSystem.HideAllValidMoves();
    }

    protected override void SetToNextMove()
    {
        if (_movesInThisTurn.Count > 0)
        {
            //Allow player to make another move
            UpdateSprite(_movesInThisTurn[0]);
            
            List<ValidMove> validMoves = GetAllValidMovesOfCurrentPiece();
            
            if (validMoves.Count > 0)
            {
                _chainUISystem.HighlightNextPiece(this);
                SetState(States.FindingMove);

                if (!_boardSystem.blackSystem.TickAlwaysMovers())
                {
                    _validMovesSystem.UpdateValidMoves(validMoves);
                }
            }
            else
            {
                //Blocked!
                SetState(States.Blocked);
            }
        }
        else
        {
            _boardSystem.blackSystem.TickAlwaysMovers();
            
            _chainUISystem.HideAllPieces();
            Finish();
        }
    }
}
