using UnityEngine;

public class LevAudioSystem : LevDependency
{
    private AudioSource _pieceMoveAudio;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _pieceMoveAudio = Camera.main.GetComponent<AudioSource>();
    }
    
    public void PlayerPieceMoveSfx()
    {
        _pieceMoveAudio.Play();
    }
}
