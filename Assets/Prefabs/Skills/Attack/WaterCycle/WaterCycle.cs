using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class WaterCycle : BaseSkill {
    [Header("GameObjects")]
    [SerializeField] GameObject Ball;
    [SerializeField] ParticleSystem WaterPs;

    Coroutine ExpandCoroutine;

    public override void Ready(Vector3 direction) {
        rb.linearVelocity = Vector2.zero;
        RotateFollowDirection(direction);
        base.Ready(direction);
    }

    public override void Play(Vector3 direction) {
        rb.AddForce(direction.normalized * speed, ForceMode2D.Impulse);
        ExpandCoroutine = StartCoroutine(WaitForStopAndExpand());
        base.Play(direction);
    }


    IEnumerator WaitForStopAndExpand() {
        yield return WaitToStop();
        WaterPs.gameObject.SetActive(true);
        Ball.SetActive(false);
        yield return new WaitUntil(() => !WaterPs.IsAlive(true));
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collided) return;

        void ExpandSoon() {
            collided = true;
            rb.linearVelocity = Vector3.zero;
            StopCoroutine(ExpandCoroutine);
            StartCoroutine(WaitForStopAndExpand());
        }

        if (HitTargetLayer(collider.gameObject.layer)) {
            TakeDamage take = collider.gameObject.GetComponent<TakeDamage>();
            if (take != null) {
                ExpandSoon();
                BindPlayer(take.player);
            }
            return;
        }

        if (HitObstacle(collider.gameObject)) {
            ExpandSoon();
        }
    }

    async void BindPlayer(PlayerManager player) {
        if (!effectedPlayers.Contains(player)) {
            effectedPlayers.Add(player);
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            playerMovement.AddSpeedRatio(effectSpeed, effectDuration);
            await Task.Delay((int)(effectDuration * 1000));
            playerMovement.RemoveSpeedRatio(effectSpeed);
        }
    }
}
