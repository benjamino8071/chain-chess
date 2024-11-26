using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineSystem : ElDependency
{
    private Animator _animator;

    private CinemachineStateDrivenCamera _cinemachineStateDrivenCamera;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        GameObject cameraStateMachine = GameObject.FindWithTag("CameraStateMachine");
        _animator = cameraStateMachine.GetComponent<Animator>();

        _cinemachineStateDrivenCamera = cameraStateMachine.GetComponent<CinemachineStateDrivenCamera>();
        
        Creator.StartACoRoutine(SetFirstState());
    }

    private IEnumerator SetFirstState()
    {
        SwitchState(0);

        yield return new WaitForSeconds(0.1f);
        
        SwitchState(Creator.playerSystemSo.roomNumberSaved);
    }

    public void SwitchState(int roomNumber)
    {
        string roomStateName = "Room" + roomNumber + "Cam";
        
        _animator.Play(roomStateName);
    }
}
