using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevGameOverUISystem : LevDependency
{
    private LevAudioSystem _audioSystem;
    private LevTurnSystem _turnSystem;

    public bool isShowing => _gameOverUI.gameObject.activeSelf;
    
    private Transform _gameOverUI;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();

        _gameOverUI = levCreator.GetFirstObjectWithName(AllTagNames.GameOver).transform;
        
        Transform resetButtonTf = levCreator.GetChildObjectByName(_gameOverUI.gameObject, AllTagNames.Retry);
        Button resetButton = resetButtonTf.GetComponent<Button>();
        resetButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform exitButtonTf = levCreator.GetChildObjectByName(_gameOverUI.gameObject, AllTagNames.Exit);
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
        _gameOverUI.gameObject.SetActive(true);
        _audioSystem.PlayerGameOverSfx();
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
