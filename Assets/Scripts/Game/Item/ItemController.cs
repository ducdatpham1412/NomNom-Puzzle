using System;
using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour {
    public GameController Controller;
    public Item Item;
    public Square square;
    public CapsuleCollider2D capsuleCollider;

    bool isPanning = false;
    bool shouldBackToChoices = false;
    bool animatingToSquare = false;
    float lastClickTime = 0f;
    int originalSortingOrder;
    Vector3 originalPos;
    Vector3 originalScale;
    Vector3 pivotPos;
    Vector3 touchPos;
    Square originalSquare;
    Color? originalColor;
    SpriteRenderer Renderer;
    LTDescr lTDescr;

    void Start() {
        originalScale = transform.localScale;
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        Renderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = Renderer.sortingOrder;
    }

    void Update() {
        HandlePan();
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.tag == Controller.GameInit.SquareTag) {
            Square colSquare = col.GetComponent<Square>();
            if (colSquare != square) {
                bool isValid = Controller.GameGraft.OpenSquareBorder(colSquare, this);
                if (isValid) square = colSquare;
            }
        }
        else if (col.gameObject.tag == Controller.GameInit.ChoicesBoardTag && originalSquare) {
            shouldBackToChoices = true;
            Controller.ChoicesBoardBorder.enabled = true;
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.tag == Controller.GameInit.SquareTag) {
            Square colSquare = col.GetComponent<Square>();
            if (colSquare == square) {
                Controller.GameGraft.HideSquareBorder();
                square = null;
            }
        }
        else if (col.gameObject.tag == Controller.GameInit.ChoicesBoardTag && originalSquare) {
            shouldBackToChoices = false;
            Controller.ChoicesBoardBorder.enabled = false;
        }
    }

    public void SetItem(Item item) {
        Item = item;
        Creature creature = Controller.GameInit.CreaturesObject.Creatures.Find(c => c.id == item.creature_id);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = creature.sprite;
        transform.rotation = GetRotation(item.direction, creature.rotationOffset, sr);
    }

    public IEnumerator ScaleAndShake(float shakeSpeed = 70f, float scale = 2f) {
        float duration = 0.15f;
        float elapsedTime = 0f;
        Vector3 targetScale = originalScale * scale;

        // Step 01: Scale up
        while (elapsedTime < duration) {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Step 02: Shake
        duration = 0.5f;
        elapsedTime = 0f;
        float shakeAmplitude = 0.03f;
        Vector3 originalPos = transform.localPosition;
        while (elapsedTime < duration) {
            float shakeAmountX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmplitude;
            float shakeAmountY = Mathf.Cos(Time.time * shakeSpeed) * shakeAmplitude;
            transform.localPosition = originalPos + new Vector3(shakeAmountX, shakeAmountY, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Step 03: Scale down
        duration = 0.15f;
        elapsedTime = 0f;
        while (elapsedTime < duration) {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        transform.localPosition = originalPos;
    }

    public void PingErrorInterval(bool shouldScale) {
        if (shouldScale) {
            StartCoroutine(ScaleAndShake());
        }
        LeanTween.cancel(gameObject);
        ResetLtDescr();
        void ScalePingPong() {
            LeanTween.scale(gameObject, originalScale * 1.7f, 1f).setEase(LeanTweenType.punch).setOnComplete(() => {
                lTDescr = LeanTween.delayedCall(3.5f, ScalePingPong);
            });
        }
        lTDescr = LeanTween.delayedCall(shouldScale ? 4f : 2f, ScalePingPong);
    }

    public void SetOriginalPos(Vector3 pos) {
        originalPos = pos;
    }

    void HandlePan() {
        if (GameHelper.TouchBegin()) {
            Vector3 mousePos = Input.mousePosition;
            isPanning = GameHelper.TouchHitGameObject(mousePos, gameObject);

            if (isPanning) {
                if (!Controller.ShouldHandlePan()) {
                    isPanning = false;
                    return;
                }

                if (square) {
                    if (lastClickTime != 0f && Time.time - lastClickTime < Controller.doubleClickThreshold) {
                        square.TemporarySetItemToNull();
                        BackToOriginal();
                        square = null;
                        originalColor = null;
                        originalSquare = null;
                        lastClickTime = 0f;
                        isPanning = false;

                        // TODO: Playing sound

                        return;
                    }

                    lastClickTime = Time.time;

                    // Is square having any coroutines (AnimateToCenter,...), do nothing, because SetItemToNull can cause error
                    if (animatingToSquare) {
                        isPanning = false;
                        return;
                    }

                    // If panning Item in Square, temporary set ItemController to null to simulate this square is empty
                    ResetCoroutines();
                    originalColor = square.GetColor();
                    square.TemporarySetItemToNull();
                    originalSquare = square;
                    square = null;
                }

                pivotPos = transform.position;
                touchPos = GameHelper.ToWorldPoint(mousePos);
                Renderer.sortingOrder = originalSortingOrder + 1;
            }
        }

        if (!isPanning) return;

        if (GameHelper.TouchReleased()) {
            Controller.GameGraft.HideSquareBorder();

            void BackToChoice() {
                square = null;
                shouldBackToChoices = false;
                Controller.ChoicesBoardBorder.enabled = false;
                BackToOriginal();
                CheckValidAtOriginalSquare();
            }

            if (shouldBackToChoices) {
                BackToChoice();
            }
            else if (square && square != originalSquare) {
                ItemController currentItem = square.GetItemController();
                if (currentItem && currentItem != this) {
                    ReplaceItem(replacedItem: currentItem);
                }
                else {
                    square.AttachItem(this, animatedTo: true, checkEndGame: true, checkValidAroundSquares: true);
                }
                CheckValidAtOriginalSquare();
            }
            else if (originalSquare) {
                if (originalSquare.isError) {
                    originalSquare.ResetError();
                    BackToChoice();
                }
                else {
                    square = originalSquare;
                    square.AttachItem(this, animatedTo: true, color: originalColor);
                    originalSquare = null;
                    originalColor = null;
                }
            }
            else {
                BackToOriginal();
            }

            Renderer.sortingOrder = originalSortingOrder;
            isPanning = false;
            return;
        }

        Vector3 mouseWorldPos = GameHelper.ToWorldPoint(Input.mousePosition);
        transform.position = pivotPos + (mouseWorldPos - touchPos) * 1.5f;
    }

    public void ResetCoroutines() {
        LeanTween.cancel(gameObject);
        ResetLtDescr();
        StopAllCoroutines();
        transform.localScale = originalScale;
    }

    public void AnimateToSquare(Action callback) {
        animatingToSquare = true;
        LeanTween.move(gameObject, square.Center, 0.1f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {
            callback.Invoke();
            Controller.GameGraft.CheckEndGame();
            animatingToSquare = false;
            transform.position = square.Center;
        });
    }

    void ResetLtDescr() {
        if (lTDescr != null) {
            LeanTween.cancel(lTDescr.id);
            lTDescr = null;
        }
    }

    void CheckValidAtOriginalSquare() {
        if (originalSquare) {
            Controller.GameGraft.CheckValidAroundSquare(originalSquare, hasMatched: false);
            originalSquare = null;
        }
    }

    void ReplaceItem(ItemController replacedItem) {
        Square sq = replacedItem.square;
        replacedItem.square = null;
        replacedItem.BackToOriginal();
        sq.AttachItem(this, animatedTo: true, checkEndGame: false, checkValidAroundSquares: true);
    }

    void BackToOriginal() {
        LeanTween.cancel(gameObject);
        StopAllCoroutines();
        capsuleCollider.enabled = false;
        LeanTween.move(gameObject, originalPos, 0.15f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {
            capsuleCollider.enabled = true;
        });
    }

    Quaternion GetRotation(string dir, float offset, SpriteRenderer sr) {
        if (dir == Item.Direction.up.ToString()) {
            return Quaternion.Euler(0f, 0f, 180f + offset);
        }
        if (dir == Item.Direction.left.ToString()) {
            return Quaternion.Euler(0f, 0f, -90f + offset);
        }
        if (dir == Item.Direction.down.ToString()) {
            return Quaternion.Euler(0f, 0f, offset);
        }
        if (dir == Item.Direction.right.ToString()) {
            if (offset == 90f) {
                sr.flipY = true;
            }
            return Quaternion.Euler(0f, 0f, 90f + offset);
        }
        return Quaternion.identity;
    }
}
