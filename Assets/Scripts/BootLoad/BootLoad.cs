using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootLoad : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private RectTransform transitionImage;
    
    [SerializeField] private float maxTransitionTime;

    [SerializeField] private SaveData_SO saveDataSo;
    [SerializeField] private Settings_SO settingsSo;
    [SerializeField] private Board_SO boardSo;
    
    private float _timer;

    private enum State
    {
        None,
        Transitioning,
        Loading
    }
    private State _state;

    private void Awake()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            LoadScreenSize();
        }
    }

    private void Start()
    {
        _state = State.Transitioning;
        _timer = 0;
     
        SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
        SceneManager.sceneLoaded += SceneManager_SceneLoaded;
    }
    
    private void SceneManager_SceneLoaded(Scene sceneLoaded, LoadSceneMode arg1)
    {
        if (sceneLoaded.buildIndex == 1)
        {
            SceneManager.SetActiveScene(sceneLoaded);
            Unload();
        }
    }

    private async void Unload()
    {
        await SceneManager.UnloadSceneAsync(0);
        
        SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Transitioning:
            {
                _timer += Time.deltaTime;
            
                float t = math.min(1, _timer / maxTransitionTime);
                float width = math.lerp(0, canvas.rect.width, t);
            
                transitionImage.sizeDelta = new Vector2(width, transitionImage.sizeDelta.y);

                if (t >= 1)
                {
                    _state = State.Loading;
                }
                break;
            }
            case State.Loading:
            {
                SceneManager.LoadScene(1, LoadSceneMode.Additive);
                
                _state = State.None;
                break;
            }
        }
    }
    
    private void LoadScreenSize()
    {
        if (ES3.KeyExists(settingsSo.saveDataKey))
        {
            SaveData_SO diskSaveDataSo = ES3.Load(settingsSo.saveDataKey, saveDataSo);
            Screen.SetResolution(diskSaveDataSo.windowWidth, diskSaveDataSo.windowHeight, diskSaveDataSo.isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            Screen.fullScreen = diskSaveDataSo.isFullscreen;
        }
        else
        {
            Screen.SetResolution(settingsSo.defaultWidth, settingsSo.defaultHeight, FullScreenMode.Windowed);
            Screen.fullScreen = false;
        }
    }
}
