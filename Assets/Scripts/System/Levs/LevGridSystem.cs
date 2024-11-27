using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevGridSystem : LevDependency
{
    private LevDoorsSystem _levDoorsSystem;
    
    private Dictionary<Vector3, TileController> _validTiles = new();

    private Dictionary<Vector3, List<Vector3>> _connectedTiles = new();
    
    private Vector3 _highlightedPosition = Vector3.zero;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        if (levCreator.NewTryGetDependency(out LevDoorsSystem levDoorsSystem))
        {
            _levDoorsSystem = levDoorsSystem;
        }
        
        GameObject[] tilesAlreadyPlaced = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tileChild in tilesAlreadyPlaced)
        {
            TileController tileController = new TileController();
            tileController.SetTile(tileChild.transform, Creator);
                
            _validTiles.Add(tileChild.transform.position, tileController);
        }
        
        //Go through every position, and find all nearby positions. We do this by checking if a nearby position is valid
        foreach (Vector3 positionInGrid in _validTiles.Keys)
        {
            List<Vector3> nearbyPositions = new();
            
            //Check all positions around grid
            for (float x = positionInGrid.x - 1; x <= positionInGrid.x + 1; x++)
            {
                for (float y = positionInGrid.y - 1; y <= positionInGrid.y + 1; y++)
                {
                    Vector3 positionToCheck = new Vector3(x, y);
                    if (IsPositionValid(positionToCheck) && positionToCheck != positionInGrid)
                    {
                        nearbyPositions.Add(positionToCheck);
                    }
                }
            }
            
            _connectedTiles.Add(positionInGrid, nearbyPositions);
        }
    }

    public override void GameUpdate(float dt)
    {
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        Vector3 screenPos = Input.mousePosition;

        Camera main = Camera.main;
        if(main == null) return;
        
        Ray ray = main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hitData))
        {
            if (_validTiles.ContainsKey(hitData.transform.position))
            {
                _highlightedPosition = hitData.transform.position;
            }
            else if (_levDoorsSystem.IsDoorPosition(hitData.transform.position))
            {
                _highlightedPosition = hitData.transform.position;
            }
            else
            {
                _highlightedPosition = Vector3.zero;
            }
        }
        else
        {
            _highlightedPosition = Vector3.zero;
        }
    }

    public bool IsPositionValid(Vector2 tilePos)
    {
        return _validTiles.ContainsKey(tilePos);
    }

    public TileController GetTileController(Vector2 tilePos)
    {
        return _validTiles.GetValueOrDefault(tilePos);
    }

    public List<TileController> GetAllTileControllers()
    {
        return _validTiles.Values.ToList();
    }

    public Vector3 GetHighlightPosition()
    {
        return _highlightedPosition;
    }
    
    public List<Vector3> GetNearbyPositions(Vector3 position)
    {
        return _connectedTiles[position];
    }
}
