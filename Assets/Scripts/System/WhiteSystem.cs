using System.Collections.Generic;
using UnityEngine;

public class WhiteSystem : SideSystem
{
    public override void GameStart(Creator creator)
    {
        _allyPieceColour = PieceColour.White;
        _enemyPieceColour = PieceColour.Black;
        _enemySideSystem = creator.GetDependency<BlackSystem>();
        
        base.GameStart(creator);
    }

    public override void CreatePiece(Vector2 position, Piece startingPiece, PieceAbility ability)
    {
        PieceController pieceController = new PlayerController();
        pieceController.GameStart(Creator);
        pieceController.Init(position, startingPiece, _allyPieceColour, ability, this, _enemySideSystem);
        
        _pieceControllers.Add(pieceController);
    }

    public override void GameUpdate(float dt)
    {
        if (_frozen || _turnSystem.CurrentTurn() == _enemyPieceColour)
        {
            return;
        }
        
        _pieceControllerSelected?.GameUpdate(dt);
    }

    public override void SpawnPieces()
    {
        base.SpawnPieces();

        SelectPiece(_pieceControllers[0]);
    }
}
