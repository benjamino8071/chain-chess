using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ElCreator : Creator
{
    [Header("SOs")]
    public Input_SO inputSo;
    public PlayerSystem_SO playerSystemSo;
    public Enemy_SO enemySo;
    public Timer_SO timerSo;
    public MainMenu_SO mainMenuSo;
    public Scoreboard_SO scoreboardSo;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject tilePrefab;
    public GameObject validPositionPrefab;
    public GameObject capturedPieceImagePrefab;
    public GameObject arrowPointingToNextPiecePrefab;

    [Header("Starting Piece")]
    public Piece startingPiece;

    [Header("Start timer on load")] 
    public bool startTimerOnLoad;

    [Header("GUITopBottom")] 
    public GameObject guiTop;
    public GameObject guiBottom;

    public GameObject fakeGrid;

    public System.Random randomGenerator;
    
    private void Start()
    {
        SceneManager.sceneUnloaded += SceneManager_SceneUnloaded;
        
        randomGenerator = new System.Random(DateTime.Now.Millisecond);
        Random.InitState(DateTime.Now.Millisecond);
        
        Camera.main.backgroundColor = Color.black;

        if (playerSystemSo.levelNumberSaved == 0)
        {
            fakeGrid.SetActive(false);
        }
        
        foreach (Dependency dependency in _dependencies)
        {
            ElDependency elDependency = (ElDependency)dependency;
            elDependency.GameStart(this);
        }

        if (playerSystemSo.firstMoveMadeWhileShowingMainMenu)
        {
            ShowGUITop();
            ShowGUIBottom();
        }
        else
        {
            HideGUITop();
            HideGUIBottom();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= SceneManager_SceneUnloaded;
    }

    private void SceneManager_SceneUnloaded(Scene sceneUnloaded)
    {
        if (sceneUnloaded.name == "MainMenuScene")
        {
            ShowGUITop();
            ShowGUIBottom();
        }
    }

    public override void CreateDependencies()
    {
        _dependencies.Add(new ElAudioSystem());
        _dependencies.Add(new ElTurnSystem());
        _dependencies.Add(new ElChainUISystem());
        _dependencies.Add(new ElCinemachineSystem());
        _dependencies.Add(new ElDoorsSystem());
        _dependencies.Add(new ElGridSystem());
        _dependencies.Add(new ElEnemiesSystem());
        _dependencies.Add(new ElPauseUISystem());
        _dependencies.Add(new ElPlayerSystem());
        _dependencies.Add(new ElGameOverUISystem());
        _dependencies.Add(new ElScoreEntryUISystem());
        _dependencies.Add(new ElTimerUISystem());
        _dependencies.Add(new ElRoomNumberUISystem());
        
        //_dependencies.Add(new ElStartingPieceUISystem());
    }

    public void ShowGUITop()
    {
        guiTop.SetActive(true);
    }
    
    public void HideGUITop()
    {
        guiTop.SetActive(false);
    }

    public void ShowGUIBottom()
    {
        guiBottom.SetActive(true);
    }

    public void HideGUIBottom()
    {
        guiBottom.SetActive(false);
    }
}
