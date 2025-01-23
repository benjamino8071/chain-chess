using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevLevelCompleteUISystem : LevDependency
{
    private Transform _levelCompleteUI;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _levelCompleteUI = levCreator.GetFirstObjectWithName(AllTagNames.LevelComplete).transform;

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
                    break;
                case "Exit":
                    button.onClick.AddListener(() =>
                    {
                        SceneManager.LoadScene("MainMenuScene");
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
