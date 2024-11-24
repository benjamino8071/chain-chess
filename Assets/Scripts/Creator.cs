using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;

public class Creator : MonoBehaviour
{
    [Header("SOs")]
    public Input_SO inputSo;
    public GridSystem_SO gridSystemSo;
    public PlayerSystem_SO playerSystemSo;
    public Enemy_SO enemySo;
    public Timer_SO timerSo;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject tilePrefab;
    public GameObject validPositionPrefab;
    public GameObject capturedPieceImagePrefab;
    public GameObject arrowPointingToNextPiecePrefab;

    private Dictionary<string, Dependency> _dependencies = new();
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        
        CreateDependencies();
    }

    private void Start()
    { 
        Random.InitState(42);
        
        Camera.main.backgroundColor = Color.black;
        
        foreach (Dependency dependency in _dependencies.Values)
        {
            dependency.GameStart(this);
        }
    }

    private void Update()
    {
        foreach (Dependency dependency in _dependencies.Values)
        {
            dependency.GameEarlyUpdate(Time.deltaTime);
        }
        
        foreach (Dependency dependency in _dependencies.Values)
        {
            dependency.GameUpdate(Time.deltaTime);
        }
        
        foreach (Dependency dependency in _dependencies.Values)
        {
            dependency.GameLateUpdate(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        foreach (Dependency dependency in _dependencies.Values)
        {
            dependency.GameFixedUpdate(Time.fixedDeltaTime);
        }
    }

    private void CreateDependencies()
    {
        _dependencies.Add("GridSystem", new GridSystem());
        _dependencies.Add("DoorsSystem", new DoorsSystem());
        _dependencies.Add("PlayerSystem", new PlayerSystem());
        _dependencies.Add("EnemiesSystem", new EnemiesSystem());
        _dependencies.Add("CinemachineSystem", new CinemachineSystem());
        _dependencies.Add("AudioSystem", new AudioSystem());
        _dependencies.Add("CapturedPiecesUISystem", new CapturedPiecesUISystem());
        _dependencies.Add("TurnInfoUISystem", new TurnInfoUISystem());
        _dependencies.Add("TimerUISystem", new TimerUISystem());
        _dependencies.Add("PauseUISystem", new PauseUISystem());
        _dependencies.Add("GameOverUISystem", new GameOverUISystem());
        _dependencies.Add("EndGameUISystem", new EndGameUISystem());
        _dependencies.Add("ChooseStartingPieceUISystem", new ChooseStartingPieceUISystem());
    }

    public GameObject InstantiateGameObject(GameObject creatorGridPrefab, Vector3 position, Quaternion rotation)
    {
        return Instantiate(creatorGridPrefab, position, rotation);
    }

    public bool TryGetDependency<T>(string className, out T dependency) where T : Dependency
    {
        if (_dependencies.TryGetValue(className, out Dependency foundDependency))
        {
            if (foundDependency is T typedDependency)
            {
                dependency = typedDependency;
                return true;
            }
        }

        dependency = default;
        return false;
    }

    public void StartACoRoutine(IEnumerator corout)
    {
        StartCoroutine(corout);
    }
}
