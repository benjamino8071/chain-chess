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
    }

    public void SwitchState(int roomNumber)
    {
        string roomStateName = "Room" + roomNumber + "Cam";
        
        _animator.Play(roomStateName);
    }
}
