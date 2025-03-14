using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCardManager : MonoBehaviour {
    public Text Title;
    public GameObject PlayerLeft;
    public GameObject PlayerRight;
    public Text TextScore;
    public Text TextStatus;
    public string roomID;

    string roomType;


    private void SetPlayerUI(GameObject Player, MatchState.Player data) {
        ImageLoader loader = Player.transform.Find("Avatar").transform.Find("Avatar").GetComponent<ImageLoader>();
        loader.SetImageUrl(data.avatar);
        Text Name = Player.transform.Find("Name").GetComponent<Text>();
        Name.text = data.name;
        if (data.id == GameManager.Instance.appState.profile.id) {
            Name.color = Helper.ColorFromHex(Configs.Color.green01);
        }
    }

    private string GetText(string key) {
        return Helper.GetLocalizedValue(LocalizationManager.Table.Game, key);
    }

    private string GetGameByType(string type) {
        if (type == BaseRoom.Type.throw_egg.ToString()) {
            return GetText("throwEgg");
        }
        if (type == BaseRoom.Type.math.ToString()) {
            return GetText("solveMath");
        }
        if (type == BaseRoom.Type.flappy_egg.ToString()) {
            return GetText("flappyEgg");
        }
        return "";
    }

    public void Initialize(string _roomID) {
        roomID = _roomID;
        MatchState matchState = GameManager.Instance.gameState.matchState;
        List<JObject> rooms = matchState.rooms;
        int index = rooms.FindIndex(r => r["id"].ToString() == roomID);
        JObject room = rooms[index];

        roomType = room["type"].ToString();

        string player1_id = "";
        int player1_point = 0;

        string player2_id = "";
        int player2_point = 0;

        string status = "";

        bool isValid = false;

        if (roomType == BaseRoom.Type.throw_egg.ToString()) {
            RoomThrowEgg roomObject = room.ToObject<RoomThrowEgg>();
            player1_id = roomObject.players[0].id;
            player1_point = roomObject.players[0].point;
            player2_id = roomObject.players[1].id;
            player2_point = roomObject.players[1].point;
            status = roomObject.status;
            isValid = true;
        }
        else if (roomType == BaseRoom.Type.math.ToString()) {
            RoomMath roomObject = room.ToObject<RoomMath>();
            player1_id = roomObject.players[0].id;
            player1_point = roomObject.players[0].point;
            player2_id = roomObject.players[1].id;
            player2_point = roomObject.players[1].point;
            status = roomObject.status;
            isValid = true;
        }

        if (!isValid) {
            return;
        }

        Title.text = $"{GetText("round")} {index + 1}: {GetGameByType(roomType)}";

        MatchState.Player Player01 = matchState.players.Find(p => p.id == player1_id);
        MatchState.Player Player02 = matchState.players.Find(p => p.id == player2_id);

        int sum01 = (Player01.id + Player01.name).ToIntArray().Sum();
        int sum02 = (Player02.id + Player02.name).ToIntArray().Sum();
        bool isLess = sum01 < sum02;

        MatchState.Player left = isLess ? Player01 : Player02;
        MatchState.Player right = isLess ? Player02 : Player01;

        int leftPoint = isLess ? player1_point : player2_point;
        int rightPoint = isLess ? player2_point : player1_point;

        SetPlayerUI(PlayerLeft, left);
        SetPlayerUI(PlayerRight, right);

        TextScore.text = $"{leftPoint}  -  {rightPoint}";
        if (status == BaseRoom.Status.ended.ToString()) {
            TextStatus.text = GetText("ended");
        }
    }

    private IEnumerator _CountDownToBegin(int seconds) {
        GetComponent<Outline>().enabled = true;
        string BeginText = GetText("beginIn");
        for (int i = seconds; i >= 1; i--) {
            string count = i > 10 ? $"{i}" : $"0{i}";
            TextStatus.text = $"{BeginText} {count}";
            yield return new WaitForSeconds(1f);
        }
        TextStatus.text = GetText("start");
        yield return new WaitForSeconds(1f);
        if (roomType == BaseRoom.Type.throw_egg.ToString()) {
            Navigator.Instance.NetworkLoad(Navigator.Scene.GameThrowEgg);
        }
        else if (roomType == BaseRoom.Type.math.ToString()) {
            Navigator.Instance.NetworkLoad(Navigator.Scene.GameMath);
        }
    }

    public void CountDownToBegin(int seconds = 3) {
        StartCoroutine(_CountDownToBegin(seconds));
    }
}
