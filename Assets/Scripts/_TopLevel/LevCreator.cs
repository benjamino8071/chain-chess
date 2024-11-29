using UnityEngine;

public class LevCreator : Creator
{
    [Header("SOs")]
    public Input_SO inputSo;
    public PlayerSystem_SO playerSystemSo;
    public Enemy_SO enemySo;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject tilePrefab;
    public GameObject validPositionPrefab;
    public GameObject capturedPieceImagePrefab;
    public GameObject arrowPointingToNextPiecePrefab;

    [Header("Starting Piece")]
    public Piece startingPiece;

    [Header("Next Level")]
    public int nextLevelNumber;
    
    private void Start()
    {
        Random.InitState(42);
        
        Camera.main.backgroundColor = Color.black;
        
        foreach (Dependency dependency in _dependencies)
        {
            LevDependency levDependency = (LevDependency)dependency;
            levDependency.GameStart(this);
        }
    }
    
    public override void CreateDependencies()
    {
        _dependencies.Add(new LevAudioSystem());
        _dependencies.Add(new LevTurnSystem());
        _dependencies.Add(new LevChainUISystem());
        _dependencies.Add(new LevCinemachineSystem());
        _dependencies.Add(new LevDoorsSystem());
        _dependencies.Add(new LevEnemiesSystem());
        _dependencies.Add(new LevGridSystem());
        _dependencies.Add(new LevLevelCompleteUISystem());
        _dependencies.Add(new LevPauseUISystem());
        _dependencies.Add(new LevPlayerSystem());
        _dependencies.Add(new LevRestartLevelSystem());
        
        Debug.Log("DEPENDENCIES CREATED!");
    }
}
