using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILevelComplete : UIPanel
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;
    
    private Transform _endOfLevelsMessage;
    
    private ButtonManager _nextLevelButton;

    private Image _starOneImage;
    private Image _starTwoImage;
    private Image _starThreeImage;
    
    private TextMeshProUGUI _turnsText;
    private TextMeshProUGUI _movesText;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _endOfLevelsMessage = Creator.GetChildObjectByName(_panel.gameObject, AllTagNames.EndOfLevelsMessage);
        
        _turnsText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsTurns);
        _movesText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.StatsMoves);
        
        _starOneImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star1Image);
        _starTwoImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star2Image);
        _starThreeImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Star3Image);
        
        _nextLevelButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonNextLevel);
        _nextLevelButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();

            Creator.levelsSo.levelOnLoad++;
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Hide();
    }

    public override void Show()
    {
        Level levelOnLoad = Creator.levelsSo.GetLevelOnLoad();
        _turnsText.text = $"{Creator.statsTurns}";
        _movesText.text = $"{Creator.statsMoves}";
        int score = Creator.statsTurns * Creator.statsMoves;
        
        Debug.Log($"Score for level {levelOnLoad.level}: {score}");
        
        bool oneStar = score <= levelOnLoad.star1Score;
        bool twoStar = score <= levelOnLoad.star2Score;
        bool threeStar = score <= levelOnLoad.star3Score;
        
        _starOneImage.sprite = oneStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
        _starOneImage.color = oneStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
        
        _starTwoImage.sprite = twoStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
        _starTwoImage.color = twoStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;

        _starThreeImage.sprite = threeStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
        _starThreeImage.color = threeStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;

        _nextLevelButton.gameObject.SetActive(Creator.levelsSo.levelOnLoad < Creator.levelsSo.AllLevels().Count);
        _endOfLevelsMessage.gameObject.SetActive(Creator.levelsSo.levelOnLoad == Creator.levelsSo.AllLevels().Count);
        
        _panel.gameObject.SetActive(true);
        _audioSystem.PlayLevelCompleteSfx();
    }
}
