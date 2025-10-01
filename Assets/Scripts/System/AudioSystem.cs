using MoreMountains.Tools;
using UnityEngine;

public class AudioSystem : Dependency
{
    private Transform _camera;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _camera = Camera.main.transform;
    }

    public void PlayPieceSelectedSfx(float pitch)
    {
        PlaySfx(Creator.audioClipsSo.pieceSelected, pitch);
    }
    
    public void PlayPieceDoubleTapSelectedSfx(float pitch)
    {
        PlaySfx(Creator.audioClipsSo.doubleTapPositionSelected, pitch);
    }
    
    public void PlayPieceMoveSfx(float pitch)
    {
        PlaySfx(Creator.audioClipsSo.pieceMoved, pitch);
    }
    
    public void PlayPieceCapturedSfx(float pitch)
    {
        PlaySfx(Creator.audioClipsSo.pieceCaptured, pitch);
    }

    public void PlayLevelCompleteSfx()
    {
        PlaySfx(Creator.audioClipsSo.roomComplete);
    }

    public void PlayerGameOverSfx()
    {
        PlaySfx(Creator.audioClipsSo.gameOver);
    }

    public void PlayMenuOpenSfx()
    {
        PlaySfx(Creator.audioClipsSo.uiOpen);
    }

    public void PlayMenuCloseSfx()
    {
        PlaySfx(Creator.audioClipsSo.uiClose);
    }

    public void PlayUIClickSfx()
    {
        PlaySfx(Creator.audioClipsSo.uiClick);
    }
    
    public void PlayUIAltClickSfx(float pitch = 1)
    {
        PlaySfx(Creator.audioClipsSo.uiAltClick, pitch);
    }
    
    public void PlayUISignificantClickSfx()
    {
        PlaySfx(Creator.audioClipsSo.uiSignificantClick);
    }

    public void PlayUIRulebookTurnClickSfx()
    {
        PlaySfx(Creator.audioClipsSo.uiRulebookTurnClick);
    }

    private void PlayMusic(AudioClip clip, float thePitch = 1)
    {
        MMSoundManagerSoundPlayEvent.Trigger(clip, MMSoundManager.MMSoundManagerTracks.Music, _camera.position, pitch:thePitch);
    }
    
    private void PlaySfx(AudioClip clip, float thePitch = 1)
    {
        MMSoundManagerSoundPlayEvent.Trigger(clip, MMSoundManager.MMSoundManagerTracks.Sfx, _camera.position, pitch:thePitch);
    }
}
