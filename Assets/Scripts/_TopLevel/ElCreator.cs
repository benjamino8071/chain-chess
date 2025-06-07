using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class ElCreator : Creator
{
    [Header("SOs")]
    public Input_SO inputSo;
    public PlayerSystem_SO playerSystemSo;
    public Enemy_SO enemySo;
    public MainMenu_SO mainMenuSo;
    public Shop_SO shopSo;
    public Chain_SO chainSo;
    public AudioClips_SO audioClipsSo;
    public Lives_SO livesSo;
    public XPBar_SO upgradeSo;
    public GameData_SO gameDataSo;
    public Settings_SO settingsSo;
    
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
        
        Random.InitState(gridSystemSo.seed);
        randomGenerator = new System.Random(gridSystemSo.seed);
        
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

        if (playerSystemSo.hideMainMenuTrigger)
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
        foreach (Dependency dependency in _dependencies)
        {
            ElDependency elDependency = (ElDependency)dependency;
            elDependency.GameEnd();
        }
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
        _dependencies.Add(new ElChainUISystem());
        _dependencies.Add(new ElCinemachineSystem());
        _dependencies.Add(new ElDoorsSystem());
        _dependencies.Add(new ElGridSystem());
        _dependencies.Add(new ElEnemiesSystem());
        _dependencies.Add(new ElUpgradeUISystem());
        _dependencies.Add(new ElRunInfoUISystem());
        _dependencies.Add(new ElSettingsUISystem());
        _dependencies.Add(new ElPlayerSystem());
        _dependencies.Add(new ElXPBarUISystem());
        _dependencies.Add(new ElGameOverUISystem());
        _dependencies.Add(new ElPlayerWonUISystem());
        _dependencies.Add(new ElTimerUISystem());
        _dependencies.Add(new ElLivesUISystem());
        _dependencies.Add(new ElRoomNumberUISystem());
        _dependencies.Add(new ElArtefactsUISystem());
        _dependencies.Add(new ElShopSystem());
        _dependencies.Add(new ElFreeUpgradeSystem());
        _dependencies.Add(new ElTurnSystem());
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
