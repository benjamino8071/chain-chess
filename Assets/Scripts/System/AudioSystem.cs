using UnityEngine;

public class AudioSystem : ElDependency
{
    private AudioSource _pieceMoveAudio;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _pieceMoveAudio = Camera.main.GetComponent<AudioSource>();
    }

    public void PlayerPieceMoveSFX()
    {
        _pieceMoveAudio.Play();
    }
}
