using System.Collections.Generic;
using UnityEngine;

public class LevBlackSystem : LevSideSystem
{
    public override void GameStart(LevCreator levCreator)
    {
        allyPieceColour = PieceColour.Black;
        enemyPieceColour = PieceColour.White;
        controlledBy = levCreator.blackControlledBy;

        base.GameStart(levCreator);
        
        _enemySideSystem = levCreator.GetDependency<LevWhiteSystem>();
    }
}
