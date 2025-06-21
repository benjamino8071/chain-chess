using System.Collections.Generic;
using UnityEngine;

public class LevWhiteSystem : LevSideSystem
{
    public override void GameStart(LevCreator levCreator)
    {
        _allyPieceColour = PieceColour.White;
        _enemyPieceColour = PieceColour.Black;
        _controlledBy = levCreator.whiteControlledBy;

        base.GameStart(levCreator);
        
        _enemySideSystem = levCreator.GetDependency<LevBlackSystem>();
    }
}
