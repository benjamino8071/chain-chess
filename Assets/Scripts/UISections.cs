using UnityEngine;

public class UISections : UIPanel
{
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        Hide();
    }
}
