using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSeedGUI : MonoBehaviour
{
    public GameObject changeSeedCanvas;
    
    public MainMenu_SO mainMenuSo;
    public GridSystem_SO gridSystemSo;

    public TextMeshProUGUI currentInputText;
    
    private string _currentInput;

    private void Start()
    {
        Hide();
    }

    public void NumberInput(string number)
    {
        _currentInput += number;
        
        UpdateText(_currentInput);
    }

    public void EnterButtonPressed()
    {
        if (_currentInput.Length > 0)
        {
            gridSystemSo.seed = int.Parse(_currentInput);
            gridSystemSo.useSeedInputOnNextLoad = true;

            SceneManager.LoadScene(0);
        }
    }

    public void BackspaceButtonPressed()
    {
        if (_currentInput.Length > 0)
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
            
            UpdateText(_currentInput);
        }
    }

    private void UpdateText(string text)
    {
        currentInputText.text = text;
    }

    public void ToggleVisual()
    {
        if (changeSeedCanvas.gameObject.activeSelf)
        {
            Hide();
            mainMenuSo.isOtherMainMenuCanvasShowing = false;
        }
        else
        {
            mainMenuSo.isOtherMainMenuCanvasShowing = true;
            Show();
        }
    }
    
    public void Show()
    {
        _currentInput = "";
        UpdateText(". . . . . .");
        changeSeedCanvas.gameObject.SetActive(true);
    }

    public void Hide()
    {
        changeSeedCanvas.gameObject.SetActive(false);
    }
}
