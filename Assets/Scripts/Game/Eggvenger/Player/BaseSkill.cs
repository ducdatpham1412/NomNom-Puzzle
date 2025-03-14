using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSkill : MonoBehaviour {
    [Header("Properties")]
    public string id;
    public LayerMask targetLayers;

    [SerializeField] protected float speed = 0.6f;
    public Sprite SkillSprite;
    public bool canReady = true;
    public int price;
    public int maxNumber; // max number of this skill player can own

    [Header("Durations")]
    [SerializeField] protected float lifeDuration = 1f;
    [SerializeField] protected float fadeDuration = 0f;
    [SerializeField] public float effectDuration = 1f;

    [Header("Effects")]
    [SerializeField] public float effectSpeed; // Ratio speed to current speed. Ex: If you want decrease 20% speed -> Set "speed" = 0.8

    [Header("Sounds")]
    [SerializeField] AudioClip ReadySound;
    [SerializeField] AudioClip PlaySound;

    [Header("Others")]
    protected PlayerManager Creator;

    // Others
    protected Rigidbody2D rb;
    protected bool collided = false;
    protected bool played = false;
    PlayerSkill playerSkill;
    LayerMask EnvMask;
    [HideInInspector] public List<PlayerManager> effectedPlayers = new List<PlayerManager>();

    protected virtual void Awake() {
        rb = GetComponent<Rigidbody2D>();
        EnvMask = LayerMask.GetMask(Helper.Layer.Environment.ToString());
    }

    public virtual void Ready(Vector3 direction) {
        // TODO: Playing sound ready
    }

    public virtual void Play(Vector3 direction) {
        // TODO: Playing sound play
        played = true;
    }

    void LateUpdate() {
        if (!played) {
            if (playerSkill == null) {
                playerSkill = Creator.GetComponent<PlayerSkill>();
            }
            transform.position = playerSkill.GetSkillSpawnPos(isLocal: false);
        }
    }

    protected void RotateFollowDirection(Vector3 direction) {
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);
    }

    protected string GetLayerName(GameObject gameObject) {
        return LayerMask.LayerToName(gameObject.layer);
    }

    public bool HitTargetLayer(LayerMask hitLayer) {
        return (targetLayers & (1 << hitLayer)) != 0;
    }

    protected bool HitObstacle(GameObject gameObject) {
        return (EnvMask & (1 << gameObject.layer)) != 0 && gameObject.tag != Helper.Tag.Grass.ToString();
    }

    protected WaitUntil WaitToStop(float magnitudeThreshold = 0.4f) {
        return new WaitUntil(() => rb.linearVelocity.magnitude <= magnitudeThreshold);
    }

    public virtual void SetCreator(PlayerManager manager, bool targetAll = false) {
        Creator = manager;

        if (targetAll) {
            targetLayers = LayerMask.GetMask(Helper.Layer.PlayerBlue.ToString(), Helper.Layer.PlayerRed.ToString());
        }
        else {
            if (GetLayerName(Creator.gameObject) == Helper.Layer.PlayerBlue.ToString()) {
                targetLayers = LayerMask.GetMask(Helper.Layer.PlayerRed.ToString());
            }
            else {
                targetLayers = LayerMask.GetMask(Helper.Layer.PlayerBlue.ToString());
            }
        }
    }
}
