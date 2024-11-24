using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSystem : Dependency
{
    private EnemiesSystem _enemiesSystem;
    
    private Dictionary<Vector3, TileController> _validTiles = new();

    private Dictionary<Vector3, List<Vector3>> _connectedTiles = new();

    //Key = position of a door, Value = position of the door directly opposite this door (to get to different part of map)
    private Dictionary<Vector3, SingleDoorPosition> _doorToDoorPositions = new();
    
    private Vector3 _currentTilePosition;

    private bool _inPuzzle;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        if (_creator.TryGetDependency("EnemiesSystem", out EnemiesSystem enemiesSystem))
        {
            _enemiesSystem = enemiesSystem;
        }
        
        Transform tileParent = GameObject.FindWithTag("TileParent").transform;
        
        //Go through tilemap
        GameObject tileMapTransform = GameObject.FindWithTag("Tilemap");
        Tilemap tileMap = tileMapTransform.GetComponent<Tilemap>();
        List<Vector3> _tileMapLocations = new();
        for (int x = tileMap.cellBounds.xMin; x < tileMap.cellBounds.xMax; x++)
        {
            for (int y = tileMap.cellBounds.yMin; y < tileMap.cellBounds.yMax; y++)
            {
                Vector3Int localLocation = new Vector3Int(
                    x: x,
                    y: y,
                    z: 0);
               
                Vector3 location = tileMap.CellToWorld(localLocation);
                if (tileMap.HasTile(localLocation))
                {
                    _tileMapLocations.Add(location);
                }
            }
        }

        foreach (GameObject doorPosition in GameObject.FindGameObjectsWithTag("DoorPosition"))
        {
            SingleDoorPosition singleDoorPosition = doorPosition.GetComponent<SingleDoorPosition>();
            _doorToDoorPositions.Add(doorPosition.transform.position, singleDoorPosition);
        }
        
        for (int x = 0; x < _creator.gridSystemSo.width; x++)
        {
            for (int y = 1; y < _creator.gridSystemSo.height; y++)
            {
                Vector3 position = new Vector3(x + 0.5f, y + 0.5f, 0);
                
                GameObject spawnedTile =
                    _creator.InstantiateGameObject(_creator.tilePrefab, position, Quaternion.identity);
                spawnedTile.transform.SetParent(tileParent, true);
                
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x + y) % 2 == 1;

                TileController tileController = new TileController();
                tileController.SetTile(spawnedTile.transform, isOffset, _creator);

                Vector3 fing = new Vector3(x, y, 0);
                if (!_tileMapLocations.Contains(fing))
                {
                    _validTiles.Add(spawnedTile.transform.position, tileController);
                }
            }
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
            _currentTilePosition = hitData.transform.position;
            foreach (TileController tileController in GetAllTileControllers())
            {
                tileController.ToggleHighlight(!_inPuzzle && tileController.GetPosition() == _currentTilePosition);
            }
        }
    }

    public bool IsPositionValid(Vector2 tilePos)
    {
        return _validTiles.ContainsKey(tilePos);
    }

    public bool IsDoorPosition(Vector3 position)
    {
        return _doorToDoorPositions.ContainsKey(position);
    }

    public bool TryGetSingleDoorPosition(Vector3 position, out SingleDoorPosition oppositeDoorPosition)
    {
        if (_doorToDoorPositions.TryGetValue(position, out SingleDoorPosition oppositeDoor))
        {
            oppositeDoorPosition = oppositeDoor;
            return true;
        }

        oppositeDoorPosition = default;
        return false;
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
        return _currentTilePosition;
    }

    public void SetInPuzzle(bool inPuzzle)
    {
        _inPuzzle = inPuzzle;
    }

    public List<Vector3> GetNearbyPositions(Vector3 position)
    {
        return _connectedTiles[position];
    }

    public List<Vector3> FindShortestPath(Vector3 start, Vector3 end)
    {
        if (!_connectedTiles.ContainsKey(start) || !_connectedTiles.ContainsKey(end))
        {
            Debug.LogError("Start or end position is not valid.");
            return null;
        }

        // Queue for BFS traversal
        Queue<Vector3> queue = new Queue<Vector3>();
        queue.Enqueue(start);

        // Dictionary to keep track of each position's predecessor in the path
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>
        {
            { start, start }
        };

        // Perform BFS
        while (queue.Count > 0)
        {
            Vector3 current = queue.Dequeue();

            // Check if we've reached the end
            if (current == end)
            {
                return ReconstructPath(cameFrom, start, end);
            }

            // Traverse each neighbor of the current position
            foreach (Vector3 neighbor in _connectedTiles[current])
            {
                // If the neighbor hasn't been visited yet
                if (!cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current; // Mark the path back to current
                }
            }
        }

        // If no path found, return null
        return null;
    }

    // Function to reconstruct path from the 'cameFrom' dictionary
    private List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 current = end;

        // Backtrack from the end to start
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Add(start);

        // Reverse to get path from start to end
        path.Reverse();
        return path;
    }
}
