using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelScrollDialog : MonoBehaviour {
    public GameObject LevelPrefab;
    public RectTransform Content;
    public ScrollRect ScrollRect;
    public int totalLevels = 100;
    public int startLevel = 20;

    float itemHeight;
    float viewPortHeight;
    float contentHeight;
    float endReachOffset;
    int batchSize;
    int poolSize;
    int firstLevel;
    readonly int spacing = 20;
    Vector3 oldVelocity = Vector3.zero;
    bool isUpdated = false;
    List<ItemLevel> itemsPool = new List<ItemLevel>();
    DragTracker dragTracker;


    void Start() {
        InitScrollRect();
        dragTracker = GetComponent<DragTracker>();
    }

    void LateUpdate() {
        CheckScrollRect();
    }

    void InitScrollRect() {
        int initIndex = 0;

        VerticalLayoutGroup layoutGroup = Content.GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = spacing;
        layoutGroup.padding.bottom = spacing;

        itemHeight = LevelPrefab.GetComponent<RectTransform>().rect.height + spacing;
        viewPortHeight = ScrollRect.viewport.rect.height;
        batchSize = (int)Mathf.Ceil(viewPortHeight / itemHeight);
        firstLevel = Mathf.Clamp(startLevel - batchSize, 1, totalLevels);
        poolSize = batchSize * 3;
        contentHeight = itemHeight * poolSize;
        endReachOffset = contentHeight - viewPortHeight;

        for (int i = 0; i < poolSize; i++) {
            GameObject level = Instantiate(LevelPrefab, Content);
            ItemLevel itemLevel = level.GetComponent<ItemLevel>();
            int lv = firstLevel + i;
            itemLevel.SetLevel(lv);
            if (lv == startLevel) {
                initIndex = i;
            }
            itemsPool.Add(itemLevel);
        }

        Content.localPosition = new Vector3(Content.localPosition.x, itemHeight * initIndex, Content.localPosition.z);
    }

    void CheckScrollRect() {
        if (isUpdated) {
            isUpdated = false;
            ScrollRect.velocity = oldVelocity;
        }

        if (Content.localPosition.y <= 0 && firstLevel > 1) {
            int currentLevel = firstLevel;
            firstLevel = Mathf.Clamp(currentLevel - batchSize, 1, totalLevels);

            int initIndex = 0;
            for (int i = 0; i < poolSize; i++) {
                int lv = firstLevel + i;
                itemsPool[i].SetLevel(lv);
                if (lv == currentLevel) {
                    initIndex = i;
                }
            }
            float initY = itemHeight * initIndex;

            Canvas.ForceUpdateCanvases();
            oldVelocity = ScrollRect.velocity;
            isUpdated = true;
            Content.localPosition = new Vector3(Content.localPosition.x, initY, Content.localPosition.z);

            if (dragTracker.isDragging) {
                // Recalculate m_ContentStartPosition and drag pivot
                PointerEventData ev = new PointerEventData(EventSystem.current) {
                    position = Input.mousePosition
                };
                ScrollRect.OnBeginDrag(ev);
                ScrollRect.OnDrag(ev);
            }
        }
        else if (Content.localPosition.y >= endReachOffset && (firstLevel + poolSize) < totalLevels) {
            int currentLevel = firstLevel + poolSize - 1;
            int lastLevel = Mathf.Clamp(currentLevel + batchSize, 1, totalLevels);
            firstLevel = lastLevel - poolSize;

            int initIndex = 0;
            for (int i = 0; i < poolSize; i++) {
                int lv = firstLevel + i;
                itemsPool[i].SetLevel(lv);
                if (lv == currentLevel) {
                    initIndex = i;
                }
            }
            float initY = contentHeight - (poolSize - 1 - initIndex) * itemHeight - viewPortHeight;

            Canvas.ForceUpdateCanvases();
            oldVelocity = ScrollRect.velocity;
            isUpdated = true;
            Content.localPosition = new Vector3(Content.localPosition.x, initY, Content.localPosition.z);

            if (dragTracker.isDragging) {
                // Recalculate m_ContentStartPosition and drag pivot
                PointerEventData ev = new PointerEventData(EventSystem.current) {
                    position = Input.mousePosition
                };
                ScrollRect.OnBeginDrag(ev);
                ScrollRect.OnDrag(ev);
            }
        }
    }
}
