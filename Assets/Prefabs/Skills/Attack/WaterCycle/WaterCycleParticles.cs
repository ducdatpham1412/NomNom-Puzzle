using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WaterCycleParticles : MonoBehaviour {
    [Header("VFXs")]
    [SerializeField] WaterCycle Cycle;
    ParticleSystem Ps;
    List<ParticleSystem.Particle> enterParticles = new List<ParticleSystem.Particle>();
    float radius = 0f;

    void Awake() {
        Ps = GetComponent<ParticleSystem>();
        GameObject[] players = GameObject.FindGameObjectsWithTag(Helper.Tag.Player.ToString());
        foreach (GameObject p in players) {
            Ps.trigger.AddCollider(p.GetComponent<Collider2D>());
        }
        radius = Ps.main.startSize.constant / 2;
    }

    void OnParticleTrigger() {
        Ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterParticles);
        foreach (ParticleSystem.Particle p in enterParticles) {
            Vector3 worldPos = Ps.transform.TransformPoint(p.position);
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, radius, Cycle.targetLayers);
            foreach (Collider2D h in hits) {
                TakeDamage take = h.gameObject.GetComponent<TakeDamage>();
                if (take != null && Cycle.HitTargetLayer(take.player.gameObject.layer)) {
                    SlowAndHitPlayer(take.player);
                }
            }
        }
    }

    public async void SlowAndHitPlayer(PlayerManager player) {
        if (!Cycle.effectedPlayers.Contains(player)) {
            Cycle.effectedPlayers.Add(player);
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            movement.AddSpeedRatio(Cycle.effectSpeed, Cycle.effectDuration);
            await Task.Delay((int)Cycle.effectDuration * 1000);
            movement.RemoveSpeedRatio(Cycle.effectSpeed);
            Cycle.effectedPlayers.Remove(player);
        }
    }
}
