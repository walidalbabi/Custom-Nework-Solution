using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : EntityHealth
{
    [SerializeField] private MeshRenderer _model;
    [SerializeField] private Material _hitMaterial;

    private Material _defaulthMaterial;

    private MeshRenderer _mr;

    protected override void Awake()
    {
        base.Awake();

        _mr = GetComponentInChildren<MeshRenderer>();

        _defaulthMaterial = _mr.material;
    }

    protected override void OnNewObjectAdded(object obj)
    {
        base.OnNewObjectAdded(obj);
    }

    protected override void Dead()
    {
        base.Dead();
        _model.enabled = false;
    }
    protected override void EntityHit()
    {
        base.EntityHit();
        StartCoroutine(HitEffect());
    }

    protected override void Respawn()
    {
        base.Respawn();
        _model.enabled = true;
    }

    private IEnumerator HitEffect()
    {
        _mr.material = _hitMaterial;
        yield return new WaitForSeconds(0.2f);
        _mr.material = _defaulthMaterial;
    }
}
