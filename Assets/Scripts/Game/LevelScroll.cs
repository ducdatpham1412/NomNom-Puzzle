using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelScrollDialog : MonoBehaviour {
    public GameObject LevelPrefab;
    public RectTransform Content;
    public ScrollRect ScrollRect;

    float itemHeight;
    float viewPortHeight;
    float contentHeight;
    float endReachOffset;
    int batchSize;
    int poolSize;
    int firstLevel;
    readonly int spacingX = 10;
    readonly int spacingY = 16;
    readonly int cols = 3;
    bool isUpdated = false;
    bool isInit = false;
    Vector3 oldVelocity = Vector3.zero;
    List<ItemLevel> itemsPool = new List<ItemLevel>();
    DragTracker dragTracker;
    GameController Controller;


    void Start() {
        Controller = GameManager.Instance.Controller;
        dragTracker = GetComponent<DragTracker>();
        InitScrollRect();
        isInit = true;
    }

    void OnEnable() {
        if (isInit) {
            SetFirstLevel();
            int initIndex = 0;
            for (int i = 0; i < poolSize; i++) {
                int lv = firstLevel + i;
                itemsPool[i].SetLevel(lv);
                if (lv == Controller.currentLevel) {
                    initIndex = i;
                }
            }
            MoveToIndex(initIndex);
        }
    }

    void LateUpdate() {
        CheckScrollRect();
    }

    void InitScrollRect() {
        /*
        Init bounds
        */
        viewPortHeight = ScrollRect.viewport.rect.height;

        GridLayoutGroup layoutGroup = Content.GetComponent<GridLayoutGroup>();
        layoutGroup.constraintCount = cols;
        layoutGroup.padding.bottom = spacingY;
        layoutGroup.spacing = new Vector2(spacingX, spacingY);

        // Set item size base on layout
        float contentWidth = Content.rect.width;
        float itemWidth = (contentWidth - (spacingX * (cols - 1))) / cols;
        RectTransform rt = LevelPrefab.GetComponent<RectTransform>();
        float ratio = rt.rect.height / rt.rect.width;
        float itemRawHeight = itemWidth * ratio;
        itemHeight = itemRawHeight + spacingY;

        layoutGroup.cellSize = new Vector2(itemWidth, itemRawHeight);

        batchSize = Mathf.CeilToInt(viewPortHeight / itemHeight) * cols;
        poolSize = batchSize * 3;
        contentHeight = itemHeight * (batchSize / cols) * 3;
        endReachOffset = contentHeight - viewPortHeight;

        SetFirstLevel();

        /*
        Init ItemLevel and Move to initIndex
        */
        int initIndex = 0;

        for (int i = 0; i < poolSize; i++) {
            GameObject level = Instantiate(LevelPrefab, Content);
            ItemLevel itemLevel = level.GetComponent<ItemLevel>();
            int lv = firstLevel + i;
            itemLevel.SetLevel(lv);
            if (lv == Controller.currentLevel) {
                initIndex = i;
            }
            itemsPool.Add(itemLevel);
        }

        MoveToIndex(initIndex);
    }

    void CheckScrollRect() {
        if (isUpdated) {
            isUpdated = false;
            ScrollRect.velocity = oldVelocity;
        }

        if (Content.localPosition.y <= 0 && firstLevel > 1) {
            int currentLevel = firstLevel;
            firstLevel = firstLevel - (firstLevel % cols) - batchSize + 1;
            firstLevel = Mathf.Clamp(currentLevel - batchSize, 1, Controller.totalLevels);

            int initIndex = 0;
            for (int i = 0; i < poolSize; i++) {
                int lv = firstLevel + i;
                itemsPool[i].SetLevel(lv);
                if (lv == currentLevel) {
                    initIndex = i;
                }
            }

            Canvas.ForceUpdateCanvases();
            oldVelocity = ScrollRect.velocity;
            isUpdated = true;
            MoveToIndex(initIndex);

            if (dragTracker.isDragging) {
                // Recalculate m_ContentStartPosition and drag pivot
                PointerEventData ev = new PointerEventData(EventSystem.current) {
                    position = GameHelper.TouchPosition()
                };
                ScrollRect.OnBeginDrag(ev);
                ScrollRect.OnDrag(ev);
            }
        }
        else if (Content.localPosition.y >= endReachOffset && (firstLevel + poolSize) < Controller.totalLevels) {
            int currentLevel = firstLevel + poolSize - 1;

            bool isFirstCol(int lv) {
                return (lv - 1) % cols == 0;
            }

            while (!isFirstCol(currentLevel)) {
                currentLevel--;
            }

            int lastLevel = Mathf.Clamp(currentLevel + batchSize, 1, Controller.totalLevels);
            firstLevel = lastLevel - poolSize;

            int initIndex = 0;
            for (int i = 0; i < poolSize; i++) {
                int lv = firstLevel + i;
                itemsPool[i].SetLevel(lv);
                if (lv == currentLevel) {
                    initIndex = i;
                }
            }
            float initY = contentHeight - Mathf.CeilToInt((poolSize - 1 - initIndex) / cols) * itemHeight - viewPortHeight;

            Canvas.ForceUpdateCanvases();
            oldVelocity = ScrollRect.velocity;
            isUpdated = true;
            Content.localPosition = new Vector3(Content.localPosition.x, initY, Content.localPosition.z);

            if (dragTracker.isDragging) {
                // Recalculate m_ContentStartPosition and drag pivot
                PointerEventData ev = new PointerEventData(EventSystem.current) {
                    position = GameHelper.TouchPosition()
                };
                ScrollRect.OnBeginDrag(ev);
                ScrollRect.OnDrag(ev);
            }
        }
    }

    void MoveToIndex(int index) {
        float initY = Mathf.FloorToInt(index / cols) * itemHeight;
        Content.localPosition = new Vector3(Content.localPosition.x, initY, Content.localPosition.z);
    }

    void SetFirstLevel() {
        firstLevel = Controller.currentLevel - (Controller.currentLevel % cols) - batchSize + 1;
        firstLevel = Mathf.Clamp(firstLevel, 1, Controller.totalLevels);
    }
}
