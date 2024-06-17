using UnityEngine;

public struct ProjectileData {
    public Unit unit;
    public Rigidbody2D rigidbody;
    public float speed;
    public Vector2 direction;
    public float maxDistance;
    public float coveredDistance;
}

public class ProjectileExtension : Extension<ProjectileData>
{
    public override void Init(Unit unit, ExtensionInitContext extensionInitContext, ref ProjectileData extensionData)
    {
        extensionData.unit = unit;
        extensionData.rigidbody = unit.GameObject().GetComponent<Rigidbody2D>();
    }

    public override void Update(ref ProjectileData extensionData)
    {
        Unit unit = extensionData.unit;
        var speed = extensionData.speed;
        var direction = extensionData.direction;
        var distanceToCover = speed * Time.deltaTime;
        var gameObject = unit.GameObject();
        var newPos = (Vector2)gameObject.transform.position + direction * distanceToCover;
        if(extensionData.rigidbody) {
            extensionData.rigidbody.MovePosition(newPos);
        } else {
            unit.GameObject().transform.position = newPos;
        }

        extensionData.coveredDistance += distanceToCover;
        if(extensionData.coveredDistance >= extensionData.maxDistance) {
            Managers.unitSpawner.MarkForDeletion(unit);
        }

        gameObject.transform.rotation = Utils2D.LookRotation(direction);
    }
    public override void Destroy(Unit unit) {}
}