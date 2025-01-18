using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LoadElScene : MonoBehaviour
{
    public Camera tempCam;

    public GameObject loadingScreen;

    public TextMeshProUGUI seedText;

    public Animator topTweenOutAnim;
    public Animator bottomTweenOutAnim;

    public List<GameObject> mainMenuButtons;
    
    public PlayerSystem_SO playerSystemSo;
    public Timer_SO timerSo;
    public Enemy_SO enemySo;
    public Shop_SO shopSo;
    public Lives_SO livesSo;
    public XPBar_SO xpBarSo;
    public GridSystem_SO gridSystemSo;
    public GameData_SO gameDataSo;
    
    public bool resetCachedDataOnStart;
    private static readonly int Play = Animator.StringToHash("play");

    private float unloadSceneTimer;

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
            gameDataSo.ResetData();

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

    private void Update()
    {
        if (unloadSceneTimer > 0)
        {
            unloadSceneTimer += Time.deltaTime;
            //Animations take one second to complete
            if (unloadSceneTimer >= 1)
            {
                SceneManager.UnloadSceneAsync("MainMenuScene");
            }
        }
        else if (playerSystemSo.firstMoveMadeWhileShowingMainMenu)
        {
            topTweenOutAnim.SetTrigger(Play);
            bottomTweenOutAnim.SetTrigger(Play);
            unloadSceneTimer += Time.deltaTime;
            foreach (GameObject button in mainMenuButtons)
            {
                button.SetActive(false);
            }
        }
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
