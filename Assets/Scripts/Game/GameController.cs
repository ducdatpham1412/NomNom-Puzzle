using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GameController : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] GameObject SettingDialog;
    [SerializeField] GameObject LevelsDialog;
    [SerializeField] GameObject GoToLevelDialog;
    [SerializeField] GameObject NextLevelDialog;
    [SerializeField] Transform VFXsContainer;
    public SpriteRenderer ChoicesBoardBorder;

    [Header("Prefabs")]
    [SerializeField] GameObject VFXLeaf;
    [SerializeField] Texture2D[] LeafTextures;
    [SerializeField] GameObject VFXsWinner;

    [HideInInspector] public GameInit GameInit;
    [HideInInspector] public GameGraft GameGraft;

    [Header("Stats")]
    public int currentLevel;
    public int totalLevels;
    public readonly float doubleClickThreshold = 0.3f;
    GameObject Vfx;

    bool ended = false;
    List<ParticleSystem> VFXsLeafPool = new List<ParticleSystem>();

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

    public void EndGame() {
        ended = true;
        currentLevel++;
        Storage.SET(Storage.Key.currentLevel, currentLevel.ToString());
        StartCoroutine(EndGameCoroutine());
    }

    public void GoToLevel(int level) {
        Debug.Log("Go to level" + level);
    }

    public bool ShouldHandlePan() {
        return !LevelsDialog.activeInHierarchy && !ended && !SettingDialog.activeInHierarchy && !GoToLevelDialog.activeInHierarchy;
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
        NextLevelDialog.SetActive(false);
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

    IEnumerator EndGameCoroutine() {
        Vfx = Instantiate(VFXsWinner, VFXsContainer);
        // TODO: Playing sound winner
        yield return new WaitForSeconds(2f);
        NextLevelDialog.SetActive(true);
    }
}
