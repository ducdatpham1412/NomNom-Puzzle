using System;
using Unity.Netcode;
using UnityEngine;

public class MoveManager : ITick {
    Direction direction = Direction.stand;
    NetworkObject network;
    Rigidbody2D Rigid;

    bool isPanning = false;
    Vector3 c_delta;

    public Action TouchRelease;
    public Action TouchBegin;
    public float moveSpeed = 0f;

    void Start() {
        network = GetComponent<NetworkObject>();
        Rigid = GetComponent<Rigidbody2D>();
    }

    protected override void BaseUpdate() {
        HandlePanning();
    }

    protected override void OnTick() {
        Move();
    }

    private void HandlePanning() {
        if (!network.IsOwner) {
            return;
        }


        if (GameHelper.TouchBegin()) {
            Vector3 mousePos = Input.mousePosition;
            bool check = GameHelper.TouchHitGameObject(mousePos, gameObject);
            if (check) {
                isPanning = true;
                c_delta = transform.position - GameHelper.ToWorldPoint(mousePos);
                TouchBegin?.Invoke();
            }
        }

        if (GameHelper.TouchReleased()) {
            if (isPanning) {
                isPanning = false;
                direction = Direction.shotted;
                Rigid.linearVelocity = Vector2.zero;
                TouchRelease?.Invoke();
            }
            return;
        }


        if (isPanning) {
            Vector3 mousePos = GameHelper.ToWorldPoint(Input.mousePosition) + c_delta;
            if (c_delta != Vector3.zero) {
                c_delta = Vector3.zero;
            }
            float directionX = (mousePos - transform.position).normalized.x;
            if (Mathf.Abs(directionX) > moveSpeed / 500) {
                direction = directionX > 0 ? Direction.right : Direction.left;
            }
            else {
                direction = Direction.stand;
            }
        }
    }


    void Move() {
        if (direction == Direction.shotted) {
            return;
        }
        Vector2 newDirection = Vector2.zero;
        if (direction != Direction.stand) {
            int t = direction == Direction.left ? -1 : 1;
            newDirection = new Vector2(t, 0f);
        }
        newDirection = newDirection.normalized;
        Rigid.linearVelocity = newDirection * moveSpeed;
    }

    public enum Direction {
        left,
        stand,
        right,
        shotted,
    }
}
