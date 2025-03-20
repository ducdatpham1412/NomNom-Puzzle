using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] GameObject SettingDialog;

    [Header("Stats")]
    [SerializeField] int chessSize = 0;
    public Dictionary<string, Sprite> CreatureSprites = new Dictionary<string, Sprite>();
    public GameInit GameInit;

    [HideInInspector] public List<ChoicePos> ChoicesPos = new List<ChoicePos>();


    void Awake() {
        GameInit = GetComponent<GameInit>();
    }

    void OnEnable() {
        GameInit.InitGame(size: chessSize);
    }

    void Start() {
        // TODO: Remove this LifeCycle, do it in WelcomeLogic
        GameManager.Instance.Initialize();
        SoundManager.Instance.PlayMusic(SoundManager.MusicSource.background);
    }

    public void ShowHideSettingDialog() {
        SettingDialog.SetActive(!SettingDialog.activeInHierarchy);
    }

    [Serializable]
    public class ChoicePos {
        public Vector3 localPos;
        public bool isEmpty;
    }
}
