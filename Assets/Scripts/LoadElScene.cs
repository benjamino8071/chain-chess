using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadElScene : MonoBehaviour
{
    public Camera tempCam;

    public GameObject loadingScreen;

    public PlayerSystem_SO playerSystemSo;
    
    private void Start()
    {
        playerSystemSo.firstMoveMadeWhileShowingMainMenu = false;
        
        tempCam.backgroundColor = Color.black;
        
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
