using Unity.Netcode;

public abstract class EntityManager : NetworkBehaviour {
    protected bool isPanning = false;
    public float moveSpeed;
}
