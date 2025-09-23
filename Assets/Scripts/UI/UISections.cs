using System.Collections.Generic;
using Michsky.MUIP;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class UISections : UIPanel
{
    private class SectionButton
    {
        public int section;
        public int starsRequiredToUnlock;
        public bool unlocked;
        public ButtonManager button;
    }

    private List<SectionButton> _sectionButtons;
    
    private TMP_Text _starsScoredText;
    private MMProgressBar _starsScoredProgressBar;

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        _starsScoredText = Creator.GetChildComponentByName<TMP_Text>(_panel.gameObject, AllTagNames.TextStars);
        
        _starsScoredProgressBar =
            Creator.GetChildComponentByName<MMProgressBar>(_panel.gameObject, AllTagNames.ProgressBar);
        
        SectionButtons sectionButtonsDirect = _panel.GetComponent<SectionButtons>();
        _sectionButtons = new(sectionButtonsDirect.buttons.Count);
        foreach (Section section in sectionButtonsDirect.buttons)
        {
            SectionButton sectionButton = new()
            {
                button = section.button,
                section = section.section,
                starsRequiredToUnlock = Creator.levelsSo.GetSectionStarsRequirement(section.section),
                unlocked = false,
            };
            
            sectionButton.button.onClick.AddListener(() =>
            {
                if (sectionButton.unlocked)
                {
                    ShowSection(sectionButton.section);
                }
            });
            
            _sectionButtons.Add(sectionButton);
        }
    }

    private void ShowSection(int section)
    {
        UILevels uiLevels = _uiSystem.GetUI<UILevels>();
        uiLevels.SetLevels(section);
        
        _uiSystem.ShowRightSideUI(AllTagNames.UILevels);
    }

    public override void Show()
    {
        UpdateStarCount();
        
        base.Show();
    }

    public void UpdateStarCount()
    {
        float starCount = 0;
        foreach (LevelSaveData levelSaveData in Creator.saveDataSo.levels)
        {
            starCount += levelSaveData.starsScored;
        }
        
        foreach (SectionButton sectionButton in _sectionButtons)
        {
            if (starCount >= sectionButton.starsRequiredToUnlock)
            {
                sectionButton.button.enableIcon = false;
                sectionButton.button.enableText = true;
                
                sectionButton.button.normalText.color = Color.white;
                sectionButton.button.SetText($"{sectionButton.section}");
                
                sectionButton.unlocked = true;
            }
            else
            {
                sectionButton.button.enableIcon = true;
                sectionButton.button.enableText = true;
                sectionButton.button.normalText.color = Creator.levelCompleteSo.starFilledColor;
                sectionButton.button.SetText($"{sectionButton.starsRequiredToUnlock}");
            }
        }
        
        //Take away 3 because we don't want to include the very last level
        float t = starCount / (Creator.levelsSo.totalStars - 3);
        _starsScoredProgressBar.SetBar01(t);
        
        _starsScoredText.text = $"{(int)starCount}";
    }
}
