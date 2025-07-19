using System.Collections.Generic;
using UnityEngine;

public class LevBlackSystem : LevSideSystem
{
    public override void GameStart(LevCreator levCreator)
    {
        _allyPieceColour = PieceColour.Black;
        _enemyPieceColour = PieceColour.White;
        _controlledBy = levCreator.blackControlledBy;
        _enemySideSystem = levCreator.GetDependency<LevWhiteSystem>();

        base.GameStart(levCreator);
    }
}
