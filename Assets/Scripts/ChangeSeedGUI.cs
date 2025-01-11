using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSeedGUI : MonoBehaviour
{
    public GameObject canvasToShowOnGoBack;
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
    
    public void Show()
    {
        _currentInput = "";
        UpdateText(". . .");
        mainMenuSo.isOtherMainMenuCanvasShowing = true;
        canvasToShowOnGoBack.gameObject.SetActive(false);
        changeSeedCanvas.gameObject.SetActive(true);
    }

    public void Hide()
    {
        mainMenuSo.isOtherMainMenuCanvasShowing = false;
        changeSeedCanvas.gameObject.SetActive(false);
        canvasToShowOnGoBack.gameObject.SetActive(true);
    }
}
