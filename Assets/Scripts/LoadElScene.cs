using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LoadElScene : MonoBehaviour
{
    public Camera tempCam;

    public GameObject loadingScreen;

    public TextMeshProUGUI seedText;
    
    public PlayerSystem_SO playerSystemSo;
    public Timer_SO timerSo;
    public Enemy_SO enemySo;
    public Shop_SO shopSo;
    public Lives_SO livesSo;
    public XPBar_SO xpBarSo;
    public GridSystem_SO gridSystemSo;
    
    public bool resetCachedDataOnStart;
    
    private void Start()
    {
        playerSystemSo.firstMoveMadeWhileShowingMainMenu = false;
        
        tempCam.backgroundColor = Color.black;

        playerSystemSo.startingPiece = Piece.Queen;
        
        if (resetCachedDataOnStart)
        {
            playerSystemSo.ResetData();
            timerSo.ResetData();
            enemySo.ResetData();
            shopSo.ResetData();
            livesSo.ResetData();
            xpBarSo.ResetData();

            if (!gridSystemSo.useSeedInputOnNextLoad)
            {
                gridSystemSo.seed = DateTime.Now.Millisecond+(DateTime.Now.Day*DateTime.Now.Year);
            }
            else
            {
                gridSystemSo.useSeedInputOnNextLoad = false;
            }
        }

        seedText.text = "Seed: " + gridSystemSo.seed;
        
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
