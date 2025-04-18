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
    [SerializeField] Suggestions SuggestionDialog;
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
    public int numberSuggestions = 0;
    public readonly float doubleClickThreshold = 0.3f;
    public bool ended = false;
    public bool isFocusPlayingGame = true;

    GameObject Vfx;
    List<ParticleSystem> VFXsLeafPool = new List<ParticleSystem>();

    void Awake() {
        GameManager.Instance.Controller = this;
        GameInit = GetComponent<GameInit>();
        GameGraft = GetComponent<GameGraft>();
        GameTutorial = GetComponent<GameTutorial>();
    }

    void Start() {
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

        Level[] levels = GetLevels();
        GameInit.InitGame(levels[currentLevel - 1], currentLevel);
        GoogleAds.Instance.Initialize();
    }

    Level[] GetLevels() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/levels");
        string json = jsonFile.text;
        Level[] levels = JsonConvert.DeserializeObject<Level[]>(json);
        totalLevels = levels.Length;
        return levels;
    }

    public void ShowHideSettingDialog() {
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

    public void OpenCloseSuggestionDialog() {
        SuggestionDialog.gameObject.SetActive(!SuggestionDialog.gameObject.activeInHierarchy);
    }

    public void ShowAdForSuggestion(int numberSugs) {
        SuggestionDialog.gameObject.SetActive(false);

        void OnSuccess() {
            numberSuggestions = numberSugs;
            InfoDialog.Open(new InfoDialog.Info {
                title = Helper.GetLocalizedValue("tapToCreature", args: new string[] { numberSugs.ToString() }),
                fontSize = 16,
                btnTitle = "Ok",
                OnClick = () => {
                    InfoDialog.Close();
                },
                canClose = false,
            });
            if (numberSugs == 3) {
                SuggestionDialog.Use03();
            }
            else {
                SuggestionDialog.Use01();
            }
        }

        void OnError() {
            InfoDialog.Open(new InfoDialog.Info {
                title = Helper.GetLocalizedValue("oppSomeError"),
                btnTitle = Helper.GetLocalizedValue("retry"),
                OnClick = () => {
                    InfoDialog.Close();
                    ShowAdForSuggestion(numberSugs);
                }
            });
        }

        if (numberSugs == 3) {
            GoogleAds.Instance.ShowReward(
                success: OnSuccess,
                error: OnError
            );
        }
        else {
            GoogleAds.Instance.ShowInterstitial(
                success: OnSuccess,
                error: OnError
            );
        }
    }

    public void OpenGoToLevelDialog(int level) {
        InfoDialog.Open(new InfoDialog.Info {
            title = Helper.GetLocalizedValue("goToLevel", new string[] { level.ToString() }),
            btnTitle = "Ok",
            OnClick = () => GoToLevel(level),
            sfx = SoundManager.SF.None
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
        return !LevelsDialog.activeInHierarchy && !ended && !SettingDialog.activeInHierarchy && !InfoDialog.gameObject.activeInHierarchy && !SuggestionDialog.gameObject.activeInHierarchy;
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

    public void SaveLevelStatus() {
        List<Matching> matching = new();
        foreach (var row in GameInit.Squares) {
            foreach (Square sq in row) {
                ItemController item = sq.ItemController;
                if (item == null) continue;
                int index = Array.FindIndex(GameInit.level.init_pos, p => p.Equals(item.Item.pos));
                bool notInitPos = index < 0;
                if (notInitPos) {
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

    IEnumerator EndGameCoroutine() {
        List<AudioSource> audios = new List<AudioSource>();
        Vfx = Instantiate(VFXsWinner, VFXsContainer);
        float duration = 0.6f;
        float elapsedTime = 0f;
        float deltaTime = 0.1f;
        while (elapsedTime <= duration) {
            audios.Add(SoundManager.Instance.PlaySF(SoundManager.SF.Pop_01));
            elapsedTime += deltaTime;
            yield return new WaitForSeconds(deltaTime);
        }
        yield return new WaitForSeconds(0.5f);
        audios.Add(SoundManager.Instance.PlaySF(
            Helper.GetRandomInArr(new SoundManager.SF[] { SoundManager.SF.Win01, SoundManager.SF.Win02, }), volumeScale: 0.67f)
        );
        InfoDialog.Open(new InfoDialog.Info {
            title = Helper.GetLocalizedValue(Helper.GetRandomInArr(new string[]{
                "perfect",
                "excellent",
            })),
            btnTitle = Helper.GetLocalizedValue("nextLevel"),
            OnClick = () => {
                foreach (AudioSource audio in audios) {
                    SoundManager.Instance.RemoveAudioSource(audio);
                }
                NextLevel();
            },
            canClose = false,
        });
    }

    void RemoveLevelStatus() {
        var level = GetPlayingLevel(currentLevel);
        if (level != null) {
            GameManager.Instance.gameState.playingLevels.Remove(level);
        }
    }
}
