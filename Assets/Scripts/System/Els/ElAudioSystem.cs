using UnityEngine;

public class ElAudioSystem : ElDependency
{
    private AudioSource _pieceMoveAudio;
    private AudioSource _enemyCapturedAudio;
    private AudioSource _capturedByEnemyAudio;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        AudioSource[] audioSources = Camera.main.GetComponentsInChildren<AudioSource>();

        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.CompareTag("Player"))
            {
                _pieceMoveAudio = audioSource;
            }
            else if (audioSource.CompareTag("CapturedPiecesParent"))
            {
                _enemyCapturedAudio = audioSource;
            }
            else if (audioSource.CompareTag("Enemy"))
            {
                _capturedByEnemyAudio = audioSource;
            }
        }
    }
    
    public void PlayerPieceMoveSfx(float pitch)
    {
        _pieceMoveAudio.pitch = pitch;
        _pieceMoveAudio.Play();
    }

    /// <summary>
    /// Pitch must increase in value as pieces are captured consecutively
    /// </summary>
    /// <param name="pitch"></param>
    public void PlayEnemyCapturedSfx(float pitch)
    {
        _enemyCapturedAudio.pitch = pitch;
        _enemyCapturedAudio.Play();
    }

    public void PlayCapturedByEnemySfx()
    {
        _capturedByEnemyAudio.Play();
    }
}
