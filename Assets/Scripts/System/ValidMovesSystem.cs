using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ValidMovesSystem : Dependency
{
    private class ValidPositionVisuals
    {
        public GameObject visual;
        
        public GameObject enemyHere;
        public GameObject enemyNotHere;
    }
    
    private List<ValidPositionVisuals> _validMovesVisuals = new(64);
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _validMovesVisuals = new(64);

        Transform allValidMovesParent = Creator.GetFirstObjectWithName(AllTagNames.AllValidMovesParent);
        
        for (int i = 0; i < _validMovesVisuals.Capacity; i++)
        {
            GameObject visual = Creator.InstantiateGameObjectWithParent(Creator.validPositionPrefab, allValidMovesParent);

            Transform enemyHere = Creator.GetChildObjectByName(visual, AllTagNames.EnemyHere);
            Transform enemyNotHere = Creator.GetChildObjectByName(visual, AllTagNames.EnemyNotHere);
            
            _validMovesVisuals.Add(new()
            {
                visual = visual,
                enemyHere = enemyHere.gameObject,
                enemyNotHere = enemyNotHere.gameObject
            });
        }
        
        HideAllValidMoves();
    }

    public void ShowSingleValidMove(Vector3 validMove)
    {
        //HideAllValidMoves();

        foreach (ValidPositionVisuals validPositionVisual in _validMovesVisuals)
        {
            if (validPositionVisual.visual.transform.position == validMove)
            {
                continue;
            }
            
            validPositionVisual.visual.transform.position = Vector3.zero;
            validPositionVisual.enemyHere.SetActive(false);
            validPositionVisual.enemyNotHere.SetActive(false);
        }
    }

    public void UpdateValidMoves(List<ValidMove> validMoves)
    {
        HideAllValidMoves();
        
        for (int i = 0; i < validMoves.Count; i++)
        {
            ValidMove valid = validMoves[i];
            
            _validMovesVisuals[i].visual.transform.position = valid.position;
            _validMovesVisuals[i].enemyHere.SetActive(valid.enemyHere);
            _validMovesVisuals[i].enemyNotHere.SetActive(!valid.enemyHere);
        }
    }
    
    public void HideAllValidMoves()
    {
        foreach (ValidPositionVisuals validPositionsVisual in _validMovesVisuals)
        {
            validPositionsVisual.visual.transform.position = Vector3.zero;
            validPositionsVisual.enemyHere.SetActive(false);
            validPositionsVisual.enemyNotHere.SetActive(false);
        }
    }
}

public struct ValidMove
{
    public float3 position;
    public bool enemyHere;
}
