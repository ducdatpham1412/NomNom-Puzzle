using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour {
    [SerializeField] RectTransform Content;
    [SerializeField] Text Title;
    [SerializeField] Text BtnTitle;
    [SerializeField] GameObject HandController;
    [SerializeField] Image ImgToolTip;

    [Header("Lines")]
    [SerializeField] RectTransform LineMiddle;

    Action OnClick;
    Canvas canvas;

    public void ClickButton() {
        OnClick?.Invoke();
    }

    public void Open(GameObject gObject, Info info) {
        Title.text = info.title;
        if (info.btnTitle != null) {
            BtnTitle.text = info.btnTitle;
        }
        OnClick = info.OnClick;
        gameObject.SetActive(true);
        StartCoroutine(RebuildAfterOneFrame(gObject));
    }

    public void Close() {
        Color startColor = ImgToolTip.color;
        LeanTween.value(startColor.a, 0f, 0.15f).setOnUpdate((float a) => {
            Color c = ImgToolTip.color;
            c.a = a;
            ImgToolTip.color = c;
        }).setOnComplete(() => {
            gameObject.SetActive(false);
            ImgToolTip.color = startColor;
        });
    }

    Vector2? ScreenToAnchoredPos(Vector3 screenPos) {
        Vector2 uiPos;
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Content.parent.GetComponent<RectTransform>(), screenPos, canvas.worldCamera, out uiPos)) {
            return uiPos;
        }
        return null;
    }

    IEnumerator RebuildAfterOneFrame(GameObject gObject) {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
        bool isWorldObject = Helper.InWorldSpace(gObject);
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        Vector3 lineScreenPos = RectTransformUtility.WorldToScreenPoint(
            (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera, LineMiddle.position
        );

        if (!isWorldObject) {
            RectTransform rect = gObject.GetComponent<RectTransform>();
            Vector3 worldPos;
            if (rect.position.y > LineMiddle.position.y) {
                Vector3 bottomLocalPos = rect.localPosition - new Vector3(0, rect.rect.height + Content.rect.height / 2, 0);
                worldPos = rect.TransformPoint(bottomLocalPos);
            }
            else {
                Vector3 topLocalPos = rect.localPosition + new Vector3(0, rect.rect.height + Content.rect.height / 2, 0);
                worldPos = rect.TransformPoint(topLocalPos);
            }

            Content.transform.position = new Vector3(Content.transform.position.x, worldPos.y, Content.transform.position.z);
        }
        else {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(gObject.transform.position);

            Vector3 size = Vector3.zero;
            SpriteRenderer renderer = gObject.GetComponent<SpriteRenderer>();
            if (renderer != null) {
                size = Vector3.Scale(renderer.bounds.size, renderer.transform.lossyScale);
            }
            else {
                RectTransform rect = gObject.GetComponent<RectTransform>();
                if (rect != null) {
                    size = Vector3.Scale(rect.rect.size, rect.lossyScale);
                }
            }

            if (screenPos.y > lineScreenPos.y) {
                screenPos = Camera.main.WorldToScreenPoint(gObject.transform.position - new Vector3(0, size.y / 2, 0));
                Vector2? uiAnchoredPos = ScreenToAnchoredPos(screenPos);
                if (uiAnchoredPos != null) {
                    Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, ((Vector2)uiAnchoredPos).y - Content.rect.height / 1.8f);
                }
            }
            else {
                screenPos = Camera.main.WorldToScreenPoint(gObject.transform.position + new Vector3(0, size.y / 2, 0));
                Vector2? uiAnchoredPos = ScreenToAnchoredPos(screenPos);
                if (uiAnchoredPos != null) {
                    Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, ((Vector2)uiAnchoredPos).y + Content.rect.height / 1.8f);
                }
            }
        }
    }


    [SerializeField]
    public class Info {
        public string title;
        public string btnTitle;
        public Action OnClick;
        public bool showHand = true;
    }
}
