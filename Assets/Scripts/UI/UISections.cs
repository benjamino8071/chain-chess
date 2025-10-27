using System.Collections.Generic;
using Michsky.MUIP;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISections : UIPanel
{
    private class SectionButton
    {
        public int section;
        public int starsRequiredToUnlock;
        public bool unlocked;
        public ButtonManager button;
        public List<Image> buttonBackgroundImages;
    }

    private TurnSystem _turnSystem;
    
    private List<SectionButton> _sectionButtons;
    
    private TMP_Text _starsScoredText;
    private MMProgressBar _starsScoredProgressBar;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _starsScoredText = Creator.GetChildComponentByName<TMP_Text>(_panel.gameObject, AllTagNames.TextStars);
        
        _starsScoredProgressBar =
            Creator.GetChildComponentByName<MMProgressBar>(_panel.gameObject, AllTagNames.ProgressBar);

        ButtonManager backButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonBack);
        backButton.onClick.AddListener(() =>
        {
            _uiSystem.ShowLeftBotSideUI(AllTagNames.UICurrentLevel);
            
            _audioSystem.PlayMenuOpenSfx();
        });
        
        SectionButtons sectionButtonsDirect = _panel.GetComponent<SectionButtons>();
        _sectionButtons = new(sectionButtonsDirect.buttons.Count);
        foreach (Section section in sectionButtonsDirect.buttons)
        {
            SectionButton sectionButton = new()
            {
                button = section.button,
                buttonBackgroundImages = Creator.GetChildComponentsByName<Image>(section.button.gameObject, AllTagNames.BackgroundImage),
                section = section.section,
                starsRequiredToUnlock = Creator.levelsSo.GetSectionStarsRequirement(section.section),
                unlocked = false,
            };
            
            sectionButton.button.onClick.AddListener(() =>
            {
                _audioSystem.PlayUIAltClickSfx(.95f);
                
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
        Level level = Creator.levelsSo.GetLevel(section, 1);
        _turnSystem.LoadLevelRuntime(level);
        
        _uiSystem.ShowLeftBotSideUI(AllTagNames.UICurrentLevel);
    }

    public override void Show()
    {
        UpdateStarCount();
        
        base.Show();
    }

    public void UpdateStarCount()
    {
        int starCount = 0;
        foreach (LevelSaveData levelSaveData in Creator.saveDataSo.levels)
        {
            starCount += levelSaveData.starsScored;
        }
        
        foreach (SectionButton sectionButton in _sectionButtons)
        {
            int starCountInSection = 0;
            foreach (LevelSaveData levelSaveData in Creator.saveDataSo.levels)
            {
                if (levelSaveData.section != sectionButton.section)
                {
                    continue;
                }
                
                starCountInSection += levelSaveData.starsScored;
            }

            SectionData section = Creator.levelsSo.GetSection(sectionButton.section);
            int totalStarsInSection = 3 * section.levels.Count;

            SetButtonAppearance(sectionButton, starCount, starCountInSection, totalStarsInSection);
        }
        
        float t = starCount / (float)Creator.levelsSo.totalStars;
        _starsScoredProgressBar.SetBar01(t);
        _starsScoredText.text = $" {starCount} ({(int)(t*100)}%)";
    }

    private void SetButtonAppearance(SectionButton sectionButton, int starsScoredInGame, int currentStarCountInSection, int totalStarCountInSection)
    {
        if (currentStarCountInSection == totalStarCountInSection)
        {
            sectionButton.button.enableIcon = true;
            sectionButton.button.enableText = true;
                
            sectionButton.button.normalText.color = Color.white;
            sectionButton.button.SetIcon(Creator.miscUiSo.tickSprite);
            foreach (Image image in sectionButton.buttonBackgroundImages)
            {
                image.color = Creator.miscUiSo.sectionCompleteColour;
            }
            sectionButton.button.normalImage.color = Creator.miscUiSo.sectionCompleteColour;
            sectionButton.button.highlightImage.color = Color.black;
            sectionButton.button.SetText($"{sectionButton.section}");
                
            sectionButton.unlocked = true;
        }
        else if (starsScoredInGame >= sectionButton.starsRequiredToUnlock)
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
}
