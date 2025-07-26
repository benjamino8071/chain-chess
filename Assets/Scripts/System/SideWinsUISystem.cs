using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SideWinsUISystem : Dependency
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;

    private Transform _sideWinsUI;

    private Image _sideWinsBackgroundImage;
    
    private TMP_Text _sideWinsTitleText;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();

        _sideWinsUI = creator.GetFirstObjectWithName(AllTagNames.SideWins).transform;

        _sideWinsBackgroundImage = _sideWinsUI.GetComponent<Image>();
        
        Transform sideWinsTitleText = creator.GetChildObjectByName(_sideWinsUI.gameObject, AllTagNames.Text);
        _sideWinsTitleText = sideWinsTitleText.GetComponent<TMP_Text>();
        
        Transform retryButtonTf = creator.GetChildObjectByName(_sideWinsUI.gameObject, AllTagNames.Retry);
        Button retryButton = retryButtonTf.GetComponent<Button>();
        retryButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform exitButtonTf = creator.GetChildObjectByName(_sideWinsUI.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            SceneManager.LoadScene("Main_Menu_Scene");
        });
        
        Hide();
    }
    
    public void Show(PieceColour winningSide)
    {
        string winningSideName = winningSide == PieceColour.White ? "White" : "Black";
        _sideWinsTitleText.text = $"<bounce a=0.1 f=0.3>{winningSideName} Wins";

        Color winningSideColour = winningSide == PieceColour.White
            ? Creator.piecesSo.whiteColor
            : Creator.piecesSo.blackColor;
        winningSideColour.a = _sideWinsBackgroundImage.color.a;
        _sideWinsBackgroundImage.color = winningSideColour;
        
        _sideWinsUI.gameObject.SetActive(true);
        _audioSystem.PlayLevelCompleteSfx();
    }

    public void Hide()
    {
        _sideWinsUI.gameObject.SetActive(false);
    }
}
