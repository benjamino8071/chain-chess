using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ElFreeUpgradeSystem : ElDependency
{
    private ElXPBarUISystem _xpBarUISystem;
    private ElArtefactsUISystem _artefactsUISystem;
    
    private SpriteRenderer _freeItemOne;
    private SpriteRenderer _freeItemTwo;

    private Transform _chooseText;
    private Transform _swapPositionsButton;

    private List<ArtefactTypes> _artefactTypes = new()
    {
        ArtefactTypes.CaptureKingClearRoom,
        ArtefactTypes.ConCaptureAttackingEnemy,
        ArtefactTypes.EnemyLineOfSight,
        ArtefactTypes.UseCapturedPieceStraightAway
    };

    private List<Piece> _pieces = new()
    {
        Piece.Bishop,
        Piece.King,
        Piece.Knight,
        Piece.Pawn,
        Piece.Queen,
        Piece.Rook
    };

    private ArtefactTypes _artefactChosen;
    private Piece _losPieceChosen;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _xpBarUISystem = elCreator.GetDependency<ElXPBarUISystem>();
        _artefactsUISystem = elCreator.GetDependency<ElArtefactsUISystem>();

        Transform freeItems = elCreator.GetFirstObjectWithName(AllTagNames.FreeItems);

        Transform freeItemLeft = elCreator.GetChildObjectByName(freeItems.gameObject, AllTagNames.FreeItemLeft);
        _freeItemOne = freeItemLeft.GetComponent<SpriteRenderer>();

        Transform freeItemRight = elCreator.GetChildObjectByName(freeItems.gameObject, AllTagNames.FreeItemRight);
        _freeItemTwo = freeItemRight.GetComponent<SpriteRenderer>();

        Transform chooseText = elCreator.GetChildObjectByName(freeItems.gameObject, AllTagNames.Text);
        _chooseText = chooseText;
        
        Button swapPosButton = freeItems.GetComponentInChildren<Button>();
        swapPosButton.onClick.AddListener(() =>
        {
            if(_freeItemOne.gameObject.activeSelf && _freeItemTwo.gameObject.activeSelf)
                SwapPositions();
        });
        _swapPositionsButton = swapPosButton.transform;

        List<ArtefactTypes> artefactTypes = _artefactTypes.ToList();
        foreach (ArtefactTypes artefactType in elCreator.upgradeSo.artefactsChosen)
        {
            artefactTypes.Remove(artefactType);
        }
        
        int chosenArtefactIndex = Random.Range(0, artefactTypes.Count);
        _artefactChosen = artefactTypes[chosenArtefactIndex];

        if (_artefactChosen == ArtefactTypes.EnemyLineOfSight)
        {
            List<Piece> pieces = _pieces.ToList();

            foreach (Piece piece in elCreator.upgradeSo.lineOfSightsChosen)
            {
                pieces.Remove(piece);
            }
            
            int chosenLosIndex = Random.Range(0, pieces.Count);
            _losPieceChosen = pieces[chosenLosIndex];
        }
        
        switch (_artefactChosen)
        {
            case ArtefactTypes.CaptureKingClearRoom:
                _freeItemOne.sprite = elCreator.upgradeSo.captureKingClearRoomSprite;
                break;
            case ArtefactTypes.ConCaptureAttackingEnemy:
                _freeItemOne.sprite = elCreator.upgradeSo.destroyChainStayAliveSprite;
                break;
            case ArtefactTypes.EnemyLineOfSight:
                switch (_losPieceChosen)
                {
                    case Piece.Bishop:
                        _freeItemOne.sprite = elCreator.upgradeSo.bishopLosSprite;
                        break;
                    case Piece.King:
                        _freeItemOne.sprite = elCreator.upgradeSo.kingLosSprite;
                        break;
                    case Piece.Knight:
                        _freeItemOne.sprite = elCreator.upgradeSo.knightLosSprite;
                        break;
                    case Piece.Pawn:
                        _freeItemOne.sprite = elCreator.upgradeSo.pawnLosSprite;
                        break;
                    case Piece.Queen:
                        _freeItemOne.sprite = elCreator.upgradeSo.queenLosSprite;
                        break;
                    case Piece.Rook:
                        _freeItemOne.sprite = elCreator.upgradeSo.rookLosSprite;
                        break;
                }
                break;
            case ArtefactTypes.UseCapturedPieceStraightAway:
                _freeItemOne.sprite = elCreator.upgradeSo.useCapturedPieceStraightAwaySprite;
                break;
        }

        _freeItemTwo.sprite = elCreator.upgradeSo.xpLevelUpgradeSprite;
    }

    public bool TryGetFreeItem(Vector3 position)
    {
        if (position == _freeItemOne.transform.position)
        {
            if (_artefactsUISystem.TryAddArtefact(_artefactChosen, _losPieceChosen))
            {
                _freeItemOne.gameObject.SetActive(false);
                _freeItemTwo.gameObject.SetActive(false);
                _chooseText.gameObject.SetActive(false);
                _swapPositionsButton.gameObject.SetActive(false);
                return true;
            }
            else
            {
                //ERROR EFFECT FOR HAVING 3 ARTEFACTS IN POSSESSION
            }
        }
        
        if (position == _freeItemTwo.transform.position)
        {
            //XP LEVEL UPGRADE
            _xpBarUISystem.UpgradeByFullLevel();
            
            _freeItemOne.gameObject.SetActive(false);
            _freeItemTwo.gameObject.SetActive(false);
            _chooseText.gameObject.SetActive(false);
            _swapPositionsButton.gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    public void SwapPositions()
    {
        (_freeItemTwo.transform.position, _freeItemOne.transform.position) = (_freeItemOne.transform.position, _freeItemTwo.transform.position);
    }
}
