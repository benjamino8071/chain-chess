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
            TagName tagName = audioSource.GetComponent<TagName>();

            switch (tagName.tagName)
            {
                case AllTagNames.Moving:
                    _pieceMoveAudio = audioSource;
                    break;
                case AllTagNames.CapturedPiecesParent:
                    _enemyCapturedAudio = audioSource;
                    break;
                case AllTagNames.Enemy:
                    _capturedByEnemyAudio = audioSource;
                    break;
                case AllTagNames.Retry:
                    _timeLostAudio = audioSource;
                    break;
                case AllTagNames.Up:
                    _levelUpAudio = audioSource;
                    break;
                case AllTagNames.LevelComplete:
                    _roomCompleteAudio = audioSource;
                    break;
                case AllTagNames.NextLevel:
                    _doorClosedAudio = audioSource;
                    break;
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
