using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FOVControl : MonoBehaviour {
    public PlayerManager Manager;
    [SerializeField] LayerMask EnvironmentLayer;
    BoxCollider2D colliderOut;
    BoxCollider2D colliderIn;
    string PlayerTag = Helper.Tag.Player.ToString();
    List<Collider2D> touchingInColliders = new List<Collider2D>();
    bool isMyTeam;

    void Start() {
        BoxCollider2D[] collider2Ds = GetComponents<BoxCollider2D>();
        colliderOut = collider2Ds[0];
        colliderIn = collider2Ds[1];
        isMyTeam = Manager.team == GameManager.Instance.gameState.my_team;
    }

    bool HitEnvironment(int layer) {
        return (EnvironmentLayer & (1 << layer)) != 0;
    }

    void SetShadowCasting(Collider2D collider) {
        ShadowCaster2D shadow = collider.GetComponent<ShadowCaster2D>();
        if (shadow != null) {
            if (collider.gameObject.transform.position.y < transform.position.y) {
                shadow.castingOption = ShadowCaster2D.ShadowCastingOptions.CastAndSelfShadow;
            }
            else {
                shadow.castingOption = ShadowCaster2D.ShadowCastingOptions.CastShadow;
            }
        }
    }

    void HideOnBush(Collider2D collider) {
        if (collider.gameObject.tag == Helper.Tag.Grass.ToString()) {
            if (isMyTeam) {
                SpriteRenderer r = collider.gameObject.GetComponent<SpriteRenderer>();
                r.color = new Color(r.color.r, r.color.g, r.color.b, 0.7f);
            }
            else {
                Manager.PlayerRenderer.enabled = false;
                Manager.GunRenderer.enabled = false;
            }
        }
    }

    void OutFromBush(Collider2D collider) {
        if (collider.gameObject.tag == Helper.Tag.Grass.ToString()) {
            if (isMyTeam) {
                SpriteRenderer r = collider.gameObject.GetComponent<SpriteRenderer>();
                r.color = new Color(r.color.r, r.color.g, r.color.b, 1f);
            }
            else {
                Manager.PlayerRenderer.enabled = true;
                Manager.GunRenderer.enabled = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.IsTouching(colliderIn) && HitEnvironment(collider.gameObject.layer)) {
            if (isMyTeam) {
                ShadowCaster2D shadow = collider.GetComponent<ShadowCaster2D>();
                if (shadow != null) {
                    touchingInColliders.Add(collider);
                    shadow.castingOption = ShadowCaster2D.ShadowCastingOptions.NoShadow;
                }
            }
            HideOnBush(collider);
            return;
        }

        if (isMyTeam && collider.IsTouching(colliderOut)) {
            if (HitEnvironment(collider.gameObject.layer)) {
                SetShadowCasting(collider);
                return;
            }

            if (collider.gameObject.tag == PlayerTag) {
                // TODO: Set active to enemy
                return;
            }

            return;
        }
    }

    void OnTriggerStay2D(Collider2D collider) {
        if (touchingInColliders.Contains(collider)) return;
        if (isMyTeam) {
            SetShadowCasting(collider);
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (touchingInColliders.Contains(collider)) {
            touchingInColliders.Remove(collider);
        }

        OutFromBush(collider);

        if (isMyTeam) {
            if (HitEnvironment(collider.gameObject.layer)) {
                SetShadowCasting(collider);
                return;
            }

            if (collider.gameObject.tag == PlayerTag) {
                // TODO: Set active to enemy
                return;
            }
        }
    }
}

