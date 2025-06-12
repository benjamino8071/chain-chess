using System.Collections.Generic;
using UnityEngine;

public class LevWhiteSystem : LevSideSystem
{
    public override void GameStart(LevCreator levCreator)
    {
        allyPieceColour = PieceColour.White;
        enemyPieceColour = PieceColour.Black;
        controlledBy = levCreator.whiteControlledBy;

        base.GameStart(levCreator);
        
        _enemySideSystem = levCreator.GetDependency<LevBlackSystem>();
    }
}
