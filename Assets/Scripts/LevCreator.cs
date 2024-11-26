using UnityEngine;

public class LevCreator : Creator
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
    
    public override void CreateDependencies()
    {
        _dependencies.Add(new LevAudioSystem());
        _dependencies.Add(new LevCapturedPiecesUISystem());
        _dependencies.Add(new LevCinemachineSystem());
        _dependencies.Add(new LevDoorsSystem());
        _dependencies.Add(new LevEnemiesSystem());
        _dependencies.Add(new LevGridSystem());
        _dependencies.Add(new LevLevelCompleteSystem());
        _dependencies.Add(new LevPauseUISystem());
        _dependencies.Add(new LevPlayerSystem());
        _dependencies.Add(new LevRestartLevelSystem());
    }
}
