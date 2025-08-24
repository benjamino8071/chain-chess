using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InvalidMovesSystem : Dependency
{
    private class InvalidMovePosition
    {
        public Transform visual;
        public int count;
    }
    
    private List<InvalidMovePosition> _invalidMovePositions;
    private int _freeMoveVisualPointer;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _invalidMovePositions = new(64);
        
        Transform allInvalidMovesParent = Creator.GetFirstObjectWithName(AllTagNames.AllInvalidMovesParent);
        
        for (int i = 0; i < _invalidMovePositions.Capacity; i++)
        {
            GameObject invalidMove =
                Creator.InstantiateGameObjectWithParent(Creator.invalidPositionPrefab, allInvalidMovesParent);
            InvalidMovePosition invalidMovePosition = new()
            {
                visual = invalidMove.transform,
                count = 0,
            };
            _invalidMovePositions.Add(invalidMovePosition);
        }
        
        HideAll();
    }

    public void DecrementPositions()
    {
        foreach (InvalidMovePosition invalidMovePosition in _invalidMovePositions)
        {
            if (invalidMovePosition.count > 0)
            {
                invalidMovePosition.count--;
                if (invalidMovePosition.count == 0)
                {
                    HidePosition(invalidMovePosition);
                }
            }
        }
    }
    
    public void AddMoves(List<Vector3> invalidMoves)
    {
        foreach (Vector3 move in invalidMoves)
        {
            bool found = false;
            foreach (InvalidMovePosition invalidMovePosition in _invalidMovePositions)
            {
                if (math.distance(invalidMovePosition.visual.position, move) < 0.1f)
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                continue;
            }
            
            _invalidMovePositions[_freeMoveVisualPointer].visual.position = move;
            _invalidMovePositions[_freeMoveVisualPointer].visual.gameObject.SetActive(true);
            _invalidMovePositions[_freeMoveVisualPointer].count = Creator.piecesSo.tileDestroyerMaxCount;
            
            _freeMoveVisualPointer++;
        }
    }

    public bool IsInvalidMove(Vector3 position)
    {
        foreach (InvalidMovePosition invalidMove in _invalidMovePositions)
        {
            if (math.distance(position, invalidMove.visual.position) < 0.1f)
            {
                return true;
            }
        }
        
        return false;
    }
    
    public void HideAll()
    {
        foreach (InvalidMovePosition invalidMovePosition in _invalidMovePositions)
        {
            HidePosition(invalidMovePosition);
        }

        _freeMoveVisualPointer = 0;
    }

    private void HidePosition(InvalidMovePosition invalidMovePosition)
    {
        invalidMovePosition.visual.gameObject.SetActive(false);
        invalidMovePosition.visual.position = Vector3.zero;
        invalidMovePosition.count = 0;
    }
}
