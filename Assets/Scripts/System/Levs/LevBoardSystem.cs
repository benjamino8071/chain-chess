using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevBoardSystem : LevDependency
{
    private LevWhiteSystem _whiteSystem;
    private LevBlackSystem _blackSystem;
    
    private Dictionary<Vector3, TileController> _validTiles = new();

    private Dictionary<Vector3, List<Vector3>> _connectedTiles = new();
    
    private Vector3 _highlightedPosition = Vector3.zero;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _whiteSystem = levCreator.GetDependency<LevWhiteSystem>();
        _blackSystem = levCreator.GetDependency<LevBlackSystem>();

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

    public bool IsAllyAtPosition(Vector3 piecePos, LevPieceController.PieceColour allyColour)
    {
        List<Vector3> enemyPositions = allyColour == LevPieceController.PieceColour.White 
            ? _whiteSystem.PiecePositions() 
            : _blackSystem.PiecePositions();

        return enemyPositions.Contains(piecePos);
    }

    public bool IsEnemyAtPosition(Vector3 piecePos, LevPieceController.PieceColour enemyColour)
    {
        List<Vector3> enemyPositions = enemyColour == LevPieceController.PieceColour.White 
            ? _whiteSystem.PiecePositions() 
            : _blackSystem.PiecePositions();

        return enemyPositions.Contains(piecePos);
    }

    public bool TryCaptureEnemyPiece(Vector3 piecePos, LevPieceController.PieceColour enemyColour, LevPieceController pieceUsed)
    {
        if (enemyColour == LevPieceController.PieceColour.White 
            && _whiteSystem.TryGetPieceAtPosition(piecePos, out LevPieceController whitePieceCont))
        {
            _whiteSystem.PieceCaptured(whitePieceCont);
            foreach (Piece enemyPiece in whitePieceCont.capturedPieces)
            {
                pieceUsed.AddCapturedPiece(enemyPiece);
            }
            return true;
        }
        if (enemyColour == LevPieceController.PieceColour.Black 
            && _blackSystem.TryGetPieceAtPosition(piecePos, out LevPieceController blackPieceCont))
        {
            _blackSystem.PieceCaptured(blackPieceCont);
            foreach (Piece enemyPiece in blackPieceCont.capturedPieces)
            {
                pieceUsed.AddCapturedPiece(enemyPiece);
            }
            return true;
        }

        return false;
    }

    public Vector3 GetHighlightPosition()
    {
        return _highlightedPosition;
    }
}
