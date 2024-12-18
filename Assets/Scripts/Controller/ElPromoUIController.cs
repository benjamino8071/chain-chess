using UnityEngine;
using UnityEngine.UI;

public class ElPromoUIController : ElController
{
    private ElTimerUISystem _timerUISystem;
    
    private Transform _promoCanvas;

    private Piece _chosenPiece;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        if (Creator.NewTryGetDependency(out ElTimerUISystem timerUISystem))
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
                    _timerUISystem.RemoveTime(9);
                });
            }
            else if (promoButton.CompareTag("ChooseRook"))
            {
                promoButton.onClick.AddListener(() =>
                {
                    SelectPromotion(Piece.Rook);
                    _timerUISystem.RemoveTime(5);
                });
            }
            else if (promoButton.CompareTag("ChooseKnight"))
            {
                promoButton.onClick.AddListener(() =>
                {
                    SelectPromotion(Piece.Knight);
                    _timerUISystem.RemoveTime(3);
                });
            }
            else if (promoButton.CompareTag("ChooseBishop"))
            {
                promoButton.onClick.AddListener(() =>
                {
                    SelectPromotion(Piece.Bishop);
                    _timerUISystem.RemoveTime(3);
                });
            }
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
}
