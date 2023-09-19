using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHealth : NetworkBehaviour
{
    [SerializeField] protected float _health;

    protected override void OnNewObjectAdded(object obj)
    {
        //Deal damage
        if (obj.GetType() == typeof(float))
        {
            _health = (float)obj;

            if (_health <= 0)
            {
                Dead();
                return;
            }

            EntityHit();
        }

        //Revive
        if (obj.GetType() == typeof(string))
        {
            Respawn();
        }

    }


    protected virtual void Dead()
    {
   
    }
    protected virtual void EntityHit()
    {

    }
    protected virtual void Respawn()
    {
        _health = 100f;
    }
}
