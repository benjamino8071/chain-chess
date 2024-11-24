using System.Collections.Generic;
using UnityEngine;

public class DoorsSystem : Dependency
{
    //Assumption we can make: door 0 of the room the player is in will ALWAYS be
    //First Key = Room Number, Second Key = Door Number
    private Dictionary<int, Dictionary<int, List<SingleDoorPosition>>> _roomDoors = new();
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        GameObject[] doorPositions = GameObject.FindGameObjectsWithTag("DoorPosition");
        foreach (GameObject doorPosition in doorPositions)
        {
            SingleDoorPosition singleDoorPosition = doorPosition.GetComponent<SingleDoorPosition>();
            if (_roomDoors.TryGetValue(singleDoorPosition.roomNumber, out Dictionary<int, List<SingleDoorPosition>> doorNumbers))
            {
                if (doorNumbers.ContainsKey(singleDoorPosition.doorNumber))
                {
                    _roomDoors[singleDoorPosition.roomNumber][singleDoorPosition.doorNumber].Add(singleDoorPosition);
                }
                else
                {
                    List<SingleDoorPosition> newDoorPositions = new()
                    {
                        singleDoorPosition
                    };
                    doorNumbers.Add(singleDoorPosition.doorNumber, newDoorPositions);
                    _roomDoors[singleDoorPosition.roomNumber] = doorNumbers;
                }
                
            }
            else
            {
                List<SingleDoorPosition> newDoorPositions = new()
                {
                    singleDoorPosition
                };
                Dictionary<int, List<SingleDoorPosition>> newDoorNumbers = new()
                {
                    {singleDoorPosition.doorNumber, newDoorPositions}
                };
                _roomDoors.Add(singleDoorPosition.roomNumber, newDoorNumbers);
            }
        }
    }

    public void SetRoomDoorsOpen(int roomNumber)
    {
        Dictionary<int, List<SingleDoorPosition>> doorsInRoom = _roomDoors[roomNumber];
        foreach (List<SingleDoorPosition> singleDoorPositions in doorsInRoom.Values)
        {
            foreach (SingleDoorPosition singleDoorPosition in singleDoorPositions)
            {
                singleDoorPosition.SetDoorToOpen();
            }
        }
    }

    public bool IsDoorOpen(int roomNumber, int doorNumber)
    {
        if (_roomDoors.ContainsKey(roomNumber))
        {
            if (_roomDoors[roomNumber].ContainsKey(doorNumber))
            {
                return true;
            }
        }

        return false;
    }
}
