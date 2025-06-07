using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LoadScene : MonoBehaviour
{
    public Camera tempCam;

    public GameObject loadingScreen;
    
    public Animator topTweenOutAnim;
    
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

    private float _unloadSceneTimer;

    private void Start()
    {
        playerSystemSo.hideMainMenuTrigger = false;
        
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
        
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
        SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
        
        SceneManager.LoadSceneAsync("Testing_Scene", LoadSceneMode.Additive);
    }

    private void Update()
    {
        if (_unloadSceneTimer > 0)
        {
            _unloadSceneTimer += Time.deltaTime;
            //Animations take one second to complete
            if (_unloadSceneTimer >= 1)
            {
                SceneManager.UnloadSceneAsync("MainMenuScene");
            }
        }
        else if (playerSystemSo.hideMainMenuTrigger)
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
        if (scene.name == "Testing_Scene")
        {
            loadingScreen.gameObject.SetActive(false);
            tempCam.gameObject.SetActive(false);
            SceneManager.SetActiveScene(scene);
        }
    }
    
    private void SceneManagerOnsceneUnloaded(Scene scene)
    {
        if (scene.name == "Testing_Scene")
        {
            tempCam.gameObject.SetActive(true);
        }
    }
}
