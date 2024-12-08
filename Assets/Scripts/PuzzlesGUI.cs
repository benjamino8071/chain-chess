using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzlesGUI : MonoBehaviour
{
    public GameObject canvasToShowOnGoBack;
    public GameObject puzzleCanvas;

    public MainMenu_SO mainMenuSo;
    
    private void Start()
    {
        Hide();
    }

    public void LoadPuzzle(int puzzleNum)
    {
        SceneManager.UnloadSceneAsync("EndlessScene");
        
        mainMenuSo.isPuzzleCanvasShowing = false;
        string sceneName = "Puzzle" + puzzleNum + "Scene";
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    public void Show()
    {
        mainMenuSo.isPuzzleCanvasShowing = true;
        canvasToShowOnGoBack.gameObject.SetActive(false);
        puzzleCanvas.gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        mainMenuSo.isPuzzleCanvasShowing = false;
        puzzleCanvas.gameObject.SetActive(false);
        canvasToShowOnGoBack.gameObject.SetActive(true);
    }
}
