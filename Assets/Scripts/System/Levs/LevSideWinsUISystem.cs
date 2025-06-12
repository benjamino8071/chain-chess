using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevSideWinsUISystem : LevDependency
{
    private LevAudioSystem _audioSystem;
    
    private Transform _sideWinsUI;

    private Image _sideWinsBackgroundImage;
    
    private TMP_Text _sideWinsTitleText;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);
        
        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        
        _sideWinsUI = levCreator.GetFirstObjectWithName(AllTagNames.SideWins).transform;

        _sideWinsBackgroundImage = _sideWinsUI.GetComponent<Image>();
        
        Transform sideWinsTitleText = levCreator.GetChildObjectByName(_sideWinsUI.gameObject, AllTagNames.Text);
        _sideWinsTitleText = sideWinsTitleText.GetComponent<TMP_Text>();
        
        Transform retryButtonTf = levCreator.GetChildObjectByName(_sideWinsUI.gameObject, AllTagNames.Retry);
        Button retryButton = retryButtonTf.GetComponent<Button>();
        retryButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        
        Transform exitButtonTf = levCreator.GetChildObjectByName(_sideWinsUI.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            SceneManager.LoadScene("MainMenuScene");
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
