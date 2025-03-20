using UnityEngine;

public class Square : MonoBehaviour {
    public GameController Controller;
    public Item Item;

    void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log("Collision: " + collision.gameObject.name);
    }
}
