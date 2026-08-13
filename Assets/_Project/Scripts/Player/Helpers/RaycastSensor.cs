using UnityEngine;

public class RaycastSensor
{
    public float castLength = 1f;
    public LayerMask layerMask = 225;
    
    Vector3 origin = Vector3.zero;
    private Transform tr;

    public enum CastDirection
    {
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    };

    private CastDirection castDirection;
    RaycastHit hit;

    public RaycastSensor(Transform playerTransform)
    {
        tr = playerTransform;
    }

    public void Cast()
    {
        var worldOrigin = tr.TransformPoint(origin);
        var worldDirection = GetCastDirection();
        
        Physics.Raycast(worldOrigin, worldDirection, out hit, castLength, layerMask, QueryTriggerInteraction.Ignore);
    }
    
    public bool HasDetectedHit() => hit.collider != null;
    public float GetDistance() => hit.distance;
    public Vector3 GetNormal() => hit.normal;
    public Vector3 GetPosition() => hit.point;
    public Collider GetCollider() => hit.collider;
    public Transform GetTransform() => hit.transform;
    
    public void SetCastDirection(CastDirection direction) => castDirection = direction;
    public void SetCastOrigin(Vector3 pos) => origin = tr.InverseTransformPoint(pos);
    
    Vector3 GetCastDirection()
    {
        return castDirection switch
        {
            CastDirection.Forward => tr.forward,
            CastDirection.Right => tr.right,
            CastDirection.Up => tr.up,
            CastDirection.Backward => -tr.forward,
            CastDirection.Left => -tr.right,
            CastDirection.Down => -tr.up,
            _ => Vector3.one
        };
    }
}