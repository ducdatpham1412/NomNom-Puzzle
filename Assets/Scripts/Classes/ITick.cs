using UnityEngine;

public abstract class ITick : MonoBehaviour {
    protected int tickRate = 60;
    [SerializeField] int currentTick;
    protected float time;
    protected float tickTime;
    protected const int BUFFERSIZE = 1024;


    void Awake() {
        tickTime = 1f / tickRate;
        time = 0;
    }

    void Update() {
        time += Time.deltaTime;
        BaseUpdate();
    }

    void FixedUpdate() {
        while (time > tickTime) {
            currentTick++;
            time -= tickTime;
            OnTick();
        }
    }

    protected abstract void OnTick();

    protected virtual void BaseUpdate() { }
}