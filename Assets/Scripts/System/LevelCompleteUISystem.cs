using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteUISystem : Dependency
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;

    public bool IsShowing => _levelCompleteUI.gameObject.activeSelf;
    
    private Transform _levelCompleteUI;

    private Transform _thankYouMessage;
    
    private Button _nextLevelButton;

    private Image _starOneImage;
    private Image _starTwoImage;
    private Image _starThreeImage;
    
    private TextMeshProUGUI _turnsText;
    private TextMeshProUGUI _movesText;
    private TextMeshProUGUI _timeText;

    private float _delayTimer;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();

        _levelCompleteUI = creator.GetFirstObjectWithName(AllTagNames.LevelComplete).transform;

        _thankYouMessage = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.ThankYouMessage);
        
        Transform turnsText = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.StatsTurns);
        _turnsText = turnsText.GetComponent<TextMeshProUGUI>();
        
        Transform bestTurnText = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.StatsMoves);
        _movesText = bestTurnText.GetComponent<TextMeshProUGUI>();
        
        Transform timeText = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.StatsTime);
        _timeText = timeText.GetComponent<TextMeshProUGUI>();
        
        _starOneImage = creator.GetChildComponentByName<Image>(_levelCompleteUI.gameObject, AllTagNames.Star1Image);
        _starTwoImage = creator.GetChildComponentByName<Image>(_levelCompleteUI.gameObject, AllTagNames.Star2Image);
        _starThreeImage = creator.GetChildComponentByName<Image>(_levelCompleteUI.gameObject, AllTagNames.Star3Image);

        Transform retryButtonTf = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.Retry);
        Button retryButton = retryButtonTf.GetComponent<Button>();
        retryButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform nextLevelButtonTf = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.NextLevel);
        _nextLevelButton = nextLevelButtonTf.GetComponent<Button>();
        _nextLevelButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();

            Creator.levelsSo.levelOnLoad++;
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform exitButtonTf = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            SceneManager.LoadScene("Main_Menu_Scene");
        });
        
        Hide();
    }

    public override void GameUpdate(float dt)
    {
        if (_delayTimer > 0)
        {
            _delayTimer -= dt;
            if (_delayTimer <= 0)
            {
                _levelCompleteUI.gameObject.SetActive(true);
            }
        }
    }

    public void Show(float delayTimer = 0)
    {
        Level levelOnLoad = Creator.levelsSo.GetLevelOnLoad();
        _turnsText.text = $"{Creator.statsTurns} / {levelOnLoad.turns}";
        _movesText.text = $"{Creator.statsMoves}";
        _timeText.text = $"{Creator.statsTime:0.##}s";
        int score = (int)((Creator.statsTurns * 10) * Creator.statsMoves * Creator.statsTime);
        
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
        _thankYouMessage.gameObject.SetActive(Creator.levelsSo.levelOnLoad == Creator.levelsSo.AllLevels().Count);
        
        _levelCompleteUI.gameObject.SetActive(delayTimer == 0);
        _delayTimer = delayTimer;
        _audioSystem.PlayLevelCompleteSfx();
    }

    public void Hide()
    {
        _levelCompleteUI.gameObject.SetActive(false);
    }
}
