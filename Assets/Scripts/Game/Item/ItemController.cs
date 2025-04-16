using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour {
    public GameController Controller;
    public Item Item;
    public Square square;
    public CapsuleCollider2D capsuleCollider;

    bool isPanning = false;
    bool isFirstTouch = false;
    bool shouldBackToChoices = false;
    [HideInInspector] public bool animatingToSquare = false;
    bool animatingToOriginalFromDoubleClick = false;
    float lastClickTime = 0f;
    int originalSortingOrder;
    Vector3 originalPos;
    [HideInInspector] public Vector3 originalScale { get; private set; }
    static Vector3 pivotPos;
    static Vector3 touchPos;
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

    void OnDestroy() {
        // ResetAllActions to avoid LeanTween delay to destroyed game objects (function ScalePingPong)
        ResetAllActions();
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (animatingToOriginalFromDoubleClick || animatingToSquare || Controller.GameInit.isInitializing) return;

        if (col.gameObject.tag == Controller.GameInit.SquareTag) {
            Square colSquare = col.GetComponent<Square>();
            if (col != colSquare.PanCollider) return;
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
        if (animatingToOriginalFromDoubleClick || animatingToSquare) return;

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
            Helper.Vibrate();
            StartCoroutine(ScaleAndShake());
        }
        LeanTween.cancel(gameObject);
        ResetLtDescr();
        void ScalePingPong() {
            // We have ResetAllActions above to avoid ScalePingPong delay to destroyed game objects, but check here one move for sure
            if (gameObject == null) return;

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
            if (!Controller.isFocusPlayingGame) return;
            Vector3 _touch = GameHelper.TouchPosition();
            isPanning = GameHelper.TouchHitGameObject(_touch, gameObject);
            if (isPanning) {
                bool shouldMoveUp = true;

                if (!Controller.ShouldHandlePan()) {
                    isPanning = false;
                    return;
                }

                if (Controller.GameTutorial.isTutorial) {
                    if (Controller.GameTutorial.StepGameObject != gameObject) {
                        isPanning = false;
                        return;
                    }
                    if (Controller.GameTutorial.StartPanItemAction != null) {
                        Controller.GameTutorial.StartPanItemAction?.Invoke();
                        Controller.GameTutorial.StartPanItemAction = null;
                    }
                }

                if (Controller.numberSuggestions > 0) {
                    isPanning = false;
                    Controller.GameGraft.SuggestItemToSquare(this);
                    return;
                }

                if (square) {
                    shouldMoveUp = false;

                    if (lastClickTime != 0f && Time.time - lastClickTime < Controller.doubleClickThreshold) {
                        animatingToOriginalFromDoubleClick = true;
                        square.RemoveItem();
                        BackToOriginal();
                        // Have to set originalSquare = square, because originalSquare has been null at the lase release, see "@Tag: Set to null after release"
                        originalSquare = square;
                        CheckValidAtOriginalSquare();
                        square = null;
                        originalColor = null;
                        lastClickTime = 0f;
                        isPanning = false;
                        return;
                    }

                    lastClickTime = Time.time;

                    // Is square having any coroutines (AnimateToCenter,...), do nothing, because SetItemToNull can cause error
                    if (animatingToSquare) {
                        isPanning = false;
                        return;
                    }

                    // If panning Item in Square, temporary set ItemController to null to simulate this square is empty
                    ResetAllActions();
                    originalColor = square.GetColor();
                    square.RemoveItem();
                    originalSquare = square;
                    square = null;
                }

                isFirstTouch = true;
                pivotPos = transform.position;
                touchPos = GameHelper.ToWorldPoint(_touch);
                Renderer.sortingOrder = originalSortingOrder + 1;
                SoundManager.Instance.PlaySF(SoundManager.SF.Pop_01);

                LeanTween.scale(gameObject, originalScale * 1.5f, 0.1f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() => {
                    isFirstTouch = false;
                });
                if (shouldMoveUp) {
                    LeanTween.moveY(gameObject, pivotPos.y + 0.7f, 0.1f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() => {
                        pivotPos = transform.position;
                    });
                }
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
                Controller.GameGraft.ItemAttachSquare(
                    item: this,
                    square: square
                );
            }
            else if (originalSquare) {
                if (originalSquare.isError) {
                    originalSquare.ResetError();
                    BackToChoice();
                }
                else {
                    square = originalSquare;
                    Controller.GameGraft.SquareAttachItem(
                        square: square,
                        item: this,
                        attachParams: new GameGraft.SquareAttachItemParams {
                            animatedTo = true,
                            color = originalColor,
                        }
                    );
                    originalSquare = null; // @Tag: Set to null after release
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

        if (isFirstTouch) return;

        Vector3 touchWorldPos = GameHelper.ToWorldPoint(GameHelper.TouchPosition());
        transform.position = pivotPos + (touchWorldPos - touchPos) * 1.5f;
    }

    public void ResetAllActions() {
        LeanTween.cancel(gameObject);
        ResetLtDescr();
        StopAllCoroutines();
        transform.localScale = originalScale;
        if (animatingToSquare) animatingToSquare = false;
    }

    public void CheckValidAtOriginalSquare() {
        if (originalSquare) {
            Controller.GameGraft.CheckValidAroundSquare(originalSquare, hasMatched: false);
            originalSquare = null;
        }
    }

    public void BackToOriginal() {
        ResetAllActions();
        capsuleCollider.enabled = false;
        LeanTween.move(gameObject, originalPos, 0.15f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {
            capsuleCollider.enabled = true;
            animatingToOriginalFromDoubleClick = false;
        });
    }

    void ResetLtDescr() {
        if (lTDescr != null) {
            LeanTween.cancel(lTDescr.id);
            lTDescr = null;
        }
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
