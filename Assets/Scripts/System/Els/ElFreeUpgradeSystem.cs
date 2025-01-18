using System.Collections.Generic;
using System.Linq;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.UI;

public class ElFreeUpgradeSystem : ElDependency
{
    private ElXPBarUISystem _xpBarUISystem;
    private ElArtefactsUISystem _artefactsUISystem;

    private Transform _guiPosChange;
    
    private SpriteRenderer _freeItemOne;
    private SpriteRenderer _freeItemTwo;
    
    private Transform _chooseText;

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
    
    private List<ArtefactTypes> _artefactsChosen = new();
    private Piece _losPieceChosen;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _xpBarUISystem = elCreator.GetDependency<ElXPBarUISystem>();
        _artefactsUISystem = elCreator.GetDependency<ElArtefactsUISystem>();

        _guiPosChange = elCreator.GetFirstObjectWithName(AllTagNames.GUITopFreeRoomPosChange);

        Transform posChangeButtonTf =
            elCreator.GetChildObjectByName(_guiPosChange.gameObject, AllTagNames.PosChangeButton);
        ButtonManager posChangeButton = posChangeButtonTf.GetComponent<ButtonManager>();
        posChangeButton.onClick.AddListener(() =>
        {
            if(_freeItemOne.gameObject.activeSelf && _freeItemTwo.gameObject.activeSelf)
                SwapPositions();
        });
        
        Transform freeItems = elCreator.GetFirstObjectWithName(AllTagNames.FreeItems);

        Transform freeItemLeft = elCreator.GetChildObjectByName(freeItems.gameObject, AllTagNames.FreeItemLeft);
        _freeItemOne = freeItemLeft.GetComponent<SpriteRenderer>();

        Transform freeItemRight = elCreator.GetChildObjectByName(freeItems.gameObject, AllTagNames.FreeItemRight);
        _freeItemTwo = freeItemRight.GetComponent<SpriteRenderer>();

        Transform chooseText = elCreator.GetChildObjectByName(freeItems.gameObject, AllTagNames.Text);
        _chooseText = chooseText;
        
        _freeItemOne.gameObject.SetActive(false);
        _freeItemTwo.gameObject.SetActive(false);
        _chooseText.gameObject.SetActive(false);
        
        HideGuiPosChange();
    }

    public void SetFreeRoom()
    {
        _freeItemOne.gameObject.SetActive(true);
        _freeItemTwo.gameObject.SetActive(true);
        _chooseText.gameObject.SetActive(true);

        List<SpriteRenderer> freeItemSprites = new()
        {
            _freeItemOne,
            _freeItemTwo
        };

        foreach (SpriteRenderer freeItemSprite in freeItemSprites)
        {
            List<ArtefactTypes> artefactTypes = _artefactTypes.ToList();
            
            //This for loop should technically only iterate once
            foreach (ArtefactTypes artefactTypeInFreeRoom in _artefactsChosen)
            {
                artefactTypes.Remove(artefactTypeInFreeRoom);
            }
            
            foreach (ArtefactTypes artefactType in Creator.upgradeSo.artefactsChosen)
            {
                artefactTypes.Remove(artefactType);
            }
            
            int chosenArtefactIndex = Random.Range(0, artefactTypes.Count);
            ArtefactTypes artefactChosen = artefactTypes[chosenArtefactIndex];

            if (artefactChosen == ArtefactTypes.EnemyLineOfSight)
            {
                List<Piece> pieces = _pieces.ToList();

                foreach (Piece piece in Creator.upgradeSo.lineOfSightsChosen)
                {
                    pieces.Remove(piece);
                }
                
                int chosenLosIndex = Random.Range(0, pieces.Count);
                _losPieceChosen = pieces[chosenLosIndex];
            }
            
            switch (artefactChosen)
            {
                case ArtefactTypes.CaptureKingClearRoom:
                    freeItemSprite.sprite = Creator.upgradeSo.captureKingClearRoomSprite;
                    break;
                case ArtefactTypes.ConCaptureAttackingEnemy:
                    freeItemSprite.sprite = Creator.upgradeSo.destroyChainStayAliveSprite;
                    break;
                case ArtefactTypes.EnemyLineOfSight:
                    switch (_losPieceChosen)
                    {
                        case Piece.Bishop:
                            freeItemSprite.sprite = Creator.upgradeSo.bishopLosSprite;
                            break;
                        case Piece.King:
                            freeItemSprite.sprite = Creator.upgradeSo.kingLosSprite;
                            break;
                        case Piece.Knight:
                            freeItemSprite.sprite = Creator.upgradeSo.knightLosSprite;
                            break;
                        case Piece.Pawn:
                            freeItemSprite.sprite = Creator.upgradeSo.pawnLosSprite;
                            break;
                        case Piece.Queen:
                            freeItemSprite.sprite = Creator.upgradeSo.queenLosSprite;
                            break;
                        case Piece.Rook:
                            freeItemSprite.sprite = Creator.upgradeSo.rookLosSprite;
                            break;
                    }
                    break;
                case ArtefactTypes.UseCapturedPieceStraightAway:
                    freeItemSprite.sprite = Creator.upgradeSo.useCapturedPieceStraightAwaySprite;
                    break;
            }
            
            _artefactsChosen.Add(artefactChosen);
        }
    }

    public bool TryGetFreeItem(Vector3 position)
    {
        if (position == _freeItemOne.transform.position)
        {
            if (_artefactsUISystem.TryAddArtefact(_artefactsChosen[0], _losPieceChosen))
            {
                _freeItemOne.gameObject.SetActive(false);
                _freeItemTwo.gameObject.SetActive(false);
                _chooseText.gameObject.SetActive(false);
                return true;
            }
            else
            {
                //ERROR EFFECT FOR HAVING 3 ARTEFACTS IN POSSESSION
            }
        }
        else if (position == _freeItemTwo.transform.position)
        {
            if (_artefactsUISystem.TryAddArtefact(_artefactsChosen[1], _losPieceChosen))
            {
                _freeItemOne.gameObject.SetActive(false);
                _freeItemTwo.gameObject.SetActive(false);
                _chooseText.gameObject.SetActive(false);
                return true;
            }
            else
            {
                //ERROR EFFECT FOR HAVING 3 ARTEFACTS IN POSSESSION
            }
        }
        return false;
    }

    private void SwapPositions()
    {
        (_freeItemTwo.transform.position, _freeItemOne.transform.position) = (_freeItemOne.transform.position, _freeItemTwo.transform.position);
    }

    public void ShowGuiPosChange()
    {
        _guiPosChange.gameObject.SetActive(true);
    }

    public void HideGuiPosChange()
    {
        _guiPosChange.gameObject.SetActive(false);
    }
}
