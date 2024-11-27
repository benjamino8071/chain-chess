using UnityEngine;

public class ElAudioSystem : ElDependency
{
    private AudioSource _pieceMoveAudio;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        _pieceMoveAudio = Camera.main.GetComponent<AudioSource>();
    }
    
    public void PlayerPieceMoveSfx()
    {
        _pieceMoveAudio.Play();
    }
}
