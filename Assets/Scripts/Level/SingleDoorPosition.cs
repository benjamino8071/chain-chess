using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SingleDoorPosition : MonoBehaviour
{
    [SerializeField] SingleDoorPosition _otherDoor;
    
    [SerializeField] private Sprite _doorOpenSprite;
    
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    [SerializeField] private Transform _playerPositionOnOut;
    
    public bool isDoorOpen;
    
    [FormerlySerializedAs("_roomNumber")] public int roomNumber;

    public int doorNumber;
    
    [FormerlySerializedAs("_otherDoorRoomNumber")] public int otherDoorRoomNumber;

    public bool isFinalDoor;

    public PlayerSystem_SO playerSystemSo;
    
    private void Start()
    {
        //Open the door if the player is already passed this point
        if (roomNumber < playerSystemSo.roomNumberSaved)
        {
            SetDoorToOpen();
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
        if(isDoorOpen)
            return;
        _spriteRenderer.sprite = _doorOpenSprite;
        isDoorOpen = true;
        _otherDoor.SetDoorToOpen();
    }

    private void ForceDoorOpen()
    {
        _spriteRenderer.sprite = _doorOpenSprite;
        isDoorOpen = true;
        _otherDoor.SetDoorToOpen();
    }
}
