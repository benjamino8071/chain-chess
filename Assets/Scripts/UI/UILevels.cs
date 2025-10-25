using System.Collections.Generic;
using Michsky.MUIP;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILevels : UIPanel
{
    private TurnSystem _turnSystem;
    
    private List<LevelInfo> _levelInfos;
    private class LevelInfo
    {
        public ButtonManager levelButton;
        public Image starOneImage;
        public Image starTwoImage;
        public Image starThreeImage;
        public Level level;
    }
    
    private Dictionary<Vector2Int, Image> _previewPieceImages = new(64);

    private ButtonManager _playButton;
    
    private TextMeshProUGUI _sectionText;
    private TextMeshProUGUI _levelText;
    
    private Transform _levelPreviewParent;

    private Image _tickImage;
    
    private Level _levelToLoad;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _tickImage = Creator.GetChildComponentByName<Image>(_panel.gameObject, AllTagNames.ImageTick);
        
        _levelInfos = new(10);
        
        _sectionText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.SectionText);
        _levelText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.LevelText);
        
        _playButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonPlay);
        _playButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUISignificantClickSfx();
            
            _uiSystem.ShowLeftBotSideUI(AllTagNames.UICurrentLevel);
            
            _turnSystem.LoadLevelRuntime(_levelToLoad);
        });
        
        _levelPreviewParent = Creator.GetChildComponentByName<Transform>(_panel.gameObject, AllTagNames.LevelPreviewParent);
        RectTransform levelPreviewBoard = Creator.GetChildComponentByName<RectTransform>(_levelPreviewParent.gameObject, AllTagNames.LevelPreviewBoard);

        float halfWidth = levelPreviewBoard.rect.width / 2;
        float halfHeight = levelPreviewBoard.rect.height / 2;
        float singleTileWidth = levelPreviewBoard.rect.width / 8;
        float singleTileHeight = levelPreviewBoard.rect.height / 8;
        
        for (int x = 1; x < 9; x++)
        {
            float xValue = -halfWidth + (singleTileWidth / 2) + (singleTileWidth * (x - 1));
            
            for (int y = 1; y < 9; y++)
            {
                float yValue = -halfHeight + (singleTileHeight / 2) + (singleTileHeight * (y - 1));
                
                GameObject previewPieceGo = Creator.InstantiateGameObjectWithParent(Creator.imagePrefab, _levelPreviewParent);
                RectTransform previewPiece = previewPieceGo.GetComponent<RectTransform>();
                previewPiece.sizeDelta = new(singleTileWidth, singleTileHeight);
                previewPiece.localPosition = new(xValue, yValue);
                previewPiece.localScale = new float3(0.75f);
                Image previewPieceImage = previewPieceGo.GetComponent<Image>();
                previewPieceGo.SetActive(false);
                
                _previewPieceImages.Add(new(x, y), previewPieceImage);
            }
        }

        int levelNumber = 1;
        List<LevelButtonRefs> levelButtonRefs = Creator.GetChildComponentsByName<LevelButtonRefs>(_panel.gameObject, AllTagNames.LevelButtonRefs);
        foreach (LevelButtonRefs levelButtonRef in levelButtonRefs)
        {
            LevelInfo levelInfo = new()
            {
                levelButton = levelButtonRef.button,
                starOneImage = levelButtonRef.star1Image,
                starTwoImage = levelButtonRef.star2Image,
                starThreeImage = levelButtonRef.star3Image,
            };
            levelInfo.levelButton.SetText($"{levelNumber}");
            levelNumber++;
            
            levelInfo.levelButton.onClick.AddListener(() =>
            {
                _audioSystem.PlayUIAltClickSfx();
                
                _levelToLoad = levelInfo.level;
                
                LoadLevelPreview(_levelToLoad);
                _levelText.gameObject.SetActive(true);
                _playButton.gameObject.SetActive(true);
            });
            
            levelInfo.starOneImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starOneImage.color = Creator.levelCompleteSo.starHollowColor;

            levelInfo.starTwoImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starTwoImage.color = Creator.levelCompleteSo.starHollowColor;

            levelInfo.starThreeImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starThreeImage.color = Creator.levelCompleteSo.starHollowColor;
            
            _levelInfos.Add(levelInfo);
        }
    }

    public override void GameUpdate(float dt)
    {
        
    }

    public override void Show()
    {
        _playButton.gameObject.SetActive(false);
        ResetLevelPreview();
        LoadLevelPreview(_turnSystem.currentLevel);
        
        base.Show();
    }

    public void SetLevels(int section)
    {
        _sectionText.text = $"Section {section}";
        
        int starCountInSection = 0;
        foreach (LevelSaveData levelSaveData in Creator.saveDataSo.levels)
        {
            if (levelSaveData.section != section)
            {
                continue;
            }
                
            starCountInSection += levelSaveData.starsScored;
        }

        SectionData sectionData = Creator.levelsSo.GetSection(section);
        int totalStarsInSection = 3 * sectionData.levels.Count;
        
        _tickImage.gameObject.SetActive(starCountInSection == totalStarsInSection);
        
        for (int i = 0; i < _levelInfos.Count; i++)
        {
            LevelInfo levelInfo = _levelInfos[i];

            levelInfo.starOneImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starOneImage.color = Creator.levelCompleteSo.starHollowColor;
            
            levelInfo.starTwoImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starTwoImage.color = Creator.levelCompleteSo.starHollowColor;
            
            levelInfo.starThreeImage.sprite = Creator.levelCompleteSo.starHollowSprite;
            levelInfo.starThreeImage.color = Creator.levelCompleteSo.starHollowColor;
            
            if (i > sectionData.levels.Count - 1)
            {
                levelInfo.levelButton.transform.parent.gameObject.SetActive(false);
                _levelInfos[i].levelButton.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                Level level = sectionData.levels[i];
                
                levelInfo.level = level;
                levelInfo.levelButton.transform.parent.gameObject.SetActive(true);

                foreach (LevelSaveData levelSaveData in Creator.saveDataSo.levels)
                {
                    if (levelSaveData.section != level.section || levelSaveData.level != level.level)
                    {
                        continue;
                    }
                    
                    bool oneStar = levelSaveData.score <= level.star1Score;
                    bool twoStar = levelSaveData.score <= level.star2Score;
                    bool threeStar = levelSaveData.score <= level.star3Score;
                    
                    levelInfo.starOneImage.sprite = oneStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
                    levelInfo.starOneImage.color = oneStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
        
                    levelInfo.starTwoImage.sprite = twoStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
                    levelInfo.starTwoImage.color = twoStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;

                    levelInfo.starThreeImage.sprite = threeStar ? Creator.levelCompleteSo.starFilledSprite : Creator.levelCompleteSo.starHollowSprite;
                    levelInfo.starThreeImage.color = threeStar ? Creator.levelCompleteSo.starFilledColor : Creator.levelCompleteSo.starHollowColor;
                    
                    break;
                }
                
                _levelInfos[i].level = sectionData.levels[i];
                _levelInfos[i].levelButton.transform.parent.gameObject.SetActive(true);
            }
        }
    }

    private void LoadLevelPreview(Level level)
    {
        ResetLevelPreview();
        
        foreach (StartingPieceSpawnData startingPieceSpawnData in level.positions)
        {
            Vector2Int position = new((int)startingPieceSpawnData.position.x, (int)startingPieceSpawnData.position.y);
            _previewPieceImages[position].sprite = GetSprite(startingPieceSpawnData.piece);
            if (startingPieceSpawnData.ability == PieceAbility.None)
            {
                _previewPieceImages[position].color = startingPieceSpawnData.colour == PieceColour.White ? Creator.piecesSo.whiteColor : Creator.piecesSo.blackColor;
            }
            else
            {
                _previewPieceImages[position].material = GetMaterial(startingPieceSpawnData.ability);
            }
            _previewPieceImages[position].gameObject.SetActive(true);
        }
        
        _levelText.text = $"Level {level.section} - {level.level}";
        _levelPreviewParent.gameObject.SetActive(true);
    }

    private void ResetLevelPreview()
    {
        foreach (Image image in _previewPieceImages.Values)
        {
            image.gameObject.SetActive(false);
            image.sprite = null;
        }
    }
    
    private Sprite GetSprite(Piece piece)
    {
        switch (piece)
        {
            case Piece.Pawn:
                return Creator.piecesSo.pawn;
            case Piece.Rook:
                return Creator.piecesSo.rook;
            case Piece.Knight:
                return Creator.piecesSo.knight;
            case Piece.Bishop:
                return Creator.piecesSo.bishop;
            case Piece.Queen:
                return Creator.piecesSo.queen;
            case Piece.King:
                return Creator.piecesSo.king;
        }

        return null;
    }

    private Material GetMaterial(PieceAbility ability)
    {
        switch (ability)
        {
            case PieceAbility.None:
            {
                return Creator.piecesSo.noneMat;
            }
            case PieceAbility.Resetter:
            {
                return Creator.piecesSo.resetterMat;
            }
            case PieceAbility.MustMove:
            {
                return Creator.piecesSo.mustMoveMat;
            }
            case PieceAbility.Multiplier:
            {
                return Creator.piecesSo.multiplierMat;

            }
            case PieceAbility.CaptureLover:
            {
                return Creator.piecesSo.captureLoverMat;
            }
            case PieceAbility.StopTurn:
            {
                return Creator.piecesSo.stopTurnMat;
            }
            case PieceAbility.AlwaysMove:
            {
                return Creator.piecesSo.alwaysMoveMat;
            }
        }

        return null;
    }

    private Color GetColour(PieceAbility ability, PieceColour pieceColour)
    {
        switch (ability)
        {
            case PieceAbility.None:
            {
                return pieceColour == PieceColour.White ? Creator.piecesSo.whiteColor : Creator.piecesSo.blackColor;
            }
            case PieceAbility.Resetter:
            {
                return Creator.piecesSo.resetterColor;
            }
            case PieceAbility.MustMove:
            {
                return Creator.piecesSo.mustMoveColor;
            }
            case PieceAbility.Multiplier:
            {
                return Creator.piecesSo.multiplierColor;

            }
            case PieceAbility.CaptureLover:
            {
                return Creator.piecesSo.captureLoverColor;
            }
            case PieceAbility.StopTurn:
            {
                return Creator.piecesSo.stopTurnColor;
            }
            case PieceAbility.AlwaysMove:
            {
                return Creator.piecesSo.alwaysMoveColor;
            }
        }

        return default;
    }
}
