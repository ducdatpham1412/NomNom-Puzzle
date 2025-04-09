using UnityEngine;
using UnityEngine.UI;

public class ItemLevel : MonoBehaviour {
    [SerializeField] Text text;
    Button Btn;
    int level;

    static ColorConfigs colorConfigs = new ColorConfigs {
        gray = Helper.ColorFromHex("#656565"),
        green = Helper.ColorFromHex("#41FD38"),
        yellow = Helper.ColorFromHex("#FFE200"),
        orange = Helper.ColorFromHex("#FABE2E"),
    };

    public void SetLevel(int lv) {
        text.text = $"{Helper.GetLocalizedValue("level")}\n{lv}";
        level = lv;
        Btn = GetComponent<Button>();
        Image img = GetComponent<Image>();

        bool canPlay = level <= GameManager.Instance.Controller.levelStorage;
        bool isCurrentLevel = level == GameManager.Instance.Controller.currentLevel;

        GameState.PlayingLevel playingLevel = GameManager.Instance.Controller.GetPlayingLevel(lv);
        bool isPlaying = playingLevel != null;

        if (!canPlay) {
            Btn.onClick.RemoveListener(GoToLevel);
            Btn.enabled = false;
            img.color = colorConfigs.gray;
            text.color = Color.white;
        }
        else if (isCurrentLevel) {
            Btn.onClick.RemoveListener(GoToLevel);
            Btn.enabled = false;
            img.color = colorConfigs.green;
            text.color = colorConfigs.yellow;
        }
        else {
            Btn.enabled = true;
            img.color = isPlaying ? colorConfigs.orange : Color.white;
            text.color = colorConfigs.yellow;
            Btn.onClick.AddListener(GoToLevel);
        }
    }

    void GoToLevel() {
        GameManager.Instance.Controller.OpenGoToLevelDialog(level);
    }

    class ColorConfigs {
        public Color gray;
        public Color green;
        public Color yellow;
        public Color orange;
    }
}
