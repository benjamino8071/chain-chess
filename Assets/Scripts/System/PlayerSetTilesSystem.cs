using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerSetTilesSystem : Dependency
{
    private BoardSystem _boardSystem;

    private Dictionary<Vector2, Transform> _tiles;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _boardSystem = creator.GetDependency<BoardSystem>();

        int capacity = 64;
        _tiles = new(capacity);
        
        Transform playerSetTilesParent = Creator.GetFirstObjectWithName(AllTagNames.PlayerSetTilesParent);

        for (int x = 1; x < 9; x++)
        {
            for (int y = 1; y < 9; y++)
            {
                GameObject invalidMove = Creator.InstantiateGameObjectWithParent(Creator.playerSetTilePrefab, playerSetTilesParent);
                invalidMove.transform.position = new(x, y, 0);
                _tiles[new(x, y)] = invalidMove.transform;
                invalidMove.gameObject.SetActive(false);
            }
        }
    }

    public override void GameUpdate(float dt)
    {
        Vector3 positionRequested = _boardSystem.GetGridPointNearMouse();
        if (Creator.inputSo.rightMouseButton.action.WasPerformedThisFrame() && _boardSystem.IsPositionValid(positionRequested))
        {
            ToggleTile(new Vector2(positionRequested.x, positionRequested.y));
        }
        else if (Creator.inputSo.leftMouseButton.action.WasPerformedThisFrame())
        {
            HideAll();
        }
    }

    private void ToggleTile(Vector2 position)
    {
        if (_tiles.TryGetValue(position, out Transform tile))
        {
            tile.gameObject.SetActive(!tile.gameObject.activeSelf);
        }
    }

    public void HideAll()
    {
        foreach (Transform tile in _tiles.Values)
        {
            tile.gameObject.SetActive(false);
        }
    }
}
