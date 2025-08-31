using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIController : PieceController
{
    private float _thinkingTimer;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
    }

    public override void Init(Vector3 position, List<Piece> startingPieces, PieceColour pieceColour, PieceAbility pieceAbility,
        ControlledBy controlledBy, SideSystem allySideSystem, SideSystem enemySideSystem)
    {
        base.Init(position, startingPieces, pieceColour, pieceAbility, controlledBy, allySideSystem, enemySideSystem);
        
        SetThinkingTime();
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

            _startPosition = _pieceInstance.position;
            if (_pieceAbility == PieceAbility.LeaveBehind)
            {
                _allySideSystem.CreatePiece(_pieceInstance.position, new(){_currentPiece}, _pieceAbility);
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
                if (_pieceAbility == PieceAbility.TileDestroyer)
                {
                    //Find out the x tiles between start position and end position

                    Vector2Int startPos = new((int)_startPosition.x, (int)_startPosition.y);
                    Vector2Int jumpPos = new((int)_jumpPosition.x, (int)_jumpPosition.y);

                    List<Vector2Int> intermediaryPositions = GetPath(startPos, jumpPos);
                    
                    List<Vector3> tilesCrossed = new(1 + intermediaryPositions.Count)
                    {
                        _startPosition
                    };
                    foreach (Vector2Int intermediaryPosition in intermediaryPositions)
                    {
                        Vector3 pos = new(intermediaryPosition.x, intermediaryPosition.y);
                        
                        tilesCrossed.Add(pos);
                    }
                    
                    _invalidMovesSystem.AddMoves(tilesCrossed);
                }
                
                SetToNextMove();
                SetThinkingTime();
            }
        }
    }
    
    private List<Vector2Int> GetPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        int dx = end.x - start.x;
        int dy = end.y - start.y;

        // Determine step direction
        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

        // Make sure it's a valid straight or diagonal move
        if (dx != 0 && dy != 0 && Mathf.Abs(dx) != Mathf.Abs(dy))
        {
            Debug.LogWarning("Invalid chess move: not straight or diagonal");
            return path;
        }

        Vector2Int current = start + new Vector2Int(stepX, stepY);

        while (current != end)
        {
            path.Add(current);
            current += new Vector2Int(stepX, stepY);
        }

        return path;
    }

    public override void ForceMove(float3 movePosition)
    {
        SetThinkingTime();
        base.ForceMove(movePosition);
    }

    private void SetThinkingTime()
    {
        _thinkingTimer = _pieceAbility == PieceAbility.AlwaysMove 
            ? Creator.piecesSo.alwaysMoveThinkingTime 
            : Creator.piecesSo.aiThinkingTime;
    }
}
