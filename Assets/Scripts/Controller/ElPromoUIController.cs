using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElPromoUIController : ElController
{
    private ElXPBarUISystem _xpBarUISystem;
    private ElPlayerSystem _playerSystem;
    
    private Transform _promoCanvas;

    private Piece _chosenPiece;

    private Dictionary<Piece, TMP_Text> _promoXpGainTexts = new();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _xpBarUISystem = elCreator.GetDependency<ElXPBarUISystem>();
        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();
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
                float xpGain = Creator.upgradeSo.promotionXPGain[piece];
                _xpBarUISystem.IncreaseProgressBar(xpGain, false);
            });
            _promoXpGainTexts.Add(piece, promoButton.GetComponentInChildren<TMP_Text>());
        }

        Hide();
    }

    private void SelectPromotion(Piece piece)
    {
        _chosenPiece = piece;
    }

    public void Show()
    {
        _chosenPiece = Piece.NotChosen;
        
        SetPromoXpGainText();
        
        _promoCanvas.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _promoCanvas.gameObject.SetActive(false);
        _chosenPiece = Piece.NotChosen;
    }

    public bool IsPromoChosen()
    {
        return _chosenPiece != Piece.NotChosen;
    }

    public Piece PieceChosen()
    {
        return _chosenPiece;
    }

    public void SetPromoXpGainText()
    {
        List<Piece> promoPieces = Creator.upgradeSo.promotionXPGain.Keys.ToList();
        foreach (Piece promoPiece in promoPieces)
        {
            float xpGain = Creator.upgradeSo.promotionXPGain[promoPiece];
            _promoXpGainTexts[promoPiece].text = $"+{xpGain:0}xp";
        }
    }
}
