using System.Linq;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    Rigidbody2D Rb;
    PlayerManager PlayerManager;
    PlayerSkill Skill;
    [SerializeField] Animator animator;

    public Vector2 moveDirection;
    public Vector2 lastDirection;
    PlayerGamepad Gamepad;

    void Start() {
        Rb = GetComponent<Rigidbody2D>();
        PlayerManager = GetComponent<PlayerManager>();
        Skill = GetComponent<PlayerSkill>();
        if (PlayerManager.IsOwner) {
            Gamepad = PlayerManager.Manager.GetComponent<PlayerGamepad>();
            Gamepad.SetPlayerMovement(this);
        }
    }

    // TODO: Assign PlayerGamepad in OnNetworkSpawn

    void Update() {
        HandleAnimator();
    }

    void FixedUpdate() {
        if (moveDirection != null) {
            Rb.linearVelocity = moveDirection * PlayerManager.currentSpeed;
        }
    }

    void HandleAnimator() {
        if (!PlayerManager.IsOwner) return;

        if (moveDirection == Vector2.zero && Skill.direction == Vector2.zero) {
            return;
        }

        Vector2 temp = Skill.direction == Vector2.zero ? moveDirection : Skill.direction;
        if (Equals(temp, lastDirection)) return;
        lastDirection = temp;

        float degree = Mathf.Atan2(temp.y, temp.x) * Mathf.Rad2Deg;

        if (degree > 0 && degree < 60) {
            animator.Play("TopRight", 1);
        }
        else if (degree <= 0 && degree > -60) {
            animator.Play("BottomRight", 1);
        }
        else if (degree >= 60 && degree < 120) {
            animator.Play("Top", 1);
        }
        else if (degree <= -60 && degree > -120) {
            animator.Play("Bottom", 1);
        }
        else if (degree >= 120 && degree < 180) {
            animator.Play("TopLeft", 1);
        }
        else {
            animator.Play("BottomLeft", 1);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        AdjustVelocityOnCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision) {
        AdjustVelocityOnCollision(collision);
    }

    void AdjustVelocityOnCollision(Collision2D collision) {
        Vector2 normal = collision.GetContact(0).normal;
        Vector2 reflect = Vector2.Reflect(Rb.linearVelocity, normal);
        Rb.linearVelocity = reflect;
    }

    void VFXMovement(float ratio) {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (ratio == 1) {
            renderer.color = new Color(1f, 1f, 1f, 1f);
            return;
        }
        if (ratio >= 0 && ratio < 1) {
            float c = Mathf.Lerp(0.55f, 1f, ratio / 1f);
            renderer.color = new Color(c, c, c, 1f);
        }
        if (ratio > 1) {
            // TODO: Add effect when ratio > 1
        }
    }

    void SetCurrentSpeed() {
        float ratio = PlayerManager.speedRatios.Count == 0 ? 1 : PlayerManager.speedRatios.Aggregate(1f, (acc, num) => acc * num);
        PlayerManager.currentSpeed = (PlayerManager.originalSpeed + PlayerManager.buffSpeed) * ratio;
        VFXMovement(ratio);
    }


    public void BuffSpeed(float value) {
        PlayerManager.buffSpeed += value;
        SetCurrentSpeed();
    }

    public void AddSpeedRatio(float ratio, float? seconds) {
        PlayerManager.speedRatios.Add(ratio);
        SetCurrentSpeed();
        if (seconds != null) {
            void Revert() {
                PlayerManager.speedRatios.Remove(ratio);
                SetCurrentSpeed();
            }
            Invoke(nameof(Revert), (float)seconds);
        }
    }

    public void RemoveSpeedRatio(float ratio) {
        if (PlayerManager.speedRatios.Contains(ratio)) {
            PlayerManager.speedRatios.Remove(ratio);
            SetCurrentSpeed();
        }
    }
}
