using UnityEngine;
using UnityEngine.UI;

public class ItemLevel : MonoBehaviour {
    [SerializeField] Text text;
    Button Btn;
    int level;
    bool isSet = false;

    public void SetLevel(int lv) {
        text.text = $"{Helper.GetLocalizedValue("level")}\n{lv}";
        level = lv;
        Btn = GetComponent<Button>();
        Image img = GetComponent<Image>();

        bool canPlay = level <= GameManager.Instance.Controller.levelStorage;
        bool isCurrentLevel = level == GameManager.Instance.Controller.currentLevel;

        if (!canPlay) {
            isSet = false;
            Btn.onClick.RemoveAllListeners();
            Btn.enabled = false;
            img.color = Helper.ColorFromHex("#656565");
            text.color = Color.white;
        }
        else if (isCurrentLevel) {
            isSet = false;
            Btn.onClick.RemoveAllListeners();
            Btn.enabled = false;
            img.color = Helper.ColorFromHex("#41FD38");
            text.color = Helper.ColorFromHex("#FFE200");
        }
        else {
            Btn.enabled = true;
            img.color = Color.white;
            text.color = Helper.ColorFromHex("#FFE200");
            if (!isSet) {
                isSet = true;
                Btn.onClick.AddListener(GoToLevel);
            }
        }
    }

    void GoToLevel() {
        GameManager.Instance.Controller.OpenGoToLevelDialog(level);
    }
}
