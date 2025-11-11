using Michsky.MUIP;

public class UIConfirmDelete : UIPanel
{
    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        ButtonManager confirmButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonDelete);
        confirmButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Creator.DeleteOnDisk();
        });
        
        ButtonManager cancelButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonCancel);
        cancelButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();

            _uiSystem.ShowLeftBotSideUI(AllTagNames.UISettings);
        });
    }
}
