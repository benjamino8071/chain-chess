using Michsky.MUIP;
using UnityEngine;

public class SettingsGUI : MonoBehaviour
{
    public GameObject settingsCanvas;

    public MainMenu_SO mainMenuSo;
    public Settings_SO settingsSo;

    public SwitchManager doubleTapSwitch;
    public SwitchManager soundSwitch;
    
    private void Start()
    {
        Hide();

        if (settingsSo.doubleTap)
        {
            doubleTapSwitch.SetOn();
        }
        else
        {
            doubleTapSwitch.SetOff();
        }
        doubleTapSwitch.onValueChanged.AddListener((isOn) =>
        {
            settingsSo.doubleTap = isOn;
        });

        if (settingsSo.sound)
        {
            soundSwitch.SetOn();
        }
        else
        {
            soundSwitch.SetOff();
        }
        soundSwitch.onValueChanged.AddListener((isOn) =>
        {
            settingsSo.sound = isOn;
        });
    }

    public void ToggleVisual()
    {
        if (settingsCanvas.gameObject.activeSelf)
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
        settingsCanvas.gameObject.SetActive(true);
    }

    public void Hide()
    {
        settingsCanvas.gameObject.SetActive(false);
    }
}
