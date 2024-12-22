using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadElScene : MonoBehaviour
{
    public Camera tempCam;

    public GameObject loadingScreen;

    public PlayerSystem_SO playerSystemSo;
    public Timer_SO timerSo;
    public Enemy_SO enemySo;
    public Shop_SO shopSo;
    
    public bool resetCachedDataOnStart;
    
    private void Start()
    {
        playerSystemSo.firstMoveMadeWhileShowingMainMenu = false;
        
        tempCam.backgroundColor = Color.black;

        if (resetCachedDataOnStart)
        {
            playerSystemSo.ResetData();
            timerSo.ResetData();
            enemySo.ResetData();
            shopSo.ResetData();
        }
        
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
        SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
        
        SceneManager.LoadSceneAsync("EndlessScene", LoadSceneMode.Additive);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
        SceneManager.sceneUnloaded -= SceneManagerOnsceneUnloaded;
    }

    private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if (scene.name == "EndlessScene")
        {
            loadingScreen.gameObject.SetActive(false);
            tempCam.gameObject.SetActive(false);
            SceneManager.SetActiveScene(scene);
        }
    }
    
    private void SceneManagerOnsceneUnloaded(Scene scene)
    {
        if (scene.name == "EndlessScene")
        {
            tempCam.gameObject.SetActive(true);
        }
    }
}
