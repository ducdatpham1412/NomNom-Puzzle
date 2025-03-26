using Newtonsoft.Json;
using UnityEngine;

public class GameController : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] GameObject SettingDialog;
    public SpriteRenderer ChoicesBoardBorder;

    [Header("Components")]
    public GameInit GameInit;
    public GameGraft GameGraft;

    [Header("Stats")]
    [SerializeField] int currentLevel;


    void Awake() {
        GameInit = GetComponent<GameInit>();
        GameGraft = GetComponent<GameGraft>();
    }

    void Start() {
        GameManager.Instance.Initialize(); // TODO: Remove this LifeCycle, do it in WelcomeLogic
        Level[] levels = GetLevels();
        currentLevel = Storage.GET<int>(Storage.Key.currentLevel);
        GameInit.InitGame(levels[currentLevel], currentLevel + 1);
    }

    Level[] GetLevels() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/levels");
        string json = jsonFile.text;
        Level[] levels = JsonConvert.DeserializeObject<Level[]>(json);
        return levels;
    }

    public void ShowHideSettingDialog() {
        SettingDialog.SetActive(!SettingDialog.activeInHierarchy);
    }

    public void NextLevel() {
        currentLevel++;
        Storage.SET(Storage.Key.currentLevel, currentLevel.ToString());
        Level[] levels = GetLevels();
        GameInit.InitGame(levels[currentLevel], currentLevel + 1);
    }
}
