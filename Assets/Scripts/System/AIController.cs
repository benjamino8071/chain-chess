using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIController : PieceController
{
    private TurnSystem _turnSystem;
    
    private float _thinkingTimer;
    
    public override void GameStart(Creator creator)
    {
        _turnSystem = creator.GetDependency<TurnSystem>();
        
        base.GameStart(creator);
    }

    public override void Init(Vector3 position, Piece startingPiece, PieceColour pieceColour, 
        PieceAbility pieceAbility, SideSystem allySideSystem, SideSystem enemySideSystem)
    {
        base.Init(position, startingPiece, pieceColour, pieceAbility, allySideSystem, enemySideSystem);
        
        SetThinkingTime();
    }

    protected override void FindingMove(float dt)
    {
        _thinkingTimer -= dt;
        if (_thinkingTimer > 0 || _movesInThisTurn.Count == 0)
        {
            return;
        }

        List<ValidMove> possiblePositions = GetAllValidMovesOfCurrentPiece();
                
        if (possiblePositions.Count > 0)
        {
            //Go through each position and see if the player is at that position. If they are, capture it!
            bool foundPlayer = false;
            foreach (ValidMove possiblePosition in possiblePositions)
            {
                if (_boardSystem.IsEnemyAtPosition(possiblePosition.position, _enemyColour))
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

            _startPosition = _pieceInstance.position;
            
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

        if (_pieceInstance.position == _jumpPosition)
        {
            _pieceInstance.position = math.round(_pieceInstance.position);
            _timer = 0;
            
            bool won = false;
            PieceController enemyPieceController = _enemySideSystem.GetPieceAtPosition(_pieceInstance.position);
            if (enemyPieceController is not null)
            {
                won = _enemySideSystem.PieceCaptured(enemyPieceController);
            }
            
            _movesInThisTurn.RemoveAt(0);

            if (won)
            {
                _enemySideSystem.Lose(GameOverReason.Captured, 0);
            }
            else
            {
                SetToNextMove();
                SetThinkingTime();
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

    protected override void SetToNextMove()
    {
        if (_movesInThisTurn.Count > 0)
        {
            //Allow player to make another move
            UpdateSprite(_movesInThisTurn[0]);
            
            List<ValidMove> validMoves = GetAllValidMovesOfCurrentPiece();
            SetState(validMoves.Count > 0 ? States.FindingMove : States.Blocked);
        }
        else if (_pieceAbility == PieceAbility.AlwaysMove && _turnSystem.CurrentTurn() == _enemyColour)
        {
            SetState(States.WaitingForTurn);
            _validMovesSystem.UpdateValidMovesOfWhitePiece();
            _enemySideSystem.UnfreezeSide();
        }
        else
        {
            Finish();
        }
    }

    private void SetThinkingTime()
    {
        _thinkingTimer = _pieceAbility == PieceAbility.AlwaysMove 
            ? Creator.piecesSo.alwaysMoveThinkingTime 
            : Creator.piecesSo.aiThinkingTime;
    }
}
