using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EternalVision.Debuging;


public struct ColliderRollData
{
    public int tick;
    public Dictionary<int, HitboxData> snapshotData;

    public void Reset(int tick)
    {
        this.tick = tick;
        snapshotData = new Dictionary<int, HitboxData>();
    }

    public void AddSnapshot(int key,HitboxData data)
    {
        snapshotData.Add(key, data);
    }
}

public struct HitboxData
{
    public int id;
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;
}

public class RollbackManager : MonoBehaviour
{
    public static RollbackManager intance;

    [SerializeField]private List<EntityHealth> _entitiesHealth = new List<EntityHealth>();

    private ColliderRollData[] _rollbackBuffer;
    private int _bufferIndex;
    private bool _isRollingBack;

    private ColliderRollData data = new ColliderRollData();
    private HitboxData hitboxData = new HitboxData();
    private void Awake()
    {
        if (intance == null) intance = this;
        else Destroy(gameObject);

        TimeManager.OnTick += OnTick;

        _rollbackBuffer = new ColliderRollData[1024];

        for (int i = 0; i < _rollbackBuffer.Length; i++)
        {
            _rollbackBuffer[i].Reset(-1);
        }
    }

    private void OnDestroy()
    {
        TimeManager.OnTick -= OnTick;
    }


    private void OnTick(int tick)
    {
        if (_entitiesHealth.Count <= 0) return;

        _bufferIndex = tick % 1024;

        data.Reset(tick);
        foreach (var entity in _entitiesHealth)
        {
            if (entity == null) return;

            foreach (var hitbox in entity.hitboxes)
            {
                hitboxData.id = hitbox.id;
                hitboxData.position = hitbox.transform.position;
                hitboxData.rotation = hitbox.transform.rotation;
                data.AddSnapshot(entity.ID, hitboxData);
            }
        }

        StoreCollidersAtBufferIndex(_bufferIndex, data);
    }

    public void AddEntityHealth(EntityHealth entityHealth)
    {
        Debug.Log("Hitbox Added");
        _entitiesHealth.Add(entityHealth);
    }

    public void StoreCollidersAtBufferIndex(int bufferIndex, ColliderRollData state)
    {
        if (!_isRollingBack)
            _rollbackBuffer[bufferIndex] = state;
    }

    public void DoRollbackAll(int exceptClient, int tick)
    {
        _isRollingBack = true;

        int bufferIndex = tick % 1024;
        var targetRollback = _rollbackBuffer[bufferIndex];

        if (targetRollback.snapshotData == null)
        {
            Debug.LogError("TargetRollback Data is null");
            return;
        }

        foreach (var roll in targetRollback.snapshotData)
        {
            EntityHealth entity = FindEntityWithID(roll.Key);

            // We skip entities that don't exist or are associated with the 'exceptClient'.
            if (entity == null || entity.ID == exceptClient) continue;

            // Since roll.Value is HitboxData, we don't need an inner loop to iterate it.
            HitboxData hitboxData = roll.Value;

            Hitbox hitbox = entity.GetHitboxByID(hitboxData.id);
            if (hitbox)
            {
                // Store the current position and rotation.
                Vector3 originalPosition = hitbox.transform.position;
                Quaternion originalRotation = hitbox.transform.rotation;

                EVDebug.DrawBox(hitbox.transform.position, hitbox.transform.rotation, 
                    hitbox.transform.lossyScale,Color.red, 5f);

                // Set the rollback position and rotation.
                hitbox.transform.position = hitboxData.position;
                hitbox.transform.rotation = hitboxData.rotation;

                // Here, you might want to check for collisions or other game logic.
                // ...

                // Reset hitbox to its original state.
                hitbox.transform.position = originalPosition;
                hitbox.transform.rotation = originalRotation;
            }
        }
        _isRollingBack = false;
    }


    EntityHealth FindEntityWithID(int id)
    {
        foreach (var entity in _entitiesHealth)
        {
            if (entity.ID == id)
                return entity;
        }
        return null;
    }
}
