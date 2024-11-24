using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineSystem : Dependency
{
    private Animator _animator;

    private CinemachineStateDrivenCamera _cinemachineStateDrivenCamera;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        GameObject cameraStateMachine = GameObject.FindWithTag("CameraStateMachine");
        _animator = cameraStateMachine.GetComponent<Animator>();

        _cinemachineStateDrivenCamera = cameraStateMachine.GetComponent<CinemachineStateDrivenCamera>();
        
        _creator.StartACoRoutine(SetFirstState());
    }

    private IEnumerator SetFirstState()
    {
        SwitchState(0);

        yield return new WaitForSeconds(0.1f);
        
        SwitchState(_creator.playerSystemSo.roomNumberSaved);
    }

    public void SwitchState(int roomNumber)
    {
        string roomStateName = "Room" + roomNumber + "Cam";
        
        _animator.Play(roomStateName);
    }
}
