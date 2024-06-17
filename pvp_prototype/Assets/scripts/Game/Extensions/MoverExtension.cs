using UnityEngine;

public struct MoverExtensionData {
    public Rigidbody2D rigidbody;
}

public class MoverExtension : Extension<MoverExtensionData>
{
    public override void Init(Unit unit, ExtensionInitContext extensionInitContext, ref MoverExtensionData extensionData)
    {
        extensionData.rigidbody = unit.GameObject().GetComponent<Rigidbody2D>();
    }

    public override void Update(ref MoverExtensionData extensionData) {}

    public virtual void Move(Unit unit, Vector2 move) {
        if(move == Vector2.zero) return;

        ref var data = ref ExtensionHandler<MoverExtension, MoverExtensionData>.GetData(unit);
        var gameObject = unit.GameObject();
        var newPos = (Vector2)gameObject.transform.position + move * Time.deltaTime;
        if(data.rigidbody) {
            data.rigidbody.MovePosition(newPos);
        } else {
            gameObject.transform.position = newPos;
        }

        gameObject.transform.rotation = Utils2D.LookRotation(move);
    }
    
    public override void Destroy(Unit unit) {}
}


public class PlayerMoverExtension : MoverExtension
{
    public override void Move(Unit unit, Vector2 move)
    {
        base.Move(unit, move);
    }
}