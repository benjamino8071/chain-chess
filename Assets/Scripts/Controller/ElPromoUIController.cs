using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElPromoUIController : ElController
{
    private ElTimerUISystem _timerUISystem;
    
    private Transform _promoCanvas;

    private Piece _chosenPiece;

    private Dictionary<Piece, TMP_Text> _promoCostTexts = new();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        _timerUISystem = elCreator.GetDependency<ElTimerUISystem>();
    }

    public void Initialise(Transform playerObject)
    {
        _promoCanvas = playerObject.GetComponentInChildren<Canvas>().transform;
        
        Button[] promoButtons = playerObject.GetComponentsInChildren<Button>();
        foreach (Button promoButton in promoButtons)
        {
            TagName tagName = promoButton.GetComponent<TagName>();

            Piece piece;
            switch (tagName.tagName)
            {
                case AllTagNames.ChooseQueen:
                    piece = Piece.Queen;
                    break;
                case AllTagNames.ChooseRook:
                    piece = Piece.Rook;
                    break;
                case AllTagNames.ChooseKnight:
                    piece = Piece.Knight;
                    break;
                case AllTagNames.ChooseBishop:
                    piece = Piece.Bishop;
                    break;
                default:
                    Debug.LogError("Piece not set correctly!");
                    return;
            }
            
            promoButton.onClick.AddListener(() =>
            {
                SelectPromotion(piece);
                _timerUISystem.RemoveTime(Creator.timerSo.timeCost[Piece.Bishop], false);
            });
            _promoCostTexts.Add(piece, promoButton.GetComponentInChildren<TMP_Text>());
        }
        
        Hide(false);
    }

    private void SelectPromotion(Piece piece)
    {
        _chosenPiece = piece;
    }

    public void Show()
    {
        _timerUISystem.StopTimer();
        _chosenPiece = Piece.NotChosen;

        List<Piece> keys = Creator.timerSo.timeCost.Keys.ToList();
        foreach (Piece piece in keys)
        {
            float timeToRemove = Creator.timerSo.timeCost[piece];
            _promoCostTexts[piece].text = $"-{timeToRemove:0.#}s";
        }
        
        _promoCanvas.gameObject.SetActive(true);
    }

    public void Hide(bool startTimer)
    {
        _promoCanvas.gameObject.SetActive(false);
        _chosenPiece = Piece.NotChosen;
        
        if(startTimer)
            _timerUISystem.StartTimer();
    }

    public bool IsPromoChosen()
    {
        return _chosenPiece != Piece.NotChosen;
    }

    public Piece PieceChosen()
    {
        return _chosenPiece;
    }
}
