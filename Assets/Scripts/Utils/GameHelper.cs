using System;
using UnityEngine.InputSystem;
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
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) {
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            return true;
        }

        return false;
    }

    public static Vector2 TouchPosition() {
        if (Touchscreen.current != null) {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (Mouse.current != null) {
            return Mouse.current.position.ReadValue();
        }

        return Vector2.zero;
    }

    public static bool TouchReleased() {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame) {
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) {
            return true;
        }

        return false;
    }


    public static Vector3 ToWorldPoint(Vector3 localPos) {
        return Camera.main.ScreenToWorldPoint(localPos);
    }
    public static bool TouchHitGameObject(Vector3 localPos, GameObject gameObject) {
        Vector3 worldPoint = ToWorldPoint(localPos);
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);
        foreach (var h in hits) {
            if (h.collider.gameObject == gameObject) return true;
        }
        return false;
    }

    public static void ScalePingPong(GameObject gObject, Vector3 scale, float time = 1f, float delay = 2f, Action<LTDescr> OnChange = null) {
        void Scale() {
            LeanTween.scale(gObject, scale, time).setEase(LeanTweenType.punch).setOnComplete(() => {
                LTDescr LT = LeanTween.delayedCall(delay, Scale);
                if (OnChange != null) {
                    OnChange?.Invoke(LT);
                }
            });
        }
        Scale();
    }
}
