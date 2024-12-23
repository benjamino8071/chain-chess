using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElArtefactsSystem : ElDependency
{
    private List<ElArtefactController> _artefactsControllers = new();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        GameObject artefactsParent = GameObject.FindWithTag("Artefacts");

        ArtefactsUIButtons artefactsUIButtons = artefactsParent.GetComponent<ArtefactsUIButtons>();

        foreach ((Button, Button) button in artefactsUIButtons.GetButtonsAsList())
        {
            ElArtefactController artefactController = new ElArtefactController();
            artefactController.GameStart(elCreator);
            artefactController.SetButtonInstance(button.Item1, button.Item2);
            _artefactsControllers.Add(artefactController);
        }
        
        for (int i = 0; i < Creator.playerSystemSo.artefacts.Count; i++)
        {
            if (i < Creator.playerSystemSo.lineOfSightsChosen.Count)
            {
                _artefactsControllers[i].SetInUse(Creator.playerSystemSo.artefacts[i], false, Creator.playerSystemSo.lineOfSightsChosen[i]);
            }
            else
            {
                _artefactsControllers[i].SetInUse(Creator.playerSystemSo.artefacts[i], false);
            }
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

    public void HideAllSellButtons()
    {
        foreach (ElArtefactController artefactsController in _artefactsControllers)
        {
            artefactsController.HideSellButton();
        }
    }
}
