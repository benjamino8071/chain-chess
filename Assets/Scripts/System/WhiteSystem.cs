using System.Collections.Generic;
using UnityEngine;

public class WhiteSystem : SideSystem
{
    public override void GameStart(Creator creator)
    {
        _allyPieceColour = PieceColour.White;
        _enemyPieceColour = PieceColour.Black;
        _controlledBy = creator.whiteControlledBy;
        _enemySideSystem = creator.GetDependency<BlackSystem>();
        
        base.GameStart(creator);
    }
}
