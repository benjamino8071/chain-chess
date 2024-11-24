using UnityEngine;

public class AudioSystem : Dependency
{
    private AudioSource _pieceMoveAudio;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _pieceMoveAudio = Camera.main.GetComponent<AudioSource>();
    }

    public void PlayerPieceMoveSFX()
    {
        _pieceMoveAudio.Play();
    }
}
