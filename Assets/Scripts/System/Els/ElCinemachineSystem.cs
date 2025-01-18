using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ElCinemachineSystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    
    private Transform _cameraStateMachine;
    
    private Animator _animator;

    private List<CinemachineCamera> _cameras = new();

    private ScreenOrientation _previousOrientation;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();

        _cameraStateMachine = elCreator.GetFirstObjectWithName(AllTagNames.CameraStateMachine);

        _cameraOriginalPos = _cameraStateMachine.position;
        
        CinemachineCamera[] cameras = _cameraStateMachine.GetComponentsInChildren<CinemachineCamera>();
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
        
        _animator = _cameraStateMachine.GetComponent<Animator>();
        
        Creator.StartACoRoutine(SetFirstState());
        
        Creator.inputSo.dragCameraInput.action.performed += DragCameraInput_Performed;
        Creator.inputSo.dragCameraInput.action.canceled += DragCameraInput_Canceled;
    }

    public override void GameEnd()
    {
        Creator.inputSo.dragCameraInput.action.performed -= DragCameraInput_Performed;
        Creator.inputSo.dragCameraInput.action.canceled -= DragCameraInput_Canceled;
    }

    private Vector3 _cameraOriginalPos;
    private Vector3 _cameraLastPos;
    
    private bool _isDraggingCamera;

    private float _mousePosYLastFrame;

    private float _hasntMovedCameraTimer;
    private float _hasntMovedCameraMaxTime = 2f;
    
    private void DragCameraInput_Performed(InputAction.CallbackContext obj)
    {
        _mousePosYLastFrame = Input.mousePosition.y;
        _isDraggingCamera = true;
    }
    
    private void DragCameraInput_Canceled(InputAction.CallbackContext obj)
    {
        _cameraLastPos = _cameraStateMachine.position;
        _hasntMovedCameraTimer = _hasntMovedCameraMaxTime;
        _isDraggingCamera = false;
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

        if (_isDraggingCamera && _playerSystem.GetState() == ElPlayerSystem.States.Idle && Creator.cinemachineSo.enableCameraDrag)
        {
            float mousePosYCurrFrame = Input.mousePosition.y;
            float yValueChange = mousePosYCurrFrame - _mousePosYLastFrame;
            
            _cameraStateMachine.position += new Vector3(_cameraStateMachine.position.x, -yValueChange * 0.05f,
                _cameraStateMachine.position.z);

            _mousePosYLastFrame = mousePosYCurrFrame;
        }
        else if (_hasntMovedCameraTimer > 0)
        {
            _hasntMovedCameraTimer -= dt;

            if (_hasntMovedCameraTimer <= _hasntMovedCameraMaxTime / 2)
            {
                //After half the time required we lerp back
                
                _cameraStateMachine.position = Vector3.Lerp(_cameraOriginalPos, _cameraLastPos,
                    _hasntMovedCameraTimer / (_hasntMovedCameraMaxTime / 2));
                
                if (_hasntMovedCameraTimer <= 0)
                {
                    //Snap camera back to original pos
                    _cameraStateMachine.position = _cameraOriginalPos;
                }
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
