using UnityEngine;

public class ElCreator : Creator
{
    [Header("SOs")]
    public Input_SO inputSo;
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

    private void Start()
    { 
        Random.InitState(42);
        
        Camera.main.backgroundColor = Color.black;
        
        foreach (ElDependency dependency in _dependencies)
        {
            dependency.GameStart(this);
        }
    }
    
    public override void CreateDependencies()
    {
        _dependencies.Add(new ElGridSystem());
        _dependencies.Add(new ElDoorsSystem());
        _dependencies.Add(new ElPlayerSystem());
        _dependencies.Add(new ElEnemiesSystem());
        _dependencies.Add(new ElCinemachineSystem());
        _dependencies.Add(new ElAudioSystem());
        _dependencies.Add(new ElChainUISystem());
        _dependencies.Add(new ElPauseUISystem());
        _dependencies.Add(new ElGameOverUISystem());
        _dependencies.Add(new ElStartingPieceUISystem());
    }
}
