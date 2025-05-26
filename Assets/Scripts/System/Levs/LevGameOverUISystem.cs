using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevGameOverUISystem : LevDependency
{
    public bool isShowing => _gameOverUI.gameObject.activeSelf;
    
    private Transform _gameOverUI;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _gameOverUI = levCreator.GetFirstObjectWithName(AllTagNames.GameOver).transform;
        
        Transform resetButtonTf = levCreator.GetChildObjectByName(_gameOverUI.gameObject, AllTagNames.Retry);
        Button resetButton = resetButtonTf.GetComponent<Button>();
        resetButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        
        Transform exitButtonTf = levCreator.GetChildObjectByName(_gameOverUI.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenuScene");
        });

        Hide();
    }

    public void Show()
    {
        _gameOverUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
