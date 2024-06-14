using UnityEngine;

public struct ProjectileData {
    public Unit unit;
    public Rigidbody2D rigidbody;
    public float speed;
    public Vector2 direction;
    public float maxDistance;
    public float coveredDistance;
}

public class ProjectileExtension : IExtension<ProjectileData>
{
    public void Init(Unit unit, ref ProjectileData extensionData)
    {
        extensionData.unit = unit;
        extensionData.rigidbody = unit.gameObject.GetComponent<Rigidbody2D>();
    }

    public void Update(ref ProjectileData extensionData)
    {
        Unit unit = extensionData.unit;
        var speed = extensionData.speed;
        var direction = extensionData.direction;
        var distanceToCover = speed * Time.deltaTime;
        var newPos = (Vector2)unit.gameObject.transform.position + direction * distanceToCover;
        if(extensionData.rigidbody) {
            extensionData.rigidbody.MovePosition(newPos);
        } else {
            unit.gameObject.transform.position = newPos;
        }

        extensionData.coveredDistance += distanceToCover;
        if(extensionData.coveredDistance >= extensionData.maxDistance) {
            Managers.unitSpawner.MarkForDeletion(unit);
        }

        extensionData.unit.gameObject.transform.rotation = Utils2D.LookRotation(direction);
    }
    public void Destroy(Unit unit) {}
}