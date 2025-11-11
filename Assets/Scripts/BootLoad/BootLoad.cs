using System;
using System.Linq;
using System.Threading.Tasks;
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
        try
        {
            await SceneManager.UnloadSceneAsync(0);
        
            SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
        }
        catch (Exception)
        {
            Application.Quit();
        }
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
}
