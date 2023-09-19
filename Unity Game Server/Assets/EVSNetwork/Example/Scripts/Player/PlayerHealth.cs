using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : EntityHealth
{

    private CharacterController _characterController;

    protected override void Awake()
    {
        base.Awake();
        _characterController = GetComponent<CharacterController>();

        _health = maxHealth;
        _hitboxes = GetComponentsInChildren<Hitbox>();
        foreach (Hitbox hitbox in _hitboxes)
        {
            hitbox.Init(this);
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
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
            if (_characterController != null)
                _characterController.enabled = false;

            transform.position = new Vector3(0f, 50f, 0f);
            StartCoroutine(Respawn());
        }

        ServerSend.EntityHealth(this, typeof(PlayerHealth));
    }

    protected override IEnumerator Respawn()
    {
        transform.position = NetworkManager.instance.GetARandomSpawnPoint().position;
        yield return new WaitForSeconds(5f);

        _health = maxHealth;

        if (_characterController != null)
            _characterController.enabled = true;

        ServerSend.EntityRespawned(this, typeof(PlayerHealth));
    }

    protected override void OnNewObjectAdded(object obj)
    {
      //  throw new System.NotImplementedException();
    }

}
