using UnityEngine;

public class ElAudioSystem : ElDependency
{
    private AudioSource _pieceMoveAudio;
    private AudioSource _enemyCapturedAudio;
    private AudioSource _capturedByEnemyAudio;
    private AudioSource _timeLostAudio;
    private AudioSource _levelUpAudio;
    private AudioSource _doorClosedAudio;
    private AudioSource _roomCompleteAudio;
    
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
            else if (audioSource.CompareTag("Retry"))
            {
                _timeLostAudio = audioSource;
            }
            else if (audioSource.CompareTag("Up"))
            {
                _levelUpAudio = audioSource;
            }
            else if (audioSource.CompareTag("LevelComplete"))
            {
                _roomCompleteAudio = audioSource;
            }
            else if (audioSource.CompareTag("NextLevel"))
            {
                _doorClosedAudio = audioSource;
            }
        }
    }
    
    public void PlayerPieceMoveSfx(float pitch)
    {
        _pieceMoveAudio.pitch = pitch;
        _pieceMoveAudio.Play();
    }
    
    public void PlayTimeLostSfx(float pitch)
    {
        _timeLostAudio.pitch = pitch;
        _timeLostAudio.Play();
    }

    /// <summary>
    /// Pitch must increase in value as pieces are captured consecutively
    /// </summary>
    /// <param name="pitch"></param>
    public void PlayTimeAddedSfx(float pitch)
    {
        _enemyCapturedAudio.pitch = pitch;
        _enemyCapturedAudio.Play();
    }

    public void PlayCapturedByEnemySfx()
    {
        _capturedByEnemyAudio.Play();
    }

    public void PlayerLevelUpSfx()
    {
        _levelUpAudio.Play();
    }

    public void PlayDoorClosedSfx()
    {
        //TODO: Find better audio for closing door
        //_doorClosedAudio.Play();
    }

    public void PlayRoomCompleteSfx()
    {
        _roomCompleteAudio.Play();
    }
}
