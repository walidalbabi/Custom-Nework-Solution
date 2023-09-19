using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollHealth : EntityHealth
{
    [SerializeField] private MeshRenderer module;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void TakeDamage(float damage)
    {
        if (_health <= 0f)
        {
            return;
        }
        Debug.Log($"Entity {ID} Got Hit {damage}");
        _health -= damage;

        if (_health <= 0f)
        {
            _health = 0f;
            module.enabled = false;
            StartCoroutine(Respawn());
        }

        ServerSend.EntityHealth(this, typeof(DollHealth));
    }

    protected override IEnumerator Respawn()
    {
        yield return new WaitForSeconds(10f);
        module.enabled = true;

        _health = maxHealth;

        ServerSend.EntityRespawned(this, typeof(DollHealth));
    }

    protected override void OnNewObjectAdded(object obj)
    {
        //  throw new System.NotImplementedException();
    }
}
