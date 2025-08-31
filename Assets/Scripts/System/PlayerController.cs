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
        if (_pieceInstance.position != _jumpPosition)
        {
            _timer += dt * Creator.piecesSo.pieceSpeed;
            _timer = Mathf.Clamp(_timer, 0f, Mathf.PI);
            float t = Evaluate(_timer);
            _pieceInstance.position = Vector3.Lerp(_pieceInstance.position, _jumpPosition, t);
        }
                
        if (_pieceInstance.position == _jumpPosition)
        {
            Creator.statsMoves++;
            
            _pieceInstance.position = math.round(_pieceInstance.position);
            _timer = 0;
            
            if (_boardSystem.TryCaptureEnemyPiece(_pieceInstance.position, _enemyColour, this))
            {
                _movesInThisTurn.RemoveAt(0);
                
                //We win!
                UpdateSprite(_movesInThisTurn[0]);
            }
            else if(_enemySideSystem.TryGetCaptureLoverMovingToPosition(_pieceInstance.position, out PieceController captureLoverController))
            {
                _allySideSystem.FreezeSide();
                _enemySideSystem.SelectCaptureLoverPiece(captureLoverController, _pieceInstance.position);
            }
            else
            {
                _movesInThisTurn.RemoveAt(0);
                
                SetToNextMove();
                if (_movesInThisTurn.Count > 0 && _state == States.FindingMove)
                {
                    _validMovesSystem.UpdateValidMoves(GetAllValidMovesOfCurrentPiece());
                }
            }
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
            }
            else
            {
                //Blocked!
                SetState(States.Blocked);
            }
        }
        else
        {
            _chainUISystem.HideAllPieces();
            Finish();
        }
    }
}
