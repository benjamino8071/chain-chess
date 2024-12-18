using TMPro;
using UnityEngine;

public class ElRoomNumberUISystem : ElDependency
{
    private TextMeshProUGUI _roomNumberText;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _roomNumberText = GameObject.FindWithTag("LevelText").GetComponent<TextMeshProUGUI>();
        
        UpdateRoomNumberText();
    }

    public void UpdateRoomNumberText()
    {
        int roomNum = Creator.playerSystemSo.levelNumberSaved * 8 + Creator.playerSystemSo.roomNumberSaved + 1;
        _roomNumberText.text = $"Room {roomNum}";
    }
}
