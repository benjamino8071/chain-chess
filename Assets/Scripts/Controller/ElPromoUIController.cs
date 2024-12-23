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
        
        if (Creator.TryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
    }

    public void Initialise(Transform playerObject)
    {
        _promoCanvas = playerObject.GetComponentInChildren<Canvas>().transform;
        
        Button[] promoButtons = playerObject.GetComponentsInChildren<Button>();
        foreach (Button promoButton in promoButtons)
        {
            if (promoButton.CompareTag("ChooseQueen"))
            {
                promoButton.onClick.AddListener(() =>
                {
                    SelectPromotion(Piece.Queen);
                    _timerUISystem.RemoveTime(Creator.timerSo.timeCost[Piece.Queen], false);
                });
                _promoCostTexts.Add(Piece.Queen, promoButton.GetComponentInChildren<TMP_Text>());
            }
            else if (promoButton.CompareTag("ChooseRook"))
            {
                promoButton.onClick.AddListener(() =>
                {
                    SelectPromotion(Piece.Rook);
                    _timerUISystem.RemoveTime(Creator.timerSo.timeCost[Piece.Rook], false);
                });
                _promoCostTexts.Add(Piece.Rook, promoButton.GetComponentInChildren<TMP_Text>());
            }
            else if (promoButton.CompareTag("ChooseKnight"))
            {
                promoButton.onClick.AddListener(() =>
                {
                    SelectPromotion(Piece.Knight);
                    _timerUISystem.RemoveTime(Creator.timerSo.timeCost[Piece.Knight], false);
                });
                _promoCostTexts.Add(Piece.Knight, promoButton.GetComponentInChildren<TMP_Text>());
            }
            else if (promoButton.CompareTag("ChooseBishop"))
            {
                promoButton.onClick.AddListener(() =>
                {
                    SelectPromotion(Piece.Bishop);
                    _timerUISystem.RemoveTime(Creator.timerSo.timeCost[Piece.Bishop], false);
                });
                _promoCostTexts.Add(Piece.Bishop, promoButton.GetComponentInChildren<TMP_Text>());
            }
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
