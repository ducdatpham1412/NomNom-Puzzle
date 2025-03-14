using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class BindingRoot : BaseSkill {
    float radius = 5f;

    float expandDuration = 2f;

    [Header("GameObjects")]
    [SerializeField] GameObject BindingCycle;
    [SerializeField] GameObject Ball;

    [Header("VFXs")]
    [SerializeField] Sprite BindingEffect;

    Color originColor;
    Coroutine ExpandCoroutine;

    protected override void Awake() {
        originColor = BindingCycle.GetComponent<SpriteRenderer>().color;
        base.Awake();
    }

    void OnEnable() {
        BindingCycle.GetComponent<SpriteRenderer>().color = originColor;
    }

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

    void OnTriggerEnter2D(Collider2D collider) {
        if (collided) return;

        void ExpandSoon() {
            collided = true;
            rb.linearVelocity = Vector3.zero;
            if (ExpandCoroutine != null) {
                StopCoroutine(ExpandCoroutine);
                ExpandCoroutine = null;
            }
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

    IEnumerator WaitForStopAndExpand() {
        yield return WaitToStop();

        /*
        Step 01: Expand BindingCycle when velocity reach nearly 0
        */
        SpriteRenderer renderer = BindingCycle.GetComponent<SpriteRenderer>();
        Material material = renderer.material;
        BindingCycle.SetActive(true);

        float currentRadius;
        float elapsedTime = 0f;
        BindingCycle.transform.localScale = Vector3.one;// have to change localScale to 1 to get original size
        float originalSpriteSize = renderer.bounds.size.x;
        float scaleFactor;

        StartCoroutine(RotateBindingCycle());

        while (elapsedTime < expandDuration) {
            currentRadius = Mathf.Lerp(0f, radius, elapsedTime / expandDuration);
            scaleFactor = currentRadius * 2 / originalSpriteSize;
            BindingCycle.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

            float currentFade = Mathf.Lerp(0f, 1f, elapsedTime / expandDuration);
            material.SetFloat("_Fade", currentFade);

            elapsedTime += Time.deltaTime;
            DetectEnemies(currentRadius);
            yield return null;
        }

        /*
        Step 02: Rotate BindingCycle in lifeDuration, and always detect enemies in this duration
        */
        currentRadius = radius;
        scaleFactor = currentRadius * 2 / originalSpriteSize;
        BindingCycle.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        elapsedTime = 0f;
        while (elapsedTime < lifeDuration) {
            elapsedTime += Time.deltaTime;
            DetectEnemies(currentRadius);
            yield return null;
        }

        StartCoroutine(FadeOut());
    }

    void DetectEnemies(float checkRadius) {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, checkRadius, targetLayers);
        foreach (Collider2D e in enemies) {
            TakeDamage take = e.gameObject.GetComponent<TakeDamage>();
            if (take != null) {
                BindPlayer(take.player);
            }
        }
    }

    async void BindPlayer(PlayerManager player) {
        if (!effectedPlayers.Contains(player)) {
            effectedPlayers.Add(player);
            PlayerVFX playerVFX = player.GetComponent<PlayerVFX>();
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            playerVFX.SetEffect(BindingEffect, 0.5f);
            playerMovement.AddSpeedRatio(effectSpeed, effectDuration);
            await Task.Delay((int)(effectDuration * 1000));
            playerMovement.RemoveSpeedRatio(effectSpeed);
            playerVFX.ResetEffect(BindingEffect, 0f);
        }
    }

    IEnumerator RotateBindingCycle() {
        Vector3 rotationSpeed = new Vector3(0, 0, 120);
        while (true) {
            BindingCycle.transform.Rotate(rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator FadeOut() {
        float elapsedTime = 0f;

        SpriteRenderer renderer = BindingCycle.GetComponent<SpriteRenderer>();
        Material material = renderer.material;

        while (elapsedTime < fadeDuration) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            float currentFade = Mathf.Lerp(1f, 0f, elapsedTime / expandDuration);
            renderer.color = new Color(
                originColor.r,
                originColor.g,
                originColor.b,
                alpha
            );
            material.SetFloat("_Fade", currentFade);
            yield return null;
        }

        Destroy(gameObject);
    }
}
