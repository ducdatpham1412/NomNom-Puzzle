using UnityEngine;

public class FollowCamera : MonoBehaviour {
    public float xOffset = 0f;
    public float yOffset = 0f;
    public Transform player;

    void LateUpdate() {
        transform.position = new Vector3(
            player.position.x + xOffset, player.position.y + yOffset, -10
        );
    }
}