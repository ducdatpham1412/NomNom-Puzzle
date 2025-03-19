using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] GameObject SettingDialog;

    [Header("Stats")]
    [SerializeField] int chessSize = 0;
    public Dictionary<string, Sprite> CreatureSprites = new Dictionary<string, Sprite>();

    [HideInInspector] public List<ChoicePos> ChoicesPos = new List<ChoicePos>();

    GameInit GameInit;

    void Awake() {
        GameInit = GetComponent<GameInit>();
    }

    void OnEnable() {
        GameInit.InitGame(size: chessSize);
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
