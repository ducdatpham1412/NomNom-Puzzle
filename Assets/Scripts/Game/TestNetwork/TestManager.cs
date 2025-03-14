using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;


public class TestManager : NetworkBehaviour {
    // [Serializable]
    // public class GameState : INetworkSerializable, IEquatable<GameState> {
    //     public class Egg : INetworkSerializable, IEquatable<Egg> {
    //         public Vector3 direction;
    //         public Vector3 position;
    //         public string status; // active, hit

    //         public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    //             if (serializer.IsWriter) {
    //                 var writer = serializer.GetFastBufferWriter();
    //                 writer.WriteValueSafe(direction);
    //                 writer.WriteValueSafe(position);
    //                 writer.WriteValueSafe(status);
    //             }
    //             else {
    //                 var reader = serializer.GetFastBufferReader();
    //                 reader.ReadValueSafe(out direction);
    //                 reader.ReadValueSafe(out position);
    //                 reader.ReadValueSafe(out status);
    //             }
    //         }


    //         public void Copy(Egg other) {
    //             direction = other.direction;
    //             position = other.position;
    //             status = other.status;
    //         }

    //         public bool Equals(Egg other) {
    //             return direction.Equals(other.direction) && position.Equals(other.position) && status == other.status;
    //         }
    //     }

    //     public Egg egg;



    //     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    //         if (serializer.IsWriter) {
    //             serializer.SerializeValue(ref egg);
    //         }
    //         else {
    //             if (egg == null) {
    //                 egg = new Egg();
    //             }
    //             serializer.SerializeValue(ref egg);
    //         }
    //     }

    //     public bool Equals(GameState other) {
    //         return true;
    //     }
    // }

    // public GameState gameState = new GameState {
    //     egg = new GameState.Egg {
    //         direction = Vector3.zero,
    //         status = "active",
    //     },
    // };

    // [ClientRpc]
    // public void PassStateClientRpc(GameState _newState) {
    //     // eggManager.Reconciliate(_newState.egg);
    // }

    public TestEggManager eggManager;
    public PlayerThrowEggManager playerManager;
    public GameObject EggPrefab;

    GameStateBuffer gameStateBuffer = new GameStateBuffer(1024);


    void Start() {
        GameObject mainCamera = GameObject.Find("MainCamera");
        FlipGameObject(mainCamera);
        FlipGameObject(GameManager.Instance.gameObject);
    }


    public void HandleShot(long timestamp, ulong SenderClientID) {
        long current = Helper.TimeStamp();
        long tsOnServer = current - 2 * (current - timestamp); // t - RTT
        GameStateSnapShot snapShot = gameStateBuffer.GetSnapshot(tsOnServer);
        if (snapShot == null) return;

        // TODO: Simulate & Rewind
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => {
                Debug.Log($"Having client connected: {clientID}");
                // if (eggManager.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.ServerClientId) {
                //     eggManager.gameObject.GetComponent<NetworkObject>().ChangeOwnership(clientID);
                // }
                // else {
                //     playerManager.gameObject.GetComponent<NetworkObject>().ChangeOwnership(clientID);
                // }
            };


            GameObject egg = Instantiate(EggPrefab);
            egg.GetComponent<NetworkObject>().Spawn();

            Debug.Log("Ok spawn an egg");
        }
    }

    void StartServer() {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", 7778);
        NetworkManager.Singleton.StartServer();
    }

    void FlipGameObject(GameObject gameObject) {
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180f));
        Vector3 pos = gameObject.transform.position;
        pos.y = pos.y - 1;
        gameObject.transform.position = pos;
    }

    void ReverseGameObject(GameObject gameObject) {
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        gameObject.transform.position = new Vector3(0, 0, 0);
    }

    public class GameStateSnapShot {
        public class Value {
            public Vector3 position;
            public Vector3 velocity;
        }
        public Value Egg;
        public Value Player;
        public long Timestamp;
    }


    public class GameStateBuffer {
        private GameStateSnapShot[] snapshots;
        private int currentIndex;
        private int capacity;

        public GameStateBuffer(int capacity) {
            this.capacity = capacity;
            snapshots = new GameStateSnapShot[capacity];
            currentIndex = 0;
        }

        public void AddSnapshot(GameStateSnapShot snapshot) {
            snapshots[currentIndex] = snapshot;
            currentIndex = (currentIndex + 1) % capacity;
        }

        public GameStateSnapShot GetSnapshot(long timestamp) {
            for (int i = 0; i < capacity; i++) {
                if (snapshots[i] != null && snapshots[i].Timestamp <= timestamp) {
                    return snapshots[i];
                }
            }
            return null;
        }
    }
}
