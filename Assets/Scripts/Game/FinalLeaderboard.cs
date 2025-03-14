using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class FinalLeaderboard : MonoBehaviour {
    [Serializable]
    private class Player {
        public string id;
        public string name;
        public int points;
    }

    public void SetData(MatchState state) {
        Dictionary<string, Player> Dict = new Dictionary<string, Player>{
            {
                state.players[0].id,
                new Player {
                    id = state.players[0].id,
                    name = state.players[0].name,
                    points = 0,
                }
            },
            {
                state.players[1].id,
                new Player {
                    id = state.players[1].id,
                    name = state.players[1].name,
                    points = 0,
                }
            }
        };

        foreach (JObject room in state.rooms) {
            if (room["type"].ToString() == BaseRoom.Type.throw_egg.ToString()) {
                RoomThrowEgg roomThrowEgg = room.ToObject<RoomThrowEgg>();
                foreach (RoomThrowEgg.Player player in roomThrowEgg.players) {
                    Dict[player.id].points += player.point;
                }
            }
        }

        bool is0LessThen1 = Dict[state.players[0].id].points < Dict[state.players[1].id].points;
        bool isEqual = Dict[state.players[0].id].points == Dict[state.players[1].id].points;

        Player gold = is0LessThen1 ? Dict[state.players[1].id] : Dict[state.players[0].id];
        Player silver = is0LessThen1 ? Dict[state.players[0].id] : Dict[state.players[1].id];

        Transform Gold = transform.Find("Gold");
        Transform Silver = transform.Find("Silver");

        Gold.Find("Text").GetComponent<Text>().text = gold.name;
        Gold.Find("Points").GetComponent<Text>().text = gold.points.ToString();

        Silver.Find("Text").GetComponent<Text>().text = silver.name;
        Silver.Find("Points").GetComponent<Text>().text = silver.points.ToString();


        if (isEqual) {
            transform.Find("Result").gameObject.SetActive(false);
            Silver.Find("Icon").GetComponent<SVGImage>().sprite = Gold.Find("Icon").GetComponent<SVGImage>().sprite;
        }
        else {
            LocalizeStringEvent localize = transform.Find("Result").Find("Text").GetComponent<LocalizeStringEvent>();
            localize.StringReference.Arguments = new object[] { gold.name, gold.points - silver.points };
            localize.RefreshString();
        }
    }
}
