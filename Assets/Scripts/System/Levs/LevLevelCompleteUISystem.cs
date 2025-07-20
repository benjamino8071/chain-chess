using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevLevelCompleteUISystem : LevDependency
{
    private LevAudioSystem _audioSystem;
    private LevTurnSystem _turnSystem;

    public bool isShowing => _levelCompleteUI.gameObject.activeSelf;
    
    private Transform _levelCompleteUI;

    private Transform _thankYouMessage;
    
    private Button _nextLevelButton;
    
    private TextMeshProUGUI _turnsText;
    private TextMeshProUGUI _bestTurnText;
    private TextMeshProUGUI _timeText;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();

        _levelCompleteUI = levCreator.GetFirstObjectWithName(AllTagNames.LevelComplete).transform;

        _thankYouMessage = levCreator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.ThankYouMessage);
        
        Transform turnsText = levCreator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.StatsTurns);
        _turnsText = turnsText.GetComponent<TextMeshProUGUI>();
        
        Transform bestTurnText = levCreator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.StatsBestTurn);
        _bestTurnText = bestTurnText.GetComponent<TextMeshProUGUI>();
        
        Transform timeText = levCreator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.StatsTime);
        _timeText = timeText.GetComponent<TextMeshProUGUI>();
        
        Transform retryButtonTf = levCreator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.Retry);
        Button retryButton = retryButtonTf.GetComponent<Button>();
        retryButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform nextLevelButtonTf = levCreator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.NextLevel);
        _nextLevelButton = nextLevelButtonTf.GetComponent<Button>();
        _nextLevelButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();

            Creator.levelsSo.levelOnLoad++;
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform exitButtonTf = levCreator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            SceneManager.LoadScene("MainMenuScene");
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
