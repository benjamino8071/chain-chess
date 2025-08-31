using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoad : MonoBehaviour
{
    public Camera tempCam;

    public GameObject loadingScreen;
    
    public Animator topTweenOutAnim;

    public Board_SO boardSo;
    public Levels_SO levelsSo;
    
    private static readonly int Play = Animator.StringToHash("play");

    public float animationTime;
    
    private float _unloadSceneTimer;

    private void Start()
    {
        levelsSo.levelOnLoad = 1;

        boardSo.hideMainMenuTrigger = false;
        
        tempCam.backgroundColor = Color.black;
        
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
        SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
        
        SceneManager.LoadSceneAsync("Game_Scene", LoadSceneMode.Additive);
    }

    private void Update()
    {
        if (_unloadSceneTimer > 0)
        {
            _unloadSceneTimer += Time.deltaTime;
            //Animations take one second to complete
            if (_unloadSceneTimer >= animationTime)
            {
                SceneManager.UnloadSceneAsync("Main_Menu_Scene");
            }
        }
        else if (boardSo.hideMainMenuTrigger)
        {
            topTweenOutAnim.SetTrigger(Play);
            _unloadSceneTimer += Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
        SceneManager.sceneUnloaded -= SceneManagerOnsceneUnloaded;
    }

    private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if (scene.name == "Game_Scene")
        {
            loadingScreen.gameObject.SetActive(false);
            tempCam.gameObject.SetActive(false);
            SceneManager.SetActiveScene(scene);
        }
    }
    
    private void SceneManagerOnsceneUnloaded(Scene scene)
    {
        if (scene.name == "Game_Scene")
        {
            tempCam.gameObject.SetActive(true);
        }
    }
}
