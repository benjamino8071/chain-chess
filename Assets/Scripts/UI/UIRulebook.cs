using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRulebook : UIPanel
{
    private Image _image;
    private TextMeshProUGUI _text;

    private int _currentIndex;
    
    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        _image = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Image);
        _text = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Text);
        
        ButtonManager leftButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonLeft);
        leftButton.onClick.AddListener(() =>
        {
            _currentIndex--;
            if (_currentIndex < 0)
            {
                _currentIndex = Creator.rulebookSo.pages.Count - 1;
            }
            SetCurrentPage(_currentIndex);
        });
        
        ButtonManager rightButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonRight);
        rightButton.onClick.AddListener(() =>
        {
            _currentIndex++;
            if (_currentIndex >= Creator.rulebookSo.pages.Count)
            {
                _currentIndex = 0;
            }
            SetCurrentPage(_currentIndex);
        });

        SetCurrentPage(0);
        
        Hide();
    }

    public override void Show()
    {
        base.Show();
    }

    private void SetCurrentPage(int index)
    {
        Page page = Creator.rulebookSo.pages[index];

        _image.sprite = page.sprite;
        _image.material = page.material;
        
        _text.text = page.description;
    }
}
