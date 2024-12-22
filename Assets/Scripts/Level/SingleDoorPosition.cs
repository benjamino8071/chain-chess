using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SingleDoorPosition : MonoBehaviour
{
    [SerializeField] SingleDoorPosition _otherDoor;
    
    [SerializeField] private Sprite _doorOpenSprite;
    [SerializeField] private Sprite _doorClosedSprite;
    
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    [SerializeField] private Transform _playerPositionOnOut;
    
    public bool isDoorOpen;
    
    [FormerlySerializedAs("_roomNumber")] public int roomNumber;

    public int doorNumber;
    
    [FormerlySerializedAs("_otherDoorRoomNumber")] public int otherDoorRoomNumber;

    public bool isFirstDoor;
    
    public bool isFinalDoor;
    
    private void Start()
    {
        if (isDoorOpen)
        {
            ForceDoorOpen();
        }
    }

    public Vector3 GetPlayerPositionOnOut()
    {
        return _playerPositionOnOut.position;
    }

    public int GetOtherDoorRoomNumber()
    {
        return otherDoorRoomNumber;
    }

    public void SetDoorToOpen()
    {
        if(isDoorOpen || isFirstDoor)
            return;
        ForceDoorOpen();
    }

    public void SetDoorToClose()
    {
        if (isDoorOpen)
        {
            _spriteRenderer.sprite = _doorClosedSprite;
            isDoorOpen = false;
            _otherDoor.SetDoorToClose();
        }
    }

    private void ForceDoorOpen()
    {
        _spriteRenderer.sprite = _doorOpenSprite;
        isDoorOpen = true;
        if(!isFinalDoor)
            _otherDoor.SetDoorToOpen();
    }
}
