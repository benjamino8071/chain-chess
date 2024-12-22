using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class ElCinemachineSystem : ElDependency
{
    private ElTimerUISystem _timerUISystem;
    
    private Animator _animator;

    private List<CinemachineCamera> _cameras = new();

    private ScreenOrientation _previousOrientation;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.TryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        
        GameObject cameraStateMachine = GameObject.FindWithTag("CameraStateMachine");
        CinemachineCamera[] cameras = cameraStateMachine.GetComponentsInChildren<CinemachineCamera>();
        _cameras = cameras.ToList();

        _previousOrientation = Screen.orientation;
        
        foreach (CinemachineCamera camera in _cameras)
        {
            if (Application.isMobilePlatform)
            {
                camera.Lens.FieldOfView = Creator.cinemachineSo.mobileVerticalFOV;
            }
            else
            {
                camera.Lens.FieldOfView = Creator.cinemachineSo.desktopVerticalFOV;
            }
        }
        
        _animator = cameraStateMachine.GetComponent<Animator>();
        
        Creator.StartACoRoutine(SetFirstState());
    }

    public override void GameUpdate(float dt)
    {
        if (Application.isMobilePlatform)
        {
            if (Screen.orientation == ScreenOrientation.Portrait && _previousOrientation != ScreenOrientation.Portrait)
            {
                foreach (CinemachineCamera camera in _cameras)
                {
                    camera.Lens.FieldOfView = Creator.cinemachineSo.mobileVerticalFOV;
                }
                _previousOrientation = ScreenOrientation.Portrait;
            }
            else if ((Screen.orientation == ScreenOrientation.LandscapeLeft ||
                     Screen.orientation == ScreenOrientation.LandscapeRight) && _previousOrientation != ScreenOrientation.LandscapeLeft)
            {
                foreach (CinemachineCamera camera in _cameras)
                {
                    camera.Lens.FieldOfView = Creator.cinemachineSo.desktopVerticalFOV;
                }
                _previousOrientation = ScreenOrientation.LandscapeLeft;
            }
        }
    }

    private IEnumerator SetFirstState()
    {
        SwitchState(0);

        yield return new WaitForSeconds(0.1f);
        
        SwitchState(Creator.playerSystemSo.roomNumberSaved);

        yield return new WaitForSeconds(1.6f);
    }

    public void SwitchState(int roomNumber)
    {
        string roomStateName = "Room" + roomNumber + "Cam";
        
        _animator.Play(roomStateName);
    }
}
