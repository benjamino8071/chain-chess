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
        _dependencies.AddLast(new LevAudioSystem());
        _dependencies.AddLast(new LevTurnSystem());
        _dependencies.AddLast(new LevChainUISystem());
        _dependencies.AddLast(new LevCinemachineSystem());
        _dependencies.AddLast(new LevDoorsSystem());
        _dependencies.AddLast(new LevEnemiesSystem());
        _dependencies.AddLast(new LevGridSystem());
        _dependencies.AddLast(new LevLevelCompleteUISystem());
        _dependencies.AddLast(new LevPauseUISystem());
        _dependencies.AddLast(new LevPlayerSystem());
        _dependencies.AddLast(new LevRestartLevelSystem());
        _dependencies.AddLast(new LevGameOverUISystem());
    }
}
