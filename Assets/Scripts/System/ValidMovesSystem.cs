using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ValidMovesSystem : Dependency
{
    private class ValidPositionVisual
    {
        public GameObject enemyHere;
        public GameObject enemyNotHere;
    }

    private Dictionary<Vector2, ValidPositionVisual> validPositions;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        validPositions = new(64);

        Transform allValidMovesParent = Creator.GetFirstObjectWithName(AllTagNames.AllValidMovesParent);
        
        for (int x = 1; x < 9; x++)
        {
            for (int y = 1; y < 9; y++)
            {
                GameObject visual = Creator.InstantiateGameObjectWithParent(Creator.validPositionPrefab, allValidMovesParent);

                Transform enemyHere = Creator.GetChildObjectByName(visual, AllTagNames.EnemyHere);
                Transform enemyNotHere = Creator.GetChildObjectByName(visual, AllTagNames.EnemyNotHere);
                
                visual.transform.position = new(x, y, 0);
                enemyHere.gameObject.SetActive(false);
                enemyNotHere.gameObject.SetActive(false);
                
                validPositions[new Vector2(x, y)] = new()
                {
                    enemyHere = enemyHere.gameObject,
                    enemyNotHere = enemyNotHere.gameObject
                };
            }
        }
    }

    public void UpdateValidMoves(List<ValidMove> validMoves)
    {
        //Debug.Log("Updating valid moves");
        
        HideAllValidMoves();
        
        for (int i = 0; i < validMoves.Count; i++)
        {
            ValidMove valid = validMoves[i];
            
            if (validPositions.TryGetValue(new(valid.position.x, valid.position.y), out ValidPositionVisual validPosition))
            {
                validPosition.enemyHere.SetActive(valid.enemyHere);
                validPosition.enemyNotHere.SetActive(!valid.enemyHere);
            }
        }
    }
    
    public void HideAllValidMoves()
    {
        foreach (ValidPositionVisual validPositionVisual in validPositions.Values)
        {
            validPositionVisual.enemyHere.SetActive(false);
            validPositionVisual.enemyNotHere.SetActive(false);
        }
    }
}

public struct ValidMove
{
    public float3 position;
    public bool enemyHere;
}
