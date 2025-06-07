using MoreMountains.Tools;
using UnityEngine;

public class ElAudioSystem : ElDependency
{
    private Transform _camera;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        _camera = Camera.main.transform;
    }
    
    public void PlayerPieceMoveSfx(float thePitch)
    {
        PlaySfx(Creator.audioClipsSo.pieceMoved, _camera.position, thePitch);
    }
    
    public void PlayTimeLostSfx(float thePitch)
    {
        PlaySfx(Creator.audioClipsSo.timeLost, _camera.position, thePitch);
    }

    /// <summary>
    /// Pitch must increase in value as pieces are captured consecutively
    /// </summary>
    /// <param name="thePitch"></param>
    public void PlayTimeAddedSfx(float thePitch)
    {
        PlaySfx(Creator.audioClipsSo.pieceCaptured, _camera.position, thePitch);
    }

    public void PlayCapturedByEnemySfx()
    {
        PlaySfx(Creator.audioClipsSo.gameOver, _camera.position);
    }

    public void PlayerLevelUpSfx()
    {
        PlaySfx(Creator.audioClipsSo.levelUp, _camera.position);
    }

    public void PlayDoorClosedSfx()
    {
        //TODO: Find better audio for closing door
    }

    public void PlayRoomCompleteSfx()
    {
        PlaySfx(Creator.audioClipsSo.roomComplete, _camera.position);
    }

    private void PlayMusic(AudioClip clip, Vector3 position, float thePitch = 1)
    {
        MMSoundManagerSoundPlayEvent.Trigger(clip, MMSoundManager.MMSoundManagerTracks.Music, _camera.position, pitch:thePitch);
    }
    
    private void PlaySfx(AudioClip clip, Vector3 position, float thePitch = 1)
    {
        MMSoundManagerSoundPlayEvent.Trigger(clip, MMSoundManager.MMSoundManagerTracks.Sfx, _camera.position, pitch:thePitch);
    }
}
