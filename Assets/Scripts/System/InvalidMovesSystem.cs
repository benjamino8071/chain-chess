using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InvalidMovesSystem : Dependency
{
    private List<Transform> _invalidMovesVisuals = new(64);
    private int _freeMoveVisualPointer;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _invalidMovesVisuals = new(64);

        Transform allInvalidMovesParent = Creator.GetFirstObjectWithName(AllTagNames.AllInvalidMovesParent);
        
        for (int i = 0; i < _invalidMovesVisuals.Capacity; i++)
        {
            GameObject invalidMove =
                Creator.InstantiateGameObjectWithParent(Creator.invalidPositionPrefab, allInvalidMovesParent);
            _invalidMovesVisuals.Add(invalidMove.transform);
        }
        
        HideAll();
    }
    
    public void AddMoves(List<Vector3> invalidMoves)
    {
        foreach (Vector3 move in invalidMoves)
        {
            _invalidMovesVisuals[_freeMoveVisualPointer].position = move;
            _invalidMovesVisuals[_freeMoveVisualPointer].gameObject.SetActive(true);

            _freeMoveVisualPointer++;
        }
    }

    public bool IsInvalidMove(Vector3 position)
    {
        foreach (Transform invalidMove in _invalidMovesVisuals)
        {
            if (math.distance(position, invalidMove.position) < 0.1f)
            {
                return true;
            }
        }
        
        return false;
    }
    
    public void HideAll()
    {
        foreach (Transform validPositionsVisual in _invalidMovesVisuals)
        {
            validPositionsVisual.position = Vector3.zero;
            validPositionsVisual.gameObject.SetActive(false);
        }

        _freeMoveVisualPointer = 0;
    }
}
