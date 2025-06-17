using System.Collections.Generic;
using UnityEngine;

public class LevValidMovesSystem : LevDependency
{
    private List<Transform> _validMovesVisuals = new(64);

    private Transform _selectedBackgroundVisual;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _validMovesVisuals = new(64);

        Transform allValidMovesParent = Creator.GetFirstObjectWithName(AllTagNames.AllValidMovesParent);
        
        for (int i = 0; i < _validMovesVisuals.Capacity; i++)
        {
            GameObject validMove =
                Creator.InstantiateGameObjectWithParent(Creator.validPositionPrefab, allValidMovesParent);
            _validMovesVisuals.Add(validMove.transform);
        }

        GameObject selectedBackgroundVisual =
            Creator.InstantiateGameObject(Creator.selectedBackgroundPrefab, Vector3.zero, Quaternion.identity);
        _selectedBackgroundVisual = selectedBackgroundVisual.transform;
        
        HideAllValidMoves();
        HideSelectedBackground();
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
            _validMovesVisuals[i].position = validMoves[i];
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

    public void ShowSelectedBackground(Vector3 position)
    {
        _selectedBackgroundVisual.position = position;
        _selectedBackgroundVisual.gameObject.SetActive(true);
    }

    public void HideSelectedBackground()
    {
        _selectedBackgroundVisual.position = Vector3.zero;
        _selectedBackgroundVisual.gameObject.SetActive(false);
    }
}
