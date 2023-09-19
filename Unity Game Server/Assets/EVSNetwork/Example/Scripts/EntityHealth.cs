using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHealth : NetworkBehaviour
{
    public float maxHealth = 100f;

    protected float _health;
    protected Hitbox[] _hitboxes;

    public float health { get { return _health; } }
    public Hitbox[] hitboxes { get { return _hitboxes; } }

    protected override void Awake()
    {
        base.Awake();

        _health = maxHealth;
        _hitboxes = GetComponentsInChildren<Hitbox>();
        foreach (Hitbox hitbox in _hitboxes)
        {
            hitbox.Init(this);
        }
    }

    protected virtual void Start()
    {
        RollbackManager.intance.AddEntityHealth(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public virtual void TakeDamage(float damage)
    {
        ServerSend.EntityHealth(this, typeof(EntityHealth));
        Debug.Log($"Entity {ID} Got Hit {damage}");
    }

    protected virtual IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        ServerSend.EntityRespawned(this, typeof(PlayerHealth));
    }

    protected override void OnNewObjectAdded(object obj)
    {
        //  throw new System.NotImplementedException();
    }

    public Hitbox GetHitboxByID(int id)
    {
        foreach (var hitbox in hitboxes)
        {
            if (hitbox.id == id)
                return hitbox;
        }
        return null;
    }
}
