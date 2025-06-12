using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevBoardSystem : LevDependency
{
    private LevWhiteSystem _whiteSystem;
    private LevBlackSystem _blackSystem;
    private LevChainUISystem _chainUISystem;
    
    private Dictionary<Vector3, TileController> _validTiles = new();

    private Dictionary<Vector3, List<Vector3>> _connectedTiles = new();
    
    private Vector3 _highlightedPosition = Vector3.zero;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _whiteSystem = levCreator.GetDependency<LevWhiteSystem>();
        _blackSystem = levCreator.GetDependency<LevBlackSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        
        List<Transform> tilesAlreadyPlaced = levCreator.GetObjectsByName(AllTagNames.Tile);
        foreach (Transform tileChild in tilesAlreadyPlaced)
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
            _highlightedPosition = _validTiles.ContainsKey(hitData.transform.position) ? hitData.transform.position : Vector3.zero;
        }
        else
        {
            _highlightedPosition = Vector3.zero;
        }
    }

    public bool IsPositionValid(Vector3 tilePos)
    {
        return _validTiles.ContainsKey(tilePos);
    }

    public bool IsAllyAtPosition(Vector3 piecePos, PieceColour allyColour)
    {
        List<Vector3> enemyPositions = allyColour == PieceColour.White 
            ? _whiteSystem.PiecePositions() 
            : _blackSystem.PiecePositions();

        return enemyPositions.Contains(piecePos);
    }

    public bool IsEnemyAtPosition(Vector3 piecePos, PieceColour enemyColour)
    {
        List<Vector3> enemyPositions = enemyColour == PieceColour.White 
            ? _whiteSystem.PiecePositions() 
            : _blackSystem.PiecePositions();

        return enemyPositions.Contains(piecePos);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="piecePos"></param>
    /// <param name="enemyColour"></param>
    /// <param name="pieceUsed"></param>
    /// <returns>True = captured all enemy pieces of pieceUsed</returns>
    public bool TryCaptureEnemyPiece(Vector3 piecePos, PieceColour enemyColour, LevPieceController pieceUsed)
    {
        if (enemyColour == PieceColour.White 
            && _whiteSystem.TryGetAllyPieceAtPosition(piecePos, out LevPieceController whitePieceCont))
        {
            _chainUISystem.ShowNewPiece(whitePieceCont.capturedPieces[0], pieceUsed.movesUsed);
            pieceUsed.AddCapturedPiece(whitePieceCont.capturedPieces[0]);
            return _whiteSystem.PieceCaptured(whitePieceCont, pieceUsed.piecesCapturedInThisTurn);
        }
        if (enemyColour == PieceColour.Black 
            && _blackSystem.TryGetAllyPieceAtPosition(piecePos, out LevPieceController blackPieceCont))
        {
            _chainUISystem.ShowNewPiece(blackPieceCont.capturedPieces[0], pieceUsed.movesUsed);
            pieceUsed.AddCapturedPiece(blackPieceCont.capturedPieces[0]);
            return _blackSystem.PieceCaptured(blackPieceCont, pieceUsed.piecesCapturedInThisTurn);
        }

        return false;
    }

    public Vector3 GetHighlightPosition()
    {
        return _highlightedPosition;
    }

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        return Camera.main.ScreenToViewportPoint(screenPos);
    }
}
