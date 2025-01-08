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
    public MainMenu_SO mainMenuSo;
    public Scoreboard_SO scoreboardSo;
    public Shop_SO shopSo;
    public Chain_SO chainSo;
    public AudioClips_SO audioClipsSo;
    public Lives_SO livesSo;
    public XPBar_SO xpBarSo;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject tilePrefab;
    public GameObject validPositionPrefab;
    public GameObject capturedPieceImagePrefab;
    public GameObject arrowPointingToNextPiecePrefab;
    public GameObject shopItemPrefab;
    public GameObject enemyValidPositionsPrefab;

    [Header("GUITopBottom")] 
    public GameObject guiTop;
    public GameObject guiBottom;

    [Header("Fake Grid")]
    public GameObject fakeGrid;
    public Sprite doorClosedSprite;
    
    private SpriteRenderer fakeDoorOne;
    private SpriteRenderer fakeDoorTwo;
    
    public System.Random randomGenerator;
    
    private void Start()
    {
        SceneManager.sceneUnloaded += SceneManager_SceneUnloaded;
        
        randomGenerator = new System.Random(DateTime.Now.Millisecond);
        Random.InitState(DateTime.Now.Millisecond);

        if (enemySo.cachedSpawnPoints.Count == 0)
        { 
            shopSo.shopRoomNumber = randomGenerator.Next(2, 8);
        }
        
        Camera.main.backgroundColor = Color.black;

        if (playerSystemSo.levelNumberSaved == 0)
        {
            fakeGrid.SetActive(false);
        }
        else
        {
            SpriteRenderer[] spriteRenderers = fakeGrid.GetComponentsInChildren<SpriteRenderer>();
            if (spriteRenderers.Length == 2)
            {
                fakeDoorOne = spriteRenderers[0];
                fakeDoorTwo = spriteRenderers[1];
            }
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
        _dependencies.AddLast(new ElAudioSystem());
        _dependencies.AddLast(new ElTurnSystem());
        _dependencies.AddLast(new ElChainUISystem());
        _dependencies.AddLast(new ElCinemachineSystem());
        _dependencies.AddLast(new ElDoorsSystem());
        _dependencies.AddLast(new ElGridSystem());
        _dependencies.AddLast(new ElEnemiesSystem());
        _dependencies.AddLast(new ElPauseUISystem());
        _dependencies.AddLast(new ElPlayerSystem());
        _dependencies.AddLast(new ElXPBarUISystem());
        _dependencies.AddLast(new ElGameOverUISystem());
        _dependencies.AddLast(new ElScoreEntryUISystem());
        _dependencies.AddLast(new ElTimerUISystem());
        _dependencies.AddLast(new ElLivesUISystem());
        _dependencies.AddLast(new ElRoomNumberUISystem());
        _dependencies.AddLast(new ElArtefactsUISystem());
        _dependencies.AddLast(new ElShopSystem());
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

    public void CloseFakeDoors()
    {
        fakeDoorOne.sprite = doorClosedSprite;
        fakeDoorTwo.sprite = doorClosedSprite;
    }
}
