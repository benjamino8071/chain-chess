using System.Collections;
using UnityEngine;

public class LevCinemachineSystem : LevDependency
{
    private Animator _animator;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        GameObject cameraStateMachine = GameObject.FindWithTag("CameraStateMachine");
        _animator = cameraStateMachine.GetComponent<Animator>();
        
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
