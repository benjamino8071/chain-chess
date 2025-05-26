using UnityEngine;

public class LevPlayerController : LevPieceController
{
    protected override void FindingMove(float dt)
    {
        //The player will find the move when they are ready
        Vector3 positionRequested = _boardSystem.GetHighlightPosition();
        if (Creator.inputSo._leftMouseButton.action.WasPerformedThisFrame() && TryMovePlayer(positionRequested))
        {
            _validMovesSystem.HideAllValidMoves();
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
            _pieceInstance.position = new Vector3(((int)_pieceInstance.position.x) + 0.5f, ((int)_pieceInstance.position.y) + 0.5f, 0);
            _sinTime = 0;

            _boardSystem.TryCaptureEnemyPiece(_pieceInstance.position, _enemyColour, this);
                    
            SetToNextMove();
            if (_movesInThisTurn.Count > 0)
            {
                _validMovesSystem.UpdateValidMoves(GetAllValidMovesOfCurrentPiece(), piecePos);
            }
        }
    }
}
