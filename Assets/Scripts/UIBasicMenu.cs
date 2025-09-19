using Michsky.MUIP;
using UnityEngine;

public class UIBasicMenu : UIPanel
{
    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        ButtonManager powerButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonPower);

        ButtonManager rulebookButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonRulebook);

        ButtonManager settingsButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonSettings);
    }
}
