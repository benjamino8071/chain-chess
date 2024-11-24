using UnityEngine;

public class Dependency
{
    protected Creator _creator;
    
    public virtual void GameStart(Creator creator)
    {
        _creator = creator;
    }
    
    public virtual void GameEarlyUpdate(float dt)
    {
        
    }
    
    public virtual void GameUpdate(float dt)
    {
        
    }

    public virtual void GameLateUpdate(float dt)
    {
        
    }

    public virtual void GameFixedUpdate(float dt)
    {
        
    }
}
