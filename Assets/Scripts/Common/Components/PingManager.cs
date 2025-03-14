using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class PingManager : MonoBehaviour {
    Text text;
    UnityTransport transport;

    void Start() {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        text = GetComponent<Text>();
        text.text = "";
        InvokeRepeating(nameof(UpdatePing), 1f, 5f);
    }

    void UpdatePing() {
        if (NetworkManager.Singleton.IsClient && transport != null) {
            float ping = transport.GetCurrentRtt(NetworkManager.ServerClientId);
            if (ping < 50)
                text.color = Color.green;
            else if (ping < 150)
                text.color = Color.yellow;
            else
                text.color = Color.red;
            text.text = $"Ping: {ping:F0} ms";
        }
    }
}
