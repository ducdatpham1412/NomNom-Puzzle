using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour {
    public GameController Controller;
    public Item Item;
    public Square square;
    public CapsuleCollider2D capsuleCollider;

    bool isPanning = false;
    bool shouldBackToChoices = false;
    Vector3 originalPos;
    Vector3 pivotPos;
    Vector3 touchPos;
    Square originalSquare;

    void Start() {
        originalPos = transform.position;
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update() {
        HandlePan();
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.tag == Controller.GameInit.SquareTag) {
            Square s = col.GetComponent<Square>();
            if (s != square) {
                bool isValid = Controller.GameGraft.OpenSquareBorder(s, this);
                if (isValid) square = s;
            }
        }
        else if (col.gameObject.tag == Controller.GameInit.ChoicesBoardTag && originalSquare) {
            shouldBackToChoices = true;
            Controller.ChoicesBoardBorder.enabled = true;
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.tag == Controller.GameInit.SquareTag) {
            Square s = col.GetComponent<Square>();
            if (s == square) {
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
        GetComponent<SpriteRenderer>().sprite = Controller.GameInit.Creatures.Find(c => c.id == item.creature_id).sprite;
        transform.rotation = GetRotation(item.direction);
    }

    Quaternion GetRotation(string dir) {
        if (dir == Item.Direction.up.ToString()) {
            return Quaternion.Euler(0f, 0f, 180f);
        }
        if (dir == Item.Direction.left.ToString()) {
            return Quaternion.Euler(0f, 0f, -90f);
        }
        if (dir == Item.Direction.down.ToString()) {
            return Quaternion.identity;
        }
        if (dir == Item.Direction.right.ToString()) {
            return Quaternion.Euler(0f, 0f, 90f);
        }
        return Quaternion.identity;
    }

    void HandlePan() {
        if (GameHelper.TouchBegin()) {
            Vector3 mousePos = Input.mousePosition;
            isPanning = GameHelper.TouchHitGameObject(mousePos, gameObject);
            if (isPanning) {
                // If panning Item in Square, temporary set ItemController to null to simulate this square is empty
                if (square) {
                    square.ItemController = null;
                    originalSquare = square;
                }
                pivotPos = transform.position;
                touchPos = GameHelper.ToWorldPoint(mousePos);
            }
        }

        if (!isPanning) return;

        if (GameHelper.TouchReleased()) {
            Controller.GameGraft.HideSquareBorder();
            if (shouldBackToChoices) {
                square = null;
                originalSquare = null;
                shouldBackToChoices = false;
                Controller.ChoicesBoardBorder.enabled = false;
                StartCoroutine(BackToOriginal());
            }
            else if (square) {
                square.ItemController = this;
                square.AttachItem(this, animatedTo: true, checkEndGame: true);
                originalSquare = null;
            }
            else if (originalSquare) {
                square = originalSquare;
                square.ItemController = this;
                square.AttachItem(this, animatedTo: true);
                originalSquare = null;
            }
            else {
                StartCoroutine(BackToOriginal());
            }

            isPanning = false;
            return;
        }

        Vector3 mouseWorldPos = GameHelper.ToWorldPoint(Input.mousePosition);
        transform.position = pivotPos + (mouseWorldPos - touchPos) * 1.5f;
    }

    IEnumerator BackToOriginal() {
        capsuleCollider.enabled = false;
        float duration = 0.15f;
        float elapsedTime = 0f;
        Vector3 currentPos = transform.position;
        while (elapsedTime < duration) {
            transform.position = Vector3.Lerp(currentPos, originalPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;
        capsuleCollider.enabled = true;
    }
}
