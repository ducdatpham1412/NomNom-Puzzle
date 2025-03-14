using UnityEngine;

public class BulletTrajectory : MonoBehaviour {
    LineRenderer lineRenderer;
    float timeStep = 0.1f;
    float gravity = -1;
    [SerializeField] LayerMask layerMask;
    [SerializeField] PlayerSkill Owner;


    void Start() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetOwner(PlayerSkill manager) {
        Owner = manager;
        string layer = LayerMask.LayerToName(manager.gameObject.layer);
        if (layer == Helper.Layer.PlayerBlue.ToString()) {
            layerMask = LayerMask.GetMask(Helper.Layer.PlayerRed.ToString(), Helper.Layer.Environment.ToString());
        }
        else {
            layerMask = LayerMask.GetMask(Helper.Layer.PlayerBlue.ToString(), Helper.Layer.Environment.ToString());
        }
    }

    public void DrawParabol(Vector3 startPos, Vector3 direction, float speed) {
        if (lineRenderer == null) return;

        int maxPointCount = 200;

        lineRenderer.positionCount = maxPointCount;
        Vector3[] points = new Vector3[maxPointCount];
        Vector3 velocity = direction * speed;

        for (int i = 0; i < maxPointCount; i++) {
            float t = i * timeStep;
            Vector3 point = startPos + velocity * t + 0.5f * new Vector3(0, gravity, 0) * t * t;
            points[i] = point;

            if (i > 0 && point.y <= 0) {
                lineRenderer.positionCount = i + 1;
                break;
            }
        }

        lineRenderer.SetPositions(points);
    }

    public void DrawParabolToDestination(Vector3 startPos, Vector3 des, float speed) {
        Debug.Log("TO DO");
    }

    public void DrawStraight(Vector3 startPos, Vector3 direction, float radius = 5f) {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = 2;
        Vector3 lastPoint = startPos + radius * direction;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float distance = (lastPoint - startPos).magnitude;
        RaycastHit2D hit = Physics2D.BoxCast(startPos, Owner.Pool.bulletSize, angle, direction, distance, layerMask);

        //  RaycastHit2D hit = Physics2D.Raycast(startPos, direction, (lastPoint - startPos).magnitude, layerMask, Owner.Pool.bulletSize.y);

        // int i = 0;
        // while (true) {
        //     bool shouldBreak = false;
        //     RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, direction, i * direction.magnitude, layerMask);
        //     Debug.Log("Hits length: " + hits.Length);
        //     foreach (RaycastHit2D hit in hits) {
        //         if ((layerMask & (1 << hit.collider.gameObject.layer)) != 0) {
        //             TakeDamage takeDamage = hit.collider.gameObject.GetComponent<TakeDamage>();
        //             if (takeDamage != null) {
        //                 if (takeDamage.player.team != Owner.team) {
        //                     lastPoint = hit.point;
        //                     Debug.Log("Last point position: " + hit.point);
        //                     shouldBreak = true;
        //                     break;
        //                 }
        //             }
        //         }
        //     }
        //     if (shouldBreak) {
        //         break;
        //     }
        //     if (i == radius) {
        //         lastPoint = startPos + i * direction;
        //         break;
        //     }
        //     i++;
        // }

        Vector3[] points = new Vector3[2] { startPos, hit.collider ? hit.point : lastPoint };
        lineRenderer.SetPositions(points);
    }

    public void RemoveLine() {
        lineRenderer.positionCount = 0;
    }
}