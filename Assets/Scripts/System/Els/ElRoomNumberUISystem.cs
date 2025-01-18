using TMPro;
using UnityEngine;

public class ElRoomNumberUISystem : ElDependency
{
    private TextMeshProUGUI _roomNumberText;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        Transform roomNumberText = elCreator.GetFirstObjectWithName(AllTagNames.LevelText);
        _roomNumberText = roomNumberText.GetComponent<TextMeshProUGUI>();
        
        UpdateRoomNumberText();
    }

    public void UpdateRoomNumberText()
    {
        int roomNum = Creator.playerSystemSo.levelNumberSaved * 10 + Creator.playerSystemSo.roomNumberSaved + 1;
        _roomNumberText.text = $"Room {roomNum}/{Creator.gameDataSo.numOfRoomsToBeat}";
    }
}
