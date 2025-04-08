using UnityEngine;

public class BlackCover : MonoBehaviour {
    [SerializeField] SpriteRenderer MaskSprite;

    SpriteRenderer _MaskSprite;

    void Awake() {
        MaskSprite.gameObject.SetActive(false);
    }

    void OnDestroy() {
        if (_MaskSprite != null) {
            Destroy(_MaskSprite.gameObject);
        }
    }

    void MatchWorldSize(GameObject pivot, float scale = 1.2f) {
        var spritePivot = pivot.GetComponent<SpriteRenderer>();

        Vector3 pivotSize = Vector3.zero;

        if (spritePivot != null) {
            pivotSize = Vector3.Scale(spritePivot.sprite.bounds.size, pivot.transform.lossyScale);
        }
        else {
            RectTransform rectPivot = pivot.GetComponent<RectTransform>();
            if (rectPivot != null) {
                pivotSize = Vector3.Scale(rectPivot.rect.size, rectPivot.lossyScale);
            }
        }

        if (pivotSize == Vector3.zero) return;

        float targetSize = Mathf.Max(pivotSize.x, pivotSize.y) * scale;
        Vector3 size = _MaskSprite.sprite.bounds.size;

        Vector3 desiredScale = new Vector3(
            targetSize / size.x,
            targetSize / size.y,
            1f
        );

        _MaskSprite.transform.localScale = desiredScale;
    }

    public void Target(GameObject gObject) {
        if (_MaskSprite == null) {
            _MaskSprite = Instantiate(MaskSprite).GetComponent<SpriteRenderer>();
            _MaskSprite.gameObject.SetActive(true);
        }

        bool isWorldObject = Helper.InWorldSpace(gObject);

        if (!isWorldObject) {
            // TODO: Do with UI Element
            return;
        }

        _MaskSprite.transform.position = gObject.transform.position;
        MatchWorldSize(gObject);
        if (!gameObject.activeInHierarchy) {
            gameObject.SetActive(true);
        }
    }

    public void Close() {
        gameObject.SetActive(false);
    }
}
