using System.Collections;
using UnityEngine;

public class MagmaFire : BaseSkill {
    [Header("GameObjects")]
    [SerializeField] GameObject Ball;
    [SerializeField] ParticleSystem FireWall;

    Coroutine BurnCoroutine;

    public override void Ready(Vector3 direction) {
        rb.linearVelocity = Vector2.zero;
        RotateFollowDirection(direction);
        base.Ready(direction);
    }

    public override void Play(Vector3 direction) {
        rb.AddForce(direction.normalized * speed, ForceMode2D.Impulse);
        BurnCoroutine = StartCoroutine(WaitForStopAndBurn());
        base.Play(direction);
    }

    IEnumerator WaitForStopAndBurn() {
        yield return WaitToStop();
        FireWall.gameObject.SetActive(true);
        Ball.SetActive(false);
        yield return new WaitUntil(() => !FireWall.IsAlive(true));
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collided) return;

        void BurnSoon() {
            collided = true;
            rb.linearVelocity = Vector3.zero;
            if (BurnCoroutine != null) {
                StopCoroutine(BurnCoroutine);
            }
            StartCoroutine(WaitForStopAndBurn());
        }

        if (HitTargetLayer(collider.gameObject.layer)) {
            TakeDamage take = collider.gameObject.GetComponent<TakeDamage>();
            if (take != null) {
                BurnSoon();
                FireWall.GetComponent<MagmaParticles>().BurnPlayer(take.player);
            }
            return;
        }

        if (HitObstacle(collider.gameObject)) {
            BurnSoon();
        }
    }

    public float GetEffectDuration() {
        return effectDuration;
    }
}
