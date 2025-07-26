public class BlackSystem : SideSystem
{
    public override void GameStart(Creator creator)
    {
        _allyPieceColour = PieceColour.Black;
        _enemyPieceColour = PieceColour.White;
        _controlledBy = creator.blackControlledBy;
        _enemySideSystem = creator.GetDependency<WhiteSystem>();

        base.GameStart(creator);
    }
}
