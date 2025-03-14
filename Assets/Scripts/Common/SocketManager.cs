using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class SocketManager {
    private static UdpClient udpClient;
    private static object lastData;
    public static event Action<JObject> OnHandleData;
    public static bool connected = false;

    static SocketManager() {
        udpClient = new UdpClient();
        udpClient.Connect(Configs.Env.socket_url, 5000);
        Task.Run(HandleState);
    }

    public static void Connect() {
        __Send(new Event.Send.Connect {
            player_id = GameManager.Instance.appState.profile.id
        });
    }

    static void HandleConnect(JObject state) {
        connected = true;
        ClientValue client = JsonConvert.DeserializeObject<ClientValue>(state["data"].ToString());
        GameManager.Instance.UpdateAppState(state => {
            state.client = client;
            return state;
        });
        if (lastData != null) {
            __Send(lastData);
            lastData = null;
        }
    }

    private static async void HandleState() {
        while (true) {
            try {
                var res = await udpClient.ReceiveAsync();
                string json = Encoding.UTF8.GetString(res.Buffer);
                // Deserialize and invoke event
                JObject state = JsonConvert.DeserializeObject<JObject>(json);
                string type = state["type"].ToString();
                if (type == "unauthorized") {
                    Connect();
                }
                else if (type == "connect") {
                    HandleConnect(state);
                }
                else {
                    lastData = null;
                    OnHandleData?.Invoke(state);
                }
            }
            catch (Exception ex) {
                Debug.LogWarning($"Error in HandleState: {ex.Message}");
            }
        }
    }

    private static void __Send(object data) {
        try {
            string jsonData = JsonConvert.SerializeObject(data);
            byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
            udpClient.Send(dataBytes, dataBytes.Length);
        }
        catch (Exception e) {
            Debug.LogWarning($"Error in SendSocket: {e.Message}");
        }
    }

    public static void Send(object data, bool saveLastData = true) {
        if (!connected) {
            lastData = data;
            Connect();
            return;
        }

        if (saveLastData) {
            lastData = data;
        }
        __Send(data);
    }

    public static void Disconnected() {
        __Send(new Event.Send.Disconnected());
    }
}
