using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevLevelCompleteUISystem : LevDependency
{
    private LevAudioSystem _audioSystem;
    private LevTurnSystem _turnSystem;

    public bool isShowing => _levelCompleteUI.gameObject.activeSelf;
    
    private Transform _levelCompleteUI;

    private Button _nextLevelButton;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();

        _levelCompleteUI = levCreator.GetFirstObjectWithName(AllTagNames.LevelComplete).transform;

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
        _nextLevelButton.gameObject.SetActive(Creator.levelsSo.levelOnLoad < Creator.levelsSo.levelsData.Count);
        
        _levelCompleteUI.gameObject.SetActive(true);
        _audioSystem.PlayLevelCompleteSfx();
    }

    public void Hide()
    {
        _levelCompleteUI.gameObject.SetActive(false);
    }
}
