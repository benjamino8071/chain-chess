using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
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
        
        _nextLevelButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonNextLevel);
        _nextLevelButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUISignificantClickSfx();
            
            Level currentLevel = _turnSystem.currentLevel;
            SectionData section = Creator.levelsSo.GetSection(currentLevel.section);
            int nextLevel = currentLevel.level == section.levels.Count ? 1 : currentLevel.level + 1;
            currentLevel = Creator.levelsSo.GetLevel(currentLevel.section, nextLevel);
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

        List<UISections> uiSections = _uiSystem.GetUI<UISections>();
        foreach (UISections uiSection in uiSections)
        {
            uiSection.UpdateStarCount();
        }
        
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
        
        _panel.gameObject.SetActive(true);

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
