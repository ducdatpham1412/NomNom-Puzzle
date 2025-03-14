using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Netcode;
using UnityEngine;

public class ConnectManager : NetworkBehaviour {
    public override void OnNetworkSpawn() {
        if (IsServer) {
            SocketManager.OnHandleData += S_Disconnected;
        }
    }

    public override void OnNetworkDespawn() {
        if (IsServer) {
            SocketManager.OnHandleData -= S_Disconnected;
        }
    }

    void S_Disconnected(JObject evt) {
        string eventType = evt["type"].ToString();
        if (eventType == "disconnected") {
            S_EndGame(MatchState.Status.disconnected);
        }
    }

    public async void S_EndGame(MatchState.Status status) {
        GameState gameState = GameManager.Instance.gameState;
        try {
            gameState.matchState.status = status.ToString();
            await ApiManager.POST<JObject>(
                path: "/game/history",
                data: new Dictionary<string, object> {
                        {"match_state", JsonConvert.SerializeObject(gameState.matchState)}
                }
            );
        }
        catch (Exception e) {
            Debug.Log($"ERROR SAVING MATCH_STATE: {JObject.FromObject(gameState.matchState)} | Error: {e.Message}");
        }
        finally {
            Debug.Log($"SERVER QUITTED | TIME: {DateTime.Now}");
            Application.Quit();
        }
    }

    // TODO: Handle ReConnected
}
