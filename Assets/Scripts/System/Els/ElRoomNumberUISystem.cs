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
        int levelNum = (Creator.playerSystemSo.roomNumberSaved / 8) + 1;
        _roomNumberText.text = $"Level {levelNum}\\nRoom {Creator.playerSystemSo.roomNumberSaved + 1}";
    }
}
