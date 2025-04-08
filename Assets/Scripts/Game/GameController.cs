using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

using Matching = GameState.PlayingLevel.Matching;

public class GameController : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] GameObject SettingDialog;
    [SerializeField] GameObject LevelsDialog;
    public InfoDialog InfoDialog;
    public ToolTip ToolTip;
    [SerializeField] Transform VFXsContainer;
    public SpriteRenderer ChoicesBoardBorder;
    public ButtonManager PlayAgainButton;

    [Header("Prefabs")]
    [SerializeField] GameObject VFXLeaf;
    [SerializeField] Texture2D[] LeafTextures;
    [SerializeField] GameObject VFXsWinner;

    [HideInInspector] public GameInit GameInit;
    [HideInInspector] public GameGraft GameGraft;
    [HideInInspector] public GameTutorial GameTutorial;

    [Header("Stats")]
    public int currentLevel;
    public int levelStorage;
    public int totalLevels;
    public readonly float doubleClickThreshold = 0.3f;
    public bool ended = false;

    GameObject Vfx;
    List<ParticleSystem> VFXsLeafPool = new List<ParticleSystem>();

    void Awake() {
        GameManager.Instance.Controller = this;
        GameInit = GetComponent<GameInit>();
        GameGraft = GetComponent<GameGraft>();
        GameTutorial = GetComponent<GameTutorial>();
    }

    void Start() {
        Level[] levels = GetLevels();
        int? lv = Storage.GETStruct<int>(Storage.Key.currentLevel);
        if (lv == null) {
            GameTutorial.StartTutorial();
            Storage.SET(Storage.Key.currentLevel, "1");
            currentLevel = 1;
        }
        else {
            currentLevel = (int)lv;
        }
        levelStorage = currentLevel;
        GameInit.InitGame(levels[currentLevel - 1], currentLevel);
    }

    Level[] GetLevels() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/levels");
        string json = jsonFile.text;
        Level[] levels = JsonConvert.DeserializeObject<Level[]>(json);
        totalLevels = levels.Length;
        return levels;
    }

    public void ShowHideSettingDialog() {
        if (GameTutorial.isTutorial && GameTutorial.StepGameObject != SettingDialog.gameObject) return;
        SettingDialog.SetActive(!SettingDialog.activeInHierarchy);
    }

    public void ShowHideLevelsDialog() {
        if (GameTutorial.isTutorial && GameTutorial.StepGameObject != LevelsDialog.gameObject) return;
        LevelsDialog.SetActive(!LevelsDialog.activeInHierarchy);
    }

    public void EndGame() {
        ended = true;
        RemoveLevelStatus();
        currentLevel++;
        if (currentLevel > levelStorage) {
            levelStorage = currentLevel;
            Storage.SET(Storage.Key.currentLevel, currentLevel.ToString());
        }
        StartCoroutine(EndGameCoroutine());
    }

    public void OpenGoToLevelDialog(int level) {
        InfoDialog.Open(new InfoDialog.Info {
            title = Helper.GetLocalizedValue("goToLevel", new string[] { level.ToString() }),
            btnTitle = "Ok",
            OnClick = () => GoToLevel(level),
        });
    }

    public void CloseGoToLevelDialog() {
        InfoDialog.Close();
    }

    public void GoToLevel(int level) {
        if (ended) {
            ended = false;
        }
        else {
            SaveLevelStatus();
        }
        currentLevel = level;
        LevelsDialog.SetActive(false);
        InfoDialog.Close();
        Level[] levels = GetLevels();
        GameInit.InitGame(levels[currentLevel - 1], currentLevel);
    }

    public bool ShouldHandlePan() {
        return !LevelsDialog.activeInHierarchy && !ended && !SettingDialog.activeInHierarchy && !InfoDialog.gameObject.activeInHierarchy;
    }

    public void PlayVFXLeaf(Vector3 pos) {
        List<ParticleSystem> readyVFXs = VFXsLeafPool.FindAll(v => v.isStopped);
        if (readyVFXs.Count == 0) {
            GameObject newVFX = Instantiate(VFXLeaf, pos, Quaternion.identity, VFXsContainer);
            ParticleSystem Ps = newVFX.GetComponent<ParticleSystem>();
            ParticleSystemRenderer PsRenderer = Ps.GetComponent<ParticleSystemRenderer>();
            PsRenderer.material.SetTexture("_MainTex", Helper.GetRandomInArr(LeafTextures));
            VFXsLeafPool.Add(Ps);
            Ps.Play();
        }
        else {
            ParticleSystem Ps = Helper.GetRandomInArr(readyVFXs.ToArray());
            Ps.gameObject.transform.position = pos;
            Ps.Play();
        }
        // TODO: Adding sound
    }

    public void NextLevel() {
        ended = false;
        Level[] levels = GetLevels();
        InfoDialog.Close();
        if (currentLevel == levels.Length) {
            // TODO: Congratulation
        }
        else {
            if (Vfx != null) {
                Destroy(Vfx);
                Vfx = null;
            }
            GameInit.InitGame(levels[currentLevel - 1], currentLevel);
        }
    }

    public void InformationScene() {
        Navigator.Instance.NavigateTo(Navigator.Scene.InformationScene, LoadSceneMode.Additive);
    }

    public GameState.PlayingLevel GetPlayingLevel(int lv) {
        GameState.PlayingLevel level = GameManager.Instance.gameState.playingLevels.Find(l => l.level == lv);
        return level;
    }

    public void PlayAgain() {
        PlayAgainButton.gameObject.SetActive(false);
        ended = false;
        GameState.PlayingLevel level = GetPlayingLevel(currentLevel);
        if (level != null) {
            GameManager.Instance.gameState.playingLevels.Remove(level);
        }
        GameManager.Instance.gameState.playingLevels.Add(new GameState.PlayingLevel {
            level = currentLevel,
            matchings = new List<Matching>(),
        });
        GameInit.InitGame(GameInit.level, currentLevel);
    }

    IEnumerator EndGameCoroutine() {
        Vfx = Instantiate(VFXsWinner, VFXsContainer);
        // TODO: Playing sound winner

        yield return new WaitForSeconds(2f);
        InfoDialog.Open(new InfoDialog.Info {
            title = Helper.GetLocalizedValue(Helper.GetRandomInArr(new string[]{
                "perfect",
                "excellent",
            })),
            btnTitle = Helper.GetLocalizedValue("nextLevel"),
            OnClick = NextLevel,
            canClose = false,
        });
    }

    void RemoveLevelStatus() {
        var level = GetPlayingLevel(currentLevel);
        if (level != null) {
            GameManager.Instance.gameState.playingLevels.Remove(level);
        }
    }

    void SaveLevelStatus() {
        List<Matching> matching = new();
        foreach (var row in GameInit.Squares) {
            foreach (Square sq in row) {
                ItemController item = sq.GetItemController();
                if (item == null) continue;
                int index = Array.FindIndex(GameInit.level.init_pos, p => p.Equals(item.Item.pos));
                if (index < 0) {
                    matching.Add(new Matching {
                        itemPos = item.Item.pos,
                        squarePos = sq.Pos,
                    });
                }
            }
        }
        GameState.PlayingLevel level = GetPlayingLevel(currentLevel);

        if (level != null) {
            level.matchings = matching;
        }
        else {
            GameManager.Instance.gameState.playingLevels.Add(new GameState.PlayingLevel {
                level = currentLevel,
                matchings = matching,
            });
        }
    }
}
