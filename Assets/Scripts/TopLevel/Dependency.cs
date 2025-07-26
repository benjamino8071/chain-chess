
public class Dependency
{
    protected Creator Creator;
    
    public virtual void GameEarlyUpdate(float dt)
    {
        
    }
    
    public virtual void GameUpdate(float dt)
    {
        
    }

    public virtual void GameLateUpdate(float dt)
    {
        
    }

    public virtual void Clean()
    {
        
    }

    public virtual void Destroy()
    {
        
    }
    
    public virtual void GameStart(Creator creator)
    {
        Creator = creator;
    }
}
