using System.Threading.Tasks;
using UnityEngine;

public class MagmaParticles : MonoBehaviour {
    [Header("VFXs")]
    [SerializeField] Sprite BurnEffect;
    [SerializeField] MagmaFire Fire;

    void OnParticleCollision(GameObject other) {
        TakeDamage take = other.gameObject.GetComponent<TakeDamage>();
        if (take != null && Fire.HitTargetLayer(take.player.gameObject.layer) && !Fire.effectedPlayers.Contains(take.player)) {
            BurnPlayer(take.player);
        }
    }

    public async void BurnPlayer(PlayerManager player) {
        if (!Fire.effectedPlayers.Contains(player)) {
            Fire.effectedPlayers.Add(player);
            PlayerVFX playerVFX = player.GetComponent<PlayerVFX>();
            playerVFX.SetSkin(BurnEffect, 0.2f);
            await Task.Delay((int)Fire.effectDuration * 1000);
            playerVFX.ResetSkin(BurnEffect, 0.1f);
            Fire.effectedPlayers.Remove(player);
        }
    }
}
