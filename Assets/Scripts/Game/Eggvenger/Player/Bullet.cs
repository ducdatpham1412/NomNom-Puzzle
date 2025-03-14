using UnityEngine;

public class Bullet : MonoBehaviour {
    public GunStats.BulletStats stats;
    public BulletPool Pool;
    int targetLayers;

    private void Update() {
        transform.Translate(Vector3.right * stats.speed * Time.deltaTime);
    }

    protected bool HitTargetLayer(int hitLayer) {
        return (targetLayers & (1 << hitLayer)) != 0;
    }

    public void SetOwner(PlayerManager Owner) {
        if (LayerMask.LayerToName(Owner.gameObject.layer) == Helper.Layer.PlayerBlue.ToString()) {
            targetLayers = LayerMask.GetMask(Helper.Layer.PlayerRed.ToString());
        }
        else {
            targetLayers = LayerMask.GetMask(Helper.Layer.PlayerBlue.ToString());
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (HitTargetLayer(collider.gameObject.layer)) {
            TakeDamage take = collider.GetComponent<TakeDamage>();
            if (take != null) {
                Pool.ReturnBullet(this);
                // TODO: Attack player
            }
            return;
        }

        string layer = LayerMask.LayerToName(collider.gameObject.layer);
        if (layer == Helper.Layer.Environment.ToString()) {
            string tag = collider.gameObject.tag;
            if (tag == Helper.Tag.Obstacle.ToString()) {
                Pool.ReturnBullet(this);
            }
            else if (tag == Helper.Tag.Item.ToString()) {
                Pool.ReturnBullet(this);
                // TODO: Destroy Item
            }
        }
    }
}
