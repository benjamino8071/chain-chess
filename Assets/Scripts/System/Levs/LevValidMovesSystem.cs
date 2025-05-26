using System.Collections.Generic;
using UnityEngine;

public class LevValidMovesSystem : LevDependency
{
    private List<Transform> _validPositionsVisuals;

    private Transform _selectedBackgroundVisual;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _validPositionsVisuals = new(64);
        
        for (int i = 0; i < _validPositionsVisuals.Capacity; i++)
        {
            GameObject validPos =
                Creator.InstantiateGameObject(Creator.validPositionPrefab, Vector3.zero, Quaternion.identity);
            _validPositionsVisuals.Add(validPos.transform);
        }

        GameObject selectedBackgroundVisual =
            Creator.InstantiateGameObject(Creator.selectedBackgroundPrefab, Vector3.zero, Quaternion.identity);
        _selectedBackgroundVisual = selectedBackgroundVisual.transform;
        
        HideAllValidMoves();
    }

    public void UpdateValidMoves(List<Vector3> validMoves, Vector3 piecePos)
    {
        HideAllValidMoves();
        
        for (int i = 0; i < validMoves.Count; i++)
        {
            _validPositionsVisuals[i].position = validMoves[i];
            _validPositionsVisuals[i].gameObject.SetActive(true);
        }

        _selectedBackgroundVisual.position = piecePos;
        _selectedBackgroundVisual.gameObject.SetActive(true);
    }

    public void UpdateSelectedBackground(Vector3 position)
    {
        HideAllValidMoves();

        _selectedBackgroundVisual.position = position;
        _selectedBackgroundVisual.gameObject.SetActive(true);
    }
    
    public void HideAllValidMoves()
    {
        foreach (Transform validPositionsVisual in _validPositionsVisuals)
        {
            validPositionsVisual.position = Vector3.zero;
            validPositionsVisual.gameObject.SetActive(false);
        }
        
        _selectedBackgroundVisual.position = Vector3.zero;
        _selectedBackgroundVisual.gameObject.SetActive(false);
    }
}
