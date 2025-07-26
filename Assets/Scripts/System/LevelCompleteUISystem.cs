using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteUISystem : Dependency
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;

    public bool isShowing => _levelCompleteUI.gameObject.activeSelf;
    
    private Transform _levelCompleteUI;

    private Transform _thankYouMessage;
    
    private Button _nextLevelButton;
    
    private TextMeshProUGUI _turnsText;
    private TextMeshProUGUI _bestTurnText;
    private TextMeshProUGUI _timeText;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();

        _levelCompleteUI = creator.GetFirstObjectWithName(AllTagNames.LevelComplete).transform;

        _thankYouMessage = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.ThankYouMessage);
        
        Transform turnsText = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.StatsTurns);
        _turnsText = turnsText.GetComponent<TextMeshProUGUI>();
        
        Transform bestTurnText = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.StatsBestTurn);
        _bestTurnText = bestTurnText.GetComponent<TextMeshProUGUI>();
        
        Transform timeText = creator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.StatsTime);
        _timeText = timeText.GetComponent<TextMeshProUGUI>();
        
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
    
    public void Show()
    {
        _turnsText.text = $"{Creator.statsTurns} / {Creator.levelsSo.GetLevelOnLoad().turns}";
        string plural = Creator.statsBestTurn == 1 ? "" : "s";
        _bestTurnText.text = $"{Creator.statsBestTurn} Capture{plural}";
        _timeText.text = $"{Creator.statsTime:0.##}s";
        
        _nextLevelButton.gameObject.SetActive(Creator.levelsSo.levelOnLoad < Creator.levelsSo.AllLevels().Count);
        _thankYouMessage.gameObject.SetActive(Creator.levelsSo.levelOnLoad == Creator.levelsSo.AllLevels().Count);
        
        _levelCompleteUI.gameObject.SetActive(true);
        _audioSystem.PlayLevelCompleteSfx();
    }

    public void Hide()
    {
        _levelCompleteUI.gameObject.SetActive(false);
    }
}
