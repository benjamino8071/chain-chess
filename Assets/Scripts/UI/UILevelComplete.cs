using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILevelComplete : UIPanel
{
    private TurnSystem _turnSystem;
    
    private ButtonManager _nextLevelButton;

    private Image _starOneImage;
    private Image _starTwoImage;
    private Image _starThreeImage;

    private TextMeshProUGUI _starsRequiredText;

    private ParticleSystem _confetti;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        _confetti = Creator.GetFirstObjectWithName(AllTagNames.Confetti).GetComponent<ParticleSystem>();
        
        _starOneImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star1Image);
        _starTwoImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star2Image);
        _starThreeImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star3Image);

        _starsRequiredText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.TextStars);
        
        _nextLevelButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonNextLevel);
        _nextLevelButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUISignificantClickSfx();

            Level currentLevel = _turnSystem.currentLevel;
            SectionData sectionData = Creator.levelsSo.GetSection(currentLevel.section);
            SectionData nextSectionData = sectionData.section == Creator.levelsSo.sections.Count ? Creator.levelsSo.sections[0] : Creator.levelsSo.GetSection(sectionData.section + 1);

            int nextSection = currentLevel.isLastInSection ? nextSectionData.section : sectionData.section;
            int nextLevel = currentLevel.isLastInSection ? nextSectionData.levels[0].level : currentLevel.level + 1;
            currentLevel = Creator.levelsSo.GetLevel(nextSection, nextLevel);
            _turnSystem.LoadLevelRuntime(currentLevel);
        });

        ButtonManager resetButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonReset);
        resetButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();

            _turnSystem.ReloadCurrentLevel();
        });
    }

    public override void Show()
    {
        Level levelCompleted = _turnSystem.currentLevel;
        int score = Creator.statsTurns * Creator.statsMoves;
        
        if (_uiSystem.canvasType == _parentCanvas.canvasType)
        {
            Debug.Log($"Score for level {levelCompleted.level}: {score}");
        }
        
        bool oneStar = score <= levelCompleted.star1Score;
        bool twoStar = score <= levelCompleted.star2Score;
        bool threeStar = score <= levelCompleted.star3Score;
        
        _starOneImage.sprite = oneStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
        _starOneImage.color = oneStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
        
        _starTwoImage.sprite = twoStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
        _starTwoImage.color = twoStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;

        _starThreeImage.sprite = threeStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
        _starThreeImage.color = threeStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
        
        int emissionRate = 0;
        if (threeStar)
        {
            emissionRate = Creator.boardSo.threeStarConfettiEmissionRate;
        }
        else if (twoStar)
        {
            emissionRate = Creator.boardSo.twoStarConfettiEmissionRate;
        }
        else if (oneStar)
        {
            emissionRate = Creator.boardSo.oneStarConfettiEmissionRate;
        }

        var emission = _confetti.emission;
        emission.rateOverTime = emissionRate;
        
        _confetti.Play();
        
        if (_uiSystem.canvasType == _parentCanvas.canvasType)
        {
            SaveLatestLevelScore(levelCompleted, score);
            
            Creator.saveDataSo.totalTurns += Creator.statsTurns;
            Creator.saveDataSo.totalMoves += Creator.statsMoves;
            Creator.saveDataSo.totalCaptures += Creator.statsCaptures;
            
            if (oneStar || twoStar || threeStar)
            {
                _audioSystem.PlayLevelCompleteSfx();
                
                if (levelCompleted.section == Creator.levelsSo.sections.Count)
                {
                    _uiSystem.ShowThankYouForPlayingUI();
                }
            }
            else
            {
                _audioSystem.PlayerGameOverSfx();
            }
        }
        
        if (levelCompleted.isLastInSection)
        {
            //Have to check if the player's unlocked the next section
            //If they haven't then we need to show the text
            //If it's the very last level then just show a 'thank you' message
            //Also need to update the text
            
            if (levelCompleted.section == Creator.levelsSo.sections.Count)
            {
                //Very last level!
                _nextLevelButton.gameObject.SetActive(false);
                _starsRequiredText.text = "Thank You for Playing!";
                _starsRequiredText.gameObject.SetActive(true);
            }
            else
            {
                SectionData nextSectionData = Creator.levelsSo.GetSection(levelCompleted.section + 1);
                
                int starsScored = 0;
                foreach (LevelSaveData level in Creator.saveDataSo.levels)
                {
                    starsScored += level.starsScored;
                }

                bool nextSectionUnlocked = starsScored >= nextSectionData.starsRequiredToUnlock;
                int starsRequiredToUnlock = math.max(nextSectionData.starsRequiredToUnlock - starsScored, 0);
                _nextLevelButton.gameObject.SetActive(nextSectionUnlocked);
                _starsRequiredText.text = $"<b>{starsRequiredToUnlock}</b><sprite index=0> more required for Section {nextSectionData.section}";
                _starsRequiredText.gameObject.SetActive(!nextSectionUnlocked);
            }
        }
        else
        {
            _nextLevelButton.gameObject.SetActive(true);
            _starsRequiredText.gameObject.SetActive(false);
        }
        
        _panel.gameObject.SetActive(true);
        
        List<UICurrentLevel> uiCurrentLevels = _uiSystem.GetUI<UICurrentLevel>();
        foreach (UICurrentLevel uiCurrentLevel in uiCurrentLevels)
        {
            uiCurrentLevel.SetStarsScored(levelCompleted);
        }
    }
    
    private void SaveLatestLevelScore(Level level, int score)
    {
        bool found = false;
        int starsScored = 0;
        starsScored += score <= level.star1Score ? 1 : 0;
        starsScored += score <= level.star2Score ? 1 : 0;
        starsScored += score <= level.star3Score ? 1 : 0;
        
        for (int i = 0; i < Creator.saveDataSo.levels.Count; i++)
        {
            LevelSaveData levelSaveData = Creator.saveDataSo.levels[i];
            
            if (levelSaveData.section != level.section || levelSaveData.level != level.level)
            {
                continue;
            }

            if (score < levelSaveData.score)
            {
                levelSaveData.starsScored = starsScored;
                levelSaveData.score = score;

                Creator.saveDataSo.levels[i] = levelSaveData;
            }
            
            found = true;
            break;
        }

        if (!found)
        {
            Creator.saveDataSo.levels.Add(new()
            {
                section = level.section,
                level = level.level,
                score = score,
                starsScored = starsScored,
            });
        }
        
        Creator.SaveToDisk();
    }
}
