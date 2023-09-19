using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private int _id;

    private EntityHealth _entityHealth;

    public int id { get { return _id; } }

   public void Init(EntityHealth entityHealth)
    {
        _id = gameObject.GetHashCode();
        _entityHealth = entityHealth;
    }

    private void Start()
    {

    }

    public void Damage(float dmg)
    {
        _entityHealth.TakeDamage(dmg);
    }
}
