using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElGameOverUISystem : ElDependency
{
    private ElRunInfoUISystem _runInfoUISystem;
    private ElArtefactsUISystem _artefactsUISystem;
    
    private Transform _gameOverUI;

    private Button _exitButton;

    private TextMeshProUGUI _titleText;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _runInfoUISystem = elCreator.GetDependency<ElRunInfoUISystem>();
        _artefactsUISystem = elCreator.GetDependency<ElArtefactsUISystem>();

        _gameOverUI = elCreator.GetFirstObjectWithName(AllTagNames.GameOver);

        Transform titleText = elCreator.GetFirstObjectWithName(AllTagNames.PlayerCapturedText);
        _titleText = titleText.GetComponent<TextMeshProUGUI>();

        Transform exitButtonTf = elCreator.GetChildObjectByName(_gameOverUI.gameObject, AllTagNames.Exit);

        _exitButton = exitButtonTf.GetComponent<Button>();
        _exitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenuScene");
        });
        Hide();
    }

    public void Show(string message)
    {
        _titleText.text = message;
        
        _runInfoUISystem.Hide();
        _artefactsUISystem.Hide();
        
        _gameOverUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _gameOverUI.gameObject.SetActive(false);
    }
}
