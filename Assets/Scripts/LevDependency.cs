
public class LevDependency : Dependency
{
    protected LevCreator Creator;
    
    public virtual void GameStart(LevCreator levCreator)
    {
        Creator = levCreator;
    }
}
