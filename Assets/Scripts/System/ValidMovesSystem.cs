using System.Collections.Generic;
using UnityEngine;

public class ValidMovesSystem : Dependency
{
    private List<Transform> _validMovesVisuals = new(64);
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _validMovesVisuals = new(64);

        Transform allValidMovesParent = Creator.GetFirstObjectWithName(AllTagNames.AllValidMovesParent);
        
        for (int i = 0; i < _validMovesVisuals.Capacity; i++)
        {
            GameObject validMove =
                Creator.InstantiateGameObjectWithParent(Creator.validPositionPrefab, allValidMovesParent);
            _validMovesVisuals.Add(validMove.transform);
        }
        
        HideAllValidMoves();
    }

    public void ShowSingleValidMove(Vector3 validMove)
    {
        HideAllValidMoves();

        _validMovesVisuals[0].position = validMove;
        _validMovesVisuals[0].gameObject.SetActive(true);
    }

    public void UpdateValidMoves(List<Vector3> validMoves)
    {
        HideAllValidMoves();
        
        for (int i = 0; i < validMoves.Count; i++)
        {
            _validMovesVisuals[i].position = validMoves[i] + new Vector3(0.5f,0.5f,0);
            _validMovesVisuals[i].gameObject.SetActive(true);
        }
    }
    
    public void HideAllValidMoves()
    {
        foreach (Transform validPositionsVisual in _validMovesVisuals)
        {
            validPositionsVisual.position = Vector3.zero;
            validPositionsVisual.gameObject.SetActive(false);
        }
    }
}
