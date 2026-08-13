using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    #region Fields
    [Header("Collider Settings:")] [Range(0, 1f)] [SerializeField]
    private float stepHeightRatio = 0.1f;

    [SerializeField] private float colliderHeight = 2f;
    [SerializeField] private float colliderThickness = 1f;
    [SerializeField] Vector3 colliderOffset = Vector3.zero;

    Rigidbody rb;
    Transform tr;
    CapsuleCollider cc;
    RaycastSensor sensor;

    private bool isGrounded;
    private float baseSensorRange;
    private Vector3 currentGroundAdjustmentVelocity;
    private int currentLayer;


    [Header("Sensor Settings:")] [SerializeField]
    private bool isInDebugMode;
    private bool isUsingExtendedSensorRange = true;
    #endregion

    private void Awake()
    {
        Setup();
        RecalculateColliderDimensions();
    }
    
    void OnValidate()
    {
        if(gameObject.activeInHierarchy)
        {
            RecalculateColliderDimensions();
        }
    }

    void Setup()
    {
        tr = transform;
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();
        
        rb.freezeRotation = true;
        rb.useGravity = false;
    }
    
    public void CheckForGround()
    {
        if (currentLayer != gameObject.layer)
        {
            RecalculateSensorLayerMask();
        }
        
        currentGroundAdjustmentVelocity = Vector3.zero;
        sensor.castLength = isUsingExtendedSensorRange ? 
            baseSensorRange + colliderHeight * tr.localScale.x * stepHeightRatio 
            : baseSensorRange;
        sensor.Cast();
        
        isGrounded = sensor.HasDetectedHit();
        if (!isGrounded) return;
        
        float distance = sensor.GetDistance();
        float upperLimit = colliderHeight * tr.localScale.x * (1f - stepHeightRatio) * 0.5f;
        float middle = upperLimit + colliderHeight * tr.localScale.x * stepHeightRatio;
        float distanceToGo = middle - distance;
        
        currentGroundAdjustmentVelocity = tr.up * (distanceToGo / Time.fixedDeltaTime);
    }
    public bool IsGrounded() => isGrounded;
    public Vector3 GetGroundNormal() => sensor.GetNormal();
    
    public void SetVelocity(Vector3 velocity) => rb.linearVelocity = velocity + currentGroundAdjustmentVelocity;
    public void SetExtendedSensorRange(bool isExtended) => isUsingExtendedSensorRange = isExtended;
    
    private void RecalculateColliderDimensions()
    {
        if (cc == null)
        {
            Setup();
        }
        
        cc.height = colliderHeight * (1f - stepHeightRatio);
        cc.radius = colliderThickness / 2f;
        cc.center = colliderOffset * colliderHeight + new Vector3(0f, stepHeightRatio * cc.height / 2f, 0f);

        if (cc.height / 2f < cc.radius)
        {
            cc.radius = cc.height / 2f;
        }

        RecalibrateSensor();
    }

    private void RecalibrateSensor()
    {
        sensor ??= new RaycastSensor(tr);
        
        sensor.SetCastOrigin(cc.bounds.center);
        sensor.SetCastDirection(RaycastSensor.CastDirection.Down);
        RecalculateSensorLayerMask();

        const float safetyDistanceFactor = 0.001f; // Small factor added to prevent clipping issues when the sensor range is calculated

        float length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
        baseSensorRange = length * (1f - safetyDistanceFactor) * tr.localScale.x;
        sensor.castLength = length * tr.localScale.x;
        
        
    }

    private void RecalculateSensorLayerMask()
    {
        int objectLayer = gameObject.layer;
        int layerMask = Physics.AllLayers;

        for (int i = 0; i < 32; i++)
        {
            if(Physics.GetIgnoreLayerCollision(objectLayer, i))
            {
                layerMask &= ~(1 << i);
            }
        }
        
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        layerMask &= ~(1 << ignoreRaycastLayer);
        
        sensor.layerMask = layerMask;
        currentLayer = objectLayer;
    }
}