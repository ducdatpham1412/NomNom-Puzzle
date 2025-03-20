using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour {
    bool isPanning = false;
    Vector3 originalPos;
    Vector3 touchPos;

    void Start() {
        originalPos = transform.position;
    }

    void Update() {
        HandlePan();
    }

    void HandlePan() {
        if (GameHelper.TouchBegin()) {
            Vector3 mousePos = Input.mousePosition;
            isPanning = GameHelper.TouchHitGameObject(mousePos, gameObject);
            if (isPanning) {
                touchPos = GameHelper.ToWorldPoint(mousePos);
            }
        }

        if (!isPanning) return;

        if (GameHelper.TouchReleased()) {
            StartCoroutine(BackToOriginal());
            isPanning = false;
            return;
        }

        Vector3 mouseWorldPos = GameHelper.ToWorldPoint(Input.mousePosition);
        transform.position = originalPos + (mouseWorldPos - touchPos) * 1.5f;
    }

    IEnumerator BackToOriginal() {
        float duration = 0.3f;
        float elapsedTime = 0f;
        Vector3 currentPos = transform.position;
        while (elapsedTime < duration) {
            transform.position = Vector3.Lerp(currentPos, originalPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;
    }
}
