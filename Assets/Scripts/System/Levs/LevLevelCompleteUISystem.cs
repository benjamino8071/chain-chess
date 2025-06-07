using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevLevelCompleteUISystem : LevDependency
{
    private LevAudioSystem _audioSystem;
    
    public bool isShowing => _levelCompleteUI.gameObject.activeSelf;
    
    private Transform _levelCompleteUI;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        
        _levelCompleteUI = levCreator.GetFirstObjectWithName(AllTagNames.LevelComplete).transform;

        Transform retryButtonTf = levCreator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.Retry);
        Button retryButton = retryButtonTf.GetComponent<Button>();
        retryButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        
        Transform nextLevelButtonTf = levCreator.GetChildObjectByName(_levelCompleteUI.gameObject, AllTagNames.NextLevel);
        Button nextLevelButton = nextLevelButtonTf.GetComponent<Button>();
        nextLevelButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            if (Creator.nextLevelNumber >= 1)
            {
                string nextSceneName = "Puzzle" + Creator.nextLevelNumber + "Scene";
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");
            }
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
        _levelCompleteUI.gameObject.SetActive(true);
        _audioSystem.PlayLevelCompleteSfx();
    }

    public void Hide()
    {
        _levelCompleteUI.gameObject.SetActive(false);
    }
}
