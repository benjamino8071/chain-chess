using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElArtefactsUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    
    private GameObject _artefactsParent;
    
    private List<ElArtefactController> _artefactsControllers = new();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();

        _artefactsParent = elCreator.GetFirstObjectWithName(AllTagNames.Artefacts).gameObject;

        ArtefactsUIButtons artefactsUIButtons = _artefactsParent.GetComponent<ArtefactsUIButtons>();

        foreach ((Button, Button) button in artefactsUIButtons.GetButtonsAsList())
        {
            ElArtefactController artefactController = new ElArtefactController();
            artefactController.GameStart(elCreator);
            artefactController.SetButtonInstance(button.Item1, button.Item2);
            _artefactsControllers.Add(artefactController);
        }

        int lineOfSightIndex = 0;
        for (int i = 0; i < Creator.upgradeSo.artefactsChosen.Count; i++)
        {
            if (Creator.upgradeSo.artefactsChosen[i] == ArtefactTypes.EnemyLineOfSight)
            {
                _artefactsControllers[i].SetInUse(Creator.upgradeSo.artefactsChosen[i], false, Creator.upgradeSo.lineOfSightsChosen[lineOfSightIndex]);
                lineOfSightIndex++;
            }
            else
            {
                _artefactsControllers[i].SetInUse(Creator.upgradeSo.artefactsChosen[i], false);
            }
        }
    }

    public override void GameEarlyUpdate(float dt)
    {
        //_artefactsParent.SetActive(_playerSystem.GetState() == ElPlayerSystem.States.Idle);
        foreach (ElArtefactController elArtefactController in _artefactsControllers)
        {
            elArtefactController.GameEarlyUpdate(dt);
        }
    }

    /// <summary>
    /// There is the edge case where the player selects an artefact they already own
    /// Therefore we will have to make it 100% certain that when the player has the ability to select an artefact, it is NOT one that they already own
    /// </summary>
    /// <param name="artefact"></param>
    /// <param name="lineOfSight"></param>
    /// <returns></returns>
    public bool TryAddArtefact(ArtefactTypes artefact, Piece lineOfSight = Piece.NotChosen)
    {
        foreach (ElArtefactController artefactsController in _artefactsControllers)
        {
            if (!artefactsController.GetInUse())
            {
                artefactsController.SetInUse(artefact, true, lineOfSight);
                return true;
            }
        }

        return false;
    }

    public void RemoveArtefact(ArtefactTypes artefactType)
    {
        foreach (ElArtefactController artefactsController in _artefactsControllers)
        {
            if (artefactsController.GetArtefactType() == artefactType)
            {
                artefactsController.SetNotInUse();
                break;
            }
        }
    }

    public void HideAllSellButtons()
    {
        foreach (ElArtefactController artefactsController in _artefactsControllers)
        {
            artefactsController.HideSellButton();
        }
    }

    public void Show()
    {
        _artefactsParent.SetActive(true);
    }

    public void Hide()
    {
        _artefactsParent.SetActive(false);
    }
}
