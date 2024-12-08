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
        _roomNumberText.text = $"Level {Creator.playerSystemSo.levelNumberSaved + 1}\\nRoom {Creator.playerSystemSo.roomNumberSaved + 1}";
    }
}
