using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ItemHistory : MonoBehaviour {
    public Text GameStatus;
    public Text EggCollected;
    public Text Date;

    public void Initialize(GameHistory gameHistory) {
        GameHistory.Room.Player me = null;
        GameHistory.Room.Player enemy = null;

        foreach (var player in gameHistory.rooms[0].players) {
            if (player.id == GameManager.Instance.appState.profile.id) {
                me = player;
            }
            else {
                enemy = player;
            }
        }

        string key = "victory";
        GameStatus.color = Helper.ColorFromHex(Configs.Color.green);
        if (me.point == enemy.point) {
            key = "tie";
            GameStatus.color = Helper.ColorFromHex(Configs.Color.yellow);
            EggCollected.text = $"{Helper.GetLocalizedValue(LocalizationManager.Table.Game, "eggCollected")}: 0";
        }
        else if (me.point < enemy.point) {
            key = "lose";
            GameStatus.color = Color.red;
            EggCollected.text = $"{Helper.GetLocalizedValue(LocalizationManager.Table.Game, "eggCollected")}: 0";
        }
        else {
            EggCollected.text = $"{Helper.GetLocalizedValue(LocalizationManager.Table.Game, "eggCollected")}: {me.point - enemy.point}";
        }
        GameStatus.text = Helper.GetLocalizedValue(LocalizationManager.Table.Game, key);

        DateTime now = new DateTime();
        Date.text = now.ToString("H:mm, dd/MM/yyyy");
    }
}
