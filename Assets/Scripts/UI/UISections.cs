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

        SectionButtons sectionButtons = _panel.GetComponent<SectionButtons>();
        foreach (Section section in sectionButtons.buttons)
        {
            int starsRequirement = Creator.levelsSo.GetSectionStarsRequirement(section.section);
            section.starsAmountText.text = $"x {starsRequirement}";
            
            section.button.onClick.AddListener(() =>
            {
                ShowSection(section.section);
            });
        }
        
        Hide();
    }

    private void ShowSection(int section)
    {
        UILevels uiLevels = _uiSystem.GetUI<UILevels>();
        uiLevels.SetLevels(section);
        
        _uiSystem.ShowRightSideUI(AllTagNames.UILevels);
    }
}
