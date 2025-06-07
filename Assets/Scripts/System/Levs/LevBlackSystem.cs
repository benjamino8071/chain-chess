using System.Collections.Generic;
using UnityEngine;

public class LevBlackSystem : LevSideSystem
{
    public override void GameStart(LevCreator levCreator)
    {
        allySpName = AllTagNames.BlackSp;
        allyPieceColour = LevPieceController.PieceColour.Black;
        enemyPieceColour = LevPieceController.PieceColour.White;
        controlledBy = levCreator.blackControlledBy;

        base.GameStart(levCreator);
        
        _enemySideSystem = levCreator.GetDependency<LevWhiteSystem>();
    }
}
