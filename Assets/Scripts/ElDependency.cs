public class ElDependency : Dependency
{
    protected ElCreator Creator;
    
    public virtual void GameStart(ElCreator elCreator)
    {
        Creator = elCreator;
    }
}
