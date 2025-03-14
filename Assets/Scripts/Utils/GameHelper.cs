using UnityEngine;

public static class GameHelper {
    public static World world { get; private set; }

    static GameHelper() {
        Camera cam = Camera.main;
        world = new World {
            maxX = cam.orthographicSize * cam.aspect,
            maxY = cam.orthographicSize,
        };
    }


    public static Vector3 Normalize(Vector3 worldPosition) {
        float normalizedX = Mathf.InverseLerp(-world.maxX, world.maxX, worldPosition.x);
        float normalizedY = Mathf.InverseLerp(-world.maxY, world.maxY, worldPosition.y);
        return new Vector3(normalizedX, normalizedY, worldPosition.z);
    }

    public static Vector3 DeNormalize(float normalizedX, float normalizedY) {
        float worldX = Mathf.Lerp(-world.maxX, world.maxX, normalizedX);
        float worldY = Mathf.Lerp(-world.maxY, world.maxY, normalizedY);
        return new Vector3(worldX, worldY, 0f);
    }

    public static bool TouchBegin() {
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began;
        }

        if (Input.GetMouseButtonDown(0)) {
            return true;
        }

        return false;
    }
    public static bool TouchMove() {
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Moved;
        }
        return false;
    }
    public static bool TouchReleased() {
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Ended;
        }

        if (Input.GetMouseButtonUp(0)) {
            return true;
        }

        return false;
    }
    public static Vector3 ToWorldPoint(Vector3 localPos) {
        return Camera.main.ScreenToWorldPoint(localPos);
    }
    public static bool TouchHitGameObject(Vector3 localPos, GameObject gameObject) {
        Vector3 worldPoint = ToWorldPoint(localPos);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint);
        if (hitCollider != null && hitCollider.gameObject == gameObject) {
            return true;
        }

        return false;
    }
}
