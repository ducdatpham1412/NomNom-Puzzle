using Newtonsoft.Json;
using UnityEngine;

public class GameController : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] GameObject SettingDialog;
    [SerializeField] GameObject LevelsDialog;
    [SerializeField] GameObject GoToLevelDialog;
    public SpriteRenderer ChoicesBoardBorder;

    [HideInInspector] public GameInit GameInit;
    [HideInInspector] public GameGraft GameGraft;

    [Header("Stats")]
    public int currentLevel;
    public int totalLevels;


    void Awake() {
        GameManager.Instance.Controller = this;
        GameInit = GetComponent<GameInit>();
        GameGraft = GetComponent<GameGraft>();
    }

    void Start() {
        Level[] levels = GetLevels();
        int? lv = Storage.GET<int>(Storage.Key.currentLevel);
        if (lv == null) {
            Storage.SET(Storage.Key.currentLevel, "1");
            currentLevel = 1;
        }
        else {
            currentLevel = (int)lv;
        }
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
        SettingDialog.SetActive(!SettingDialog.activeInHierarchy);
    }

    public void ShowHideLevelsDialog() {
        LevelsDialog.SetActive(!LevelsDialog.activeInHierarchy);
    }

    public void NextLevel() {
        currentLevel++;
        Storage.SET(Storage.Key.currentLevel, currentLevel.ToString());
        Level[] levels = GetLevels();
        GameInit.InitGame(levels[currentLevel - 1], currentLevel);
    }

    public void GoToLevel(int level) {
        Debug.Log("Go to level" + level);
    }
}
