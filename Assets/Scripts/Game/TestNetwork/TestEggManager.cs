using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class TestEggManager : NetworkBehaviour {
    public Sprite ExplodedSprite;
    Sprite NormalSprite;
    public TestManager manager;
    public ShakeManager shakeManager;
    Rigidbody2D Rigid;
    Vector3 c_delta;
    // Vector3 predictedPosition;
    bool isPanning = false;
    public float moveSpeed = 2f;

    private int tickRate = 60;
    [SerializeField] int currentTick;
    private float time;
    private float tickTime;
    private const int BUFFERSIZE = 1024;

    Vector3 originalPosition;
    Direction direction = Direction.stand;
    MovementData[] movementData = new MovementData[BUFFERSIZE];
    [SerializeField] private const float maxPositionError = 0.1f;
    bool c_isHit = false;


    void Awake() {
        Rigid = GetComponent<Rigidbody2D>();
        tickTime = 1f / tickRate;
        time = 0;
    }

    void Start() {
        NormalSprite = shakeManager.gameObject.GetComponent<SpriteRenderer>().sprite;
        SoundManager.Instance.PlayMusic(SoundManager.MusicSource.lifeWandering);
    }

    void Update() {
        time += Time.deltaTime;
        HandlePanning();
    }

    void FixedUpdate() {
        while (time > tickTime) {
            currentTick++;
            time -= tickTime;
            Move();
        }
    }

    void Shake() {
        float shakeAmountX = Mathf.Sin(Time.time * 70f) * 0.02f;
        float shakeAmountY = Mathf.Cos(Time.time * 70f) * 0.02f;
        transform.position = originalPosition + new Vector3(shakeAmountX, shakeAmountY, 0);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (!IsServer) {
            if (!c_isHit) {
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                renderer.enabled = false;
            }
            return;
        }
        Rigid.linearVelocity = Vector2.zero;
        Explode();
        ExplodeClientRpc(transform.position);
    }

    async void Explode() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.enabled = true;
        renderer.sprite = ExplodedSprite;
        transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        await Task.Delay(1300);
        Reset();
    }


    void Reset() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = NormalSprite;
        transform.localScale = new Vector3(0.6518f, 0.6518f, 0.6518f);
        transform.position = new Vector2(0f, -3f);
        if (IsServer) {
            NetworkObject network = GetComponent<NetworkObject>();
            network.ChangeOwnership(1);
        }
    }


    [ClientRpc]
    void ExplodeClientRpc(Vector3 explodedPos) {
        c_isHit = true;
        transform.position = explodedPos;
        Rigid.linearVelocity = Vector2.zero;
        Explode();
    }


    void M_Shot() {
        GetComponent<TestNetworkTransform>().enabled = false;
        // Rigid.linearVelocity = new Vector2(0f, 40f);
    }

    void StartShot() {
        M_Shot();

    }

    [ServerRpc]
    void ShotServerRpc(ServerRpcParams rpcParams = default) {
        direction = Direction.shot;
        NetworkObject network = GetComponent<NetworkObject>();
        network.ChangeOwnership(NetworkManager.ServerClientId);
        ShotBroadcastClientRpc(new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = NetworkManager.ConnectedClientsIds.Where(C => C != rpcParams.Receive.SenderClientId).ToList(),
            }
        });
        M_Shot();
    }

    [ClientRpc]
    void ShotBroadcastClientRpc(ClientRpcParams rpcParams) {
        M_Shot();
    }

    void Move() {
        if (direction == Direction.shot) {
            return;
        }

        Vector2 newDirection = Vector2.zero;
        if (direction != Direction.stand) {
            int t = direction == Direction.left ? -1 : 1;
            newDirection = new Vector2(t, 0f);
        }
        newDirection = newDirection.normalized;
        Rigid.linearVelocity = newDirection * moveSpeed;
    }

    async void TestExplode() {
        SoundManager.Instance.PlaySF(SoundManager.SF.Whoosh);
        await Task.Delay(1000);
        SoundManager.Instance.PlaySF(SoundManager.SF.Splat);
    }

    void HandlePanning() {
        // if (!IsOwner) {
        //     return;
        // }

        if (GameHelper.TouchBegin()) {
            Vector3 mousePos = Input.mousePosition;
            if (GameHelper.TouchHitGameObject(mousePos, gameObject)) {
                isPanning = true;
                c_delta = transform.position - GameHelper.ToWorldPoint(mousePos);
                shakeManager.StartShake();
                SoundManager.Instance.PlaySF(SoundManager.SF.Stretch);
            }
        }

        if (GameHelper.TouchReleased()) {
            if (isPanning) {
                isPanning = false;
                direction = Direction.stand;
                StartShot();
                shakeManager.StopShake();
                TestExplode();
                return;
            }
            return;
        }


        if (isPanning) {
            Vector3 mousePos = GameHelper.ToWorldPoint(Input.mousePosition) + c_delta;
            if (c_delta != Vector3.zero) {
                c_delta = Vector3.zero;
            }
            float directionX = (mousePos - transform.position).normalized.x;
            if (Mathf.Abs(directionX) > 0.004f) {
                direction = directionX > 0 ? Direction.right : Direction.left;
            }
            else {
                direction = Direction.stand;
            }

            // Predict
            // updateLocal++;
            // Debug.Log($"Update local: {updateLocal}");
            // predictedPosition += state.direction * moveSpeed * Time.deltaTime;
            // transform.position = predictedPosition;
        }
    }


    // void InterpolateToServerPosition() {
    //     transform.position = Vector3.Lerp(transform.position, state.position, Time.deltaTime * 10f);
    // }


    // void PredictMovement() {
    //     if (isPanning) {
    //         updateLocal++;
    //         Debug.Log($"Update local: {updateLocal}");
    //         predictedPosition += state.direction * moveSpeed * Time.deltaTime;
    //         transform.position = predictedPosition;
    //     }
    // }

    // public void Reconciliate(TestManager.GameState.Egg serverEgg) {
    //     state.Copy(other: serverEgg);
    //     float positionError = Vector3.Distance(predictedPosition, state.position);

    //     if (positionError > 0.1f) {
    //         updateFromServer++;
    //         Debug.Log($"Update from server: {updateFromServer}");
    //         transform.position = state.position;
    //         predictedPosition = state.position;
    //     }

    //     if (state.status == "hit") {
    //         Explode();
    //     }
    // }


    // [ServerRpc(RequireOwnership = false)]
    // void SendMoveDirectionToServerRpc(Vector3 direction) {
    //     state.direction = direction;
    //     transform.position += direction * moveSpeed * Time.deltaTime;
    //     state.position = transform.position;
    // }


    // [ServerRpc]
    // void MoveServerRpc(MovementData current, MovementData last, ServerRpcParams rpcParams = default) {
    //     Vector2 startPosition = transform.position;
    //     Vector2 moveVector = last.direction * moveSpeed;
    //     Physics.simulationMode = SimulationMode.Script;
    //     transform.position = last.position;
    //     Rigid.linearVelocity = moveVector;
    //     Physics.Simulate(tickTime);
    //     Vector2 correctPosition = transform.position;
    //     transform.position = startPosition;
    //     Physics.simulationMode = SimulationMode.FixedUpdate;
    //     if (Vector2.Distance(correctPosition, current.position) > maxPositionError) {
    //         ReconciliateClientRPC(current.tick, new ClientRpcParams {
    //             Send = new ClientRpcSendParams {
    //                 TargetClientIds = new List<ulong>() { rpcParams.Receive.SenderClientId }
    //             }
    //         });
    //     }
    //     else {
    //         Vector2 newMoveVector = current.direction * moveSpeed;
    //         Rigid.linearVelocity = newMoveVector;
    //     }
    // }

    // [ClientRpc]
    // private void ReconciliateClientRPC(int activationTick, ClientRpcParams rpcParams) {
    //     Vector2 correctPosition = movementData[(activationTick - 1) % BUFFERSIZE].position;

    //     Physics.simulationMode = SimulationMode.Script;
    //     while (activationTick <= currentTick) {
    //         Vector2 moveVector = movementData[(activationTick - 1) % BUFFERSIZE].direction.normalized * moveSpeed;
    //         transform.position = correctPosition;
    //         Rigid.linearVelocity = moveVector;
    //         Physics.Simulate(tickTime);
    //         correctPosition = transform.position;
    //         movementData[activationTick % BUFFERSIZE].position = correctPosition;
    //         activationTick++;
    //     }
    //     Physics.simulationMode = SimulationMode.FixedUpdate;

    //     transform.position = correctPosition;
    // }


    // void Simulate() {
    //     Vector3 originalPos = transform.position;

    //     Physics2D.simulationMode = SimulationMode2D.Script;
    //     for (int i = 0; i < 35; i++) {
    //         Rigid.linearVelocity = new Vector2(0f, 2f);
    //         Physics2D.Simulate(Time.fixedDeltaTime);
    //     }
    //     Vector3 correctedPos = transform.position;
    //     transform.position = originalPos;
    //     Rigid.linearVelocity = Vector2.zero;

    //     Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

    //     transform.position = correctedPos;
    // }


    // IEnumerator SendPositionAndVelocityToClient() {
    //     while (isShotting) {
    //         transform.position += state.direction * moveSpeed * Time.deltaTime;
    //         state.position = transform.position;
    //         yield return new WaitForSeconds(0.1f);
    //     }
    // }



    [System.Serializable]
    public class MovementData : INetworkSerializable {
        public int tick;
        public Vector2 direction;
        public Vector2 position;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref direction);
            serializer.SerializeValue(ref position);
        }
    }

    public enum Direction {
        left,
        stand,
        right,
        shot,
    }
}
