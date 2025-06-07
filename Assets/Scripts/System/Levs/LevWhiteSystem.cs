using System.Collections.Generic;
using UnityEngine;

public class LevWhiteSystem : LevSideSystem
{
    public override void GameStart(LevCreator levCreator)
    {
        allySpName = AllTagNames.WhiteSp;
        allyPieceColour = LevPieceController.PieceColour.White;
        enemyPieceColour = LevPieceController.PieceColour.Black;
        controlledBy = levCreator.whiteControlledBy;

        base.GameStart(levCreator);
        
        _enemySideSystem = levCreator.GetDependency<LevBlackSystem>();
    }
}
