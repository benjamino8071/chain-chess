using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class DoorsSystem : ElDependency
{
    //Assumption we can make: door 0 of the room the player is in will ALWAYS be
    //First Key = Room Number, Second Key = Door Number
    private Dictionary<int, Dictionary<int, List<SingleDoorPosition>>> _roomDoors = new();

    private LinkedList<SingleDoorPosition> _doorPositions = new ();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

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
            _doorPositions.AddLast(singleDoorPosition);
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

    public LinkedList<SingleDoorPosition> GetDoorPositions()
    {
        return _doorPositions;
    }

    public Dictionary<int, Dictionary<int, List<SingleDoorPosition>>> GetRoomDoors()
    {
        return _roomDoors;
    }
}
