using System.Collections;
using UnityEngine;

public class ElCinemachineSystem : ElDependency
{
    private ElTimerUISystem _timerUISystem;
    
    private Animator _animator;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.NewTryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }

        GameObject cameraStateMachine = GameObject.FindWithTag("CameraStateMachine");
        _animator = cameraStateMachine.GetComponent<Animator>();
        
        Creator.StartACoRoutine(SetFirstState());
    }

    private IEnumerator SetFirstState()
    {
        SwitchState(0);

        yield return new WaitForSeconds(0.1f);
        
        SwitchState(Creator.playerSystemSo.roomNumberSaved);

        yield return new WaitForSeconds(1.6f);
        
        if (Creator.startTimerOnLoad)
        {
            _timerUISystem.StartTimer();
        }
    }

    public void SwitchState(int roomNumber)
    {
        string roomStateName = "Room" + roomNumber + "Cam";
        
        _animator.Play(roomStateName);
    }
}
