using UnityEngine;

public struct MoverExtensionData {
    public Rigidbody2D rigidbody;
}

public class MoverExtension : IExtension<MoverExtensionData>
{
    public void Init(Unit unit, ref MoverExtensionData extensionData)
    {
        extensionData.rigidbody = unit.gameObject.GetComponent<Rigidbody2D>();
    }

    public void Update(ref MoverExtensionData extensionData) {}

    public void Move(Unit unit, Vector2 move) {
        if(move == Vector2.zero) return;

        ref var data = ref ExtensionHandler<MoverExtension, MoverExtensionData>.GetData(unit);
        var newPos = (Vector2)unit.gameObject.transform.position + move * Time.deltaTime;
        if(data.rigidbody) {
            data.rigidbody.MovePosition(newPos);
        } else {
            unit.gameObject.transform.position = newPos;
        }

        unit.gameObject.transform.rotation = Utils2D.LookRotation(move);
    }
    
    public void Destroy(Unit unit) {}
}
