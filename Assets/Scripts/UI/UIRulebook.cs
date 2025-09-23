using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRulebook : UIPanel
{
    private Image _image;
    private TextMeshProUGUI _text;

    private int _currentIndex;

    private class HighlightImage
    {
        public int index;
        public Image image;
    }
    private List<HighlightImage> _highlightImages;
    
    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        _image = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.Image);
        _text = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.Text);

        Transform highlightImagesParent =
            Creator.GetChildComponentByName<Transform>(_panel.gameObject, AllTagNames.Parent);
        
        _highlightImages = new(Creator.rulebookSo.pages.Count);
        for (int i = 0; i < Creator.rulebookSo.pages.Count; i++)
        {
            GameObject highlightImageGo = Creator.InstantiateGameObjectWithParent(Creator.rulebookHighlightImagePrefab, highlightImagesParent);
            
            Image highlightImage = highlightImageGo.GetComponent<Image>();
            highlightImage.color = Creator.rulebookSo.notHighlightedColour;
            
            Button highlightButton = highlightImageGo.GetComponent<Button>();

            int index = i;
            highlightButton.onClick.AddListener(() =>
            {
                SetCurrentPage(index);
            });
            
            _highlightImages.Add(new()
            {
                index = index,
                image = highlightImage,
            });
        }
        
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
    }

    private void SetCurrentPage(int index)
    {
        Page page = Creator.rulebookSo.pages[index];

        _image.sprite = page.sprite;
        _image.material = page.material;
        
        _text.text = page.description;

        foreach (HighlightImage highlightImage in _highlightImages)
        {
            highlightImage.image.color = highlightImage.index == index ? Creator.rulebookSo.highlightedColour : Creator.rulebookSo.notHighlightedColour;
        }
    }
}
