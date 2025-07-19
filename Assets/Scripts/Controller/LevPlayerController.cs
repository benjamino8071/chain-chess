using System.Collections.Generic;
using UnityEngine;

public class LevPlayerController : LevPieceController
{
    private Vector3 _positionRequested;
    
    protected override void FindingMove(float dt)
    {
        //The player will find the move when they are ready
        Vector3 positionRequested = _boardSystem.GetGridPointNearMouse();
        if (Creator.inputSo._leftMouseButton.action.WasPerformedThisFrame() && CanMove(positionRequested))
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
        if (Creator.inputSo._leftMouseButton.action.WasPerformedThisFrame())
        {
            if (_positionRequested == positionRequested)
            {
                MovePiece(positionRequested);
            }
            else if(hasMoved)
            {
                _validMovesSystem.UpdateValidMoves(GetAllValidMovesOfCurrentPiece());
                SetState(States.FindingMove);
            }
            else
            {
                //Later in LevSideSystem.GameUpdate we check this state and if it equals, then we un-select
                SetState(States.FindingMove);
            }
            _positionRequested = Vector3.zero; //For next time the player double taps
        }
    }

    protected override void Moving(float dt)
    {
        if (_pieceInstance.position != _jumpPosition)
        {
            _sinTime += dt * _moveSpeed;
            _sinTime = Mathf.Clamp(_sinTime, 0f, Mathf.PI);
            float t = Evaluate(_sinTime);
            _pieceInstance.position = Vector3.Lerp(_pieceInstance.position, _jumpPosition, t);
        }
                
        if (_pieceInstance.position == _jumpPosition)
        {
            int posX = (int)_pieceInstance.position.x;
            int posY = (int)_pieceInstance.position.y;
            
            _pieceInstance.position = new Vector3(posX + 0.5f, posY + 0.5f, 0);
            _sinTime = 0;
            
            bool reachedPromoPoint = pieceColour == PieceColour.White ? posY == 8 : posY == 1;
            if (reachedPromoPoint && _currentPiece == Piece.Pawn)
            {
                PromotePiece();
            }
            
            if (_boardSystem.TryCaptureEnemyPiece(_pieceInstance.position, _enemyColour, this))
            {
                _movesInThisTurn.RemoveAt(0);
                
                //We win!
                UpdateSprite(_movesInThisTurn[0]);
            }
            else if(_enemySideSystem.TryGetCaptureLoverMovingToPosition(_pieceInstance.position, out LevPieceController playerController))
            {
                _allySideSystem.FreezeSide();
                _enemySideSystem.SelectCaptureLoverPiece(playerController);
            }
            else
            {
                _movesInThisTurn.RemoveAt(0);
                
                SetToNextMove();
                if (_state == States.FindingMove)
                {
                    _validMovesSystem.UpdateValidMoves(GetAllValidMovesOfCurrentPiece());
                }
            }
        }
    }
    
    private bool CanMove(Vector3 positionRequested)
    {
        List<Vector3> validMoves = GetAllValidMovesOfCurrentPiece();
        if (validMoves.Contains(positionRequested))
        {
            return true;
        }

        return false;
    }

    private void MovePiece(Vector3 positionRequested)
    {
        // Set our position as a fraction of the distance between the markers.
        _jumpPosition = positionRequested;
        _moveSpeed = Creator.playerSystemSo.moveSpeed;
        SetState(States.Moving);
                
        _audioSystem.PlayPieceMoveSfx(1);
            
        _validMovesSystem.HideAllValidMoves();
        
        Creator.playerFirstMoveMadeInLevel = true;
    }
}
