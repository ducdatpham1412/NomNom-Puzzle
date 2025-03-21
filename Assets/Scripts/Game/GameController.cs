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
    [SerializeField] Vector2 size = new Vector2(3, 3);


    void Awake() {
        GameInit = GetComponent<GameInit>();
        GameGraft = GetComponent<GameGraft>();
    }

    void OnEnable() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/levels");
        string json = jsonFile.text;
        Level[] levels = JsonConvert.DeserializeObject<Level[]>(json);
        GameInit.InitGame(levels[0]);
    }

    void Start() {
        // TODO: Remove this LifeCycle, do it in WelcomeLogic
        GameManager.Instance.Initialize();
        // SoundManager.Instance.PlayMusic(SoundManager.MusicSource.background);
    }

    public void ShowHideSettingDialog() {
        SettingDialog.SetActive(!SettingDialog.activeInHierarchy);
    }
}
