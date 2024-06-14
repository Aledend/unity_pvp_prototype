using UnityEngine;

public static class Utils2D
{
    private static Vector3 realUp = -Vector3.forward;
    private static Quaternion rotationConversion = Quaternion.AngleAxis(90, Vector3.right);
    public static Quaternion LookRotation(Vector2 direction) {
        return Quaternion.LookRotation(direction, realUp) * rotationConversion;
    }

    public static Vector2 LookDirection(Quaternion rotation) {
        return rotation * rotationConversion * realUp;
    }
}