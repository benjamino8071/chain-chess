using System;
using System.Collections.Generic;
using Michsky.MUIP;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISections : UIPanel
{
    private TurnSystem _turnSystem;

    private Transform _sectionLockedParent;
    private Transform _sectionUnlockedParent;
    
    private TextMeshProUGUI _totalStarsScoredText;
    private TextMeshProUGUI _currentSectionText;
    
    private TextMeshProUGUI _starsRequiredToUnlockText;
    
    private class LevelButton
    {
        public ButtonManager levelButton;
        public Image starOneImage;
        public Image starTwoImage;
        public Image starThreeImage;
        public Level level;
    }
    private List<LevelButton> _levelButtons;

    private MMProgressBar _starsScoredProgressBar;
    
    private SectionData _currentSectionShown;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _levelButtons = new(10);
        
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        _sectionLockedParent =
            Creator.GetChildComponentByName<Transform>(_panel.gameObject, AllTagNames.SectionLockedParent);
        _sectionUnlockedParent =
            Creator.GetChildComponentByName<Transform>(_panel.gameObject, AllTagNames.SectionUnlockedParent);
        
        _totalStarsScoredText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.TextStars);
        
        _currentSectionText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.SectionText);

        _starsRequiredToUnlockText =
            Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StarsRequiredText);

        try
        {
            if (Creator.GetChildComponentByName<MMProgressBar>(_panel.gameObject, AllTagNames.ProgressBar) is
                { } progressBar)
            {
                _starsScoredProgressBar = progressBar;
            }
        }
        catch (NullReferenceException)
        {
            //Nothing to see here...
        }
        
        ButtonManager backButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonBack);
        backButton.onClick.AddListener(() =>
        {
            _uiSystem.ShowLeftBotSideUI(AllTagNames.UICurrentLevel);
            
            _audioSystem.PlayMenuOpenSfx();
        });

        ButtonManager leftButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonLeft);
        leftButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            int nextSection = _currentSectionShown.section == 1
                ? Creator.levelsSo.sections.Count
                : _currentSectionShown.section - 1;
            
            ShowSection(nextSection);
        });
        
        ButtonManager rightButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonRight);
        rightButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            int nextSection = _currentSectionShown.section == Creator.levelsSo.sections.Count
                ? 1
                : _currentSectionShown.section + 1;
            
            ShowSection(nextSection);
        });

        List<Transform> levelButtons = Creator.GetChildComponentsByName<Transform>(_panel.gameObject, AllTagNames.ButtonLevel);
        foreach (Transform levelButton in levelButtons)
        {
            LevelButton lb = new()
            {
                levelButton =
                    Creator.GetChildComponentByName<ButtonManager>(levelButton.gameObject, AllTagNames.ButtonPlay),
                starOneImage = Creator.GetChildComponentByName<Image>(levelButton.gameObject, AllTagNames.Star1Image),
                starTwoImage = Creator.GetChildComponentByName<Image>(levelButton.gameObject, AllTagNames.Star2Image),
                starThreeImage = Creator.GetChildComponentByName<Image>(levelButton.gameObject, AllTagNames.Star3Image),
            };
            _levelButtons.Add(lb);
            
            lb.levelButton.onClick.AddListener(() =>
            {
                _audioSystem.PlayUISignificantClickSfx();
                
                _turnSystem.LoadLevelRuntime(lb.level);
        
                _uiSystem.ShowLeftBotSideUI(AllTagNames.UICurrentLevel);
            });
        }
        
        SectionData sectionOnLoad = Creator.levelsSo.GetSection(Creator.saveDataSo.sectionLastLoaded);
        ShowSection(sectionOnLoad);
    }

    public override void Show()
    {
        ShowSection(_turnSystem.currentLevel.section);
        
        base.Show();
    }

    private void ShowSection(int section)
    {
        ShowSection(Creator.levelsSo.GetSection(section));
    }
    
    private void ShowSection(SectionData sectionData)
    {
        _currentSectionText.text = $"Section {sectionData.section}";
        
        _currentSectionShown = sectionData;
        
        int totalStarCount = 0;
        int sectionStarCount = 0;
        foreach (LevelSaveData levelSaveData in Creator.saveDataSo.levels)
        {
            totalStarCount += levelSaveData.starsScored;
            if (levelSaveData.section == sectionData.section)
            {
                sectionStarCount += levelSaveData.starsScored;
            }
        }

        if (totalStarCount < sectionData.starsRequiredToUnlock)
        {
            _sectionLockedParent.gameObject.SetActive(true);
            _sectionUnlockedParent.gameObject.SetActive(false);
            _starsScoredProgressBar?.gameObject.SetActive(false);
            
            _starsRequiredToUnlockText.text = $"<sprite index=0>{sectionData.starsRequiredToUnlock}";
            return;
        }
        
        _sectionLockedParent.gameObject.SetActive(false);
        _sectionUnlockedParent.gameObject.SetActive(true);
        _starsScoredProgressBar?.gameObject.SetActive(true);

        for (int i = 0; i < _levelButtons.Count; i++)
        {
            LevelButton levelInfo = _levelButtons[i];

            levelInfo.starOneImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starOneImage.color = Creator.levelCompleteSo.starHollowColor;
            
            levelInfo.starTwoImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starTwoImage.color = Creator.levelCompleteSo.starHollowColor;
            
            levelInfo.starThreeImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starThreeImage.color = Creator.levelCompleteSo.starHollowColor;
            
            if (i > sectionData.levels.Count - 1) //Allows sections to have different number of levels
            {
                levelInfo.levelButton.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                Level level = sectionData.levels[i];
                
                //Update star count
                foreach (LevelSaveData levelSaveData in Creator.saveDataSo.levels)
                {
                    if (levelSaveData.section != level.section || levelSaveData.level != level.level)
                    {
                        continue;
                    }
                    
                    bool oneStar = levelSaveData.score <= level.star1Score;
                    bool twoStar = levelSaveData.score <= level.star2Score;
                    bool threeStar = levelSaveData.score <= level.star3Score;
                    
                    levelInfo.starOneImage.sprite = oneStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
                    levelInfo.starOneImage.color = oneStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
        
                    levelInfo.starTwoImage.sprite = twoStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
                    levelInfo.starTwoImage.color = twoStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;

                    levelInfo.starThreeImage.sprite = threeStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
                    levelInfo.starThreeImage.color = threeStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
                    
                    break;
                }

                levelInfo.level = level;
                levelInfo.levelButton.SetText($"{level.section} - {level.level}");
                levelInfo.levelButton.transform.parent.gameObject.SetActive(true);
            }
        }
        
        float t = sectionStarCount / (3f * sectionData.levels.Count);
        _starsScoredProgressBar?.SetBar01(t);
        
        _totalStarsScoredText.text = $"<sprite index=0>{totalStarCount} / {Creator.levelsSo.totalStars}";
    }
}
