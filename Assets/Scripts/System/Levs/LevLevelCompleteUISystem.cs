using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevLevelCompleteUISystem : LevDependency
{
    private Transform _levelCompleteUI;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _levelCompleteUI = GameObject.FindWithTag("LevelComplete").transform;

        Button[] buttons = _levelCompleteUI.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            switch (button.tag)
            {
                case "Retry":
                    button.onClick.AddListener(() =>
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    });
                    break;
                case "NextLevel":
                    button.onClick.AddListener(() =>
                    {
                        string nextSceneName = "TutorialLevel" + Creator.nextLevelNumber + "Scene";
                        SceneManager.LoadScene(nextSceneName);
                    });
                    break;
                case "Exit":
                    //TODO: Add 'exit to main menu' action
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log("ADD A MAIN MENU");
                    });
                    break;
            }
        }
        
        Hide();
    }

    public void Show()
    {
        _levelCompleteUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _levelCompleteUI.gameObject.SetActive(false);
    }
}
