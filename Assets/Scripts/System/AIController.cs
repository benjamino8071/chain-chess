using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIController : PieceController
{
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _thinkingTimer = Creator.piecesSo.aiThinkingTime;
    }

    protected override void FindingMove(float dt)
    {
        _thinkingTimer -= dt;
        if (_thinkingTimer > 0 || _movesInThisTurn.Count == 0)
        {
            return;
        }

        List<Vector3> possiblePositions = GetAllValidMovesOfCurrentPiece();
                
        if (possiblePositions.Count > 0)
        {
            //Go through each position and see if the player is at that position. If they are, capture it!
            bool foundPlayer = false;
            foreach (Vector3 possiblePosition in possiblePositions)
            {
                if (_boardSystem.IsEnemyAtPosition(possiblePosition, _enemyColour))
                {
                    _jumpPosition = possiblePosition;
                    foundPlayer = true;
                    break;
                }
            }
            if (!foundPlayer)
            {
                int chosenPositionIndex = Random.Range(0, possiblePositions.Count);
                _jumpPosition = possiblePositions[chosenPositionIndex];
            }
            
            _audioSystem.PlayPieceMoveSfx(1);
            SetState(States.Moving);
        }
        else
        {
            SetToNextMove();
        }
    }

    protected override void Moving(float dt)
    {
        _thinkingTimer -= dt;
        if (_thinkingTimer > 0 || _movesInThisTurn.Count == 0)
        {
            return;
        }
        
        if (_pieceInstance.position != _jumpPosition)
        {
            _timer += dt * Creator.piecesSo.pieceSpeed;
            _timer = Mathf.Clamp(_timer, 0f, Mathf.PI);
            float t = Evaluate(_timer);
            _pieceInstance.position = Vector3.Lerp(_pieceInstance.position, _jumpPosition, t);
        }
                
        if (_pieceInstance.position == _jumpPosition)
        {
            _pieceInstance.position = math.round(_pieceInstance.position);
            _timer = 0;
            
            _movesInThisTurn.RemoveAt(0);
            if (_boardSystem.TryCaptureEnemyPiece(_pieceInstance.position, _enemyColour, this))
            {
                //We win!
                UpdateSprite(_movesInThisTurn[0]);
            }
            else
            {
                SetToNextMove();
                _thinkingTimer = Creator.piecesSo.aiThinkingTime;
            }
        }
    }
}
