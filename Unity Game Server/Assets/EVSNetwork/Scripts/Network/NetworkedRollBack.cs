using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedRollBack : NetworkBehaviour
{
    private Hitbox[] _hitboxes;

    protected override void OnNewObjectAdded(object obj)
    {
     
    }

    protected override void Awake()
    {
        base.Awake();

     //   TimeManager.OnTick += OnTick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        //TimeManager.OnTick = OnTick;
    }

    // Start is called before the first frame update
    void Start()
    {
        _hitboxes = GetComponentsInChildren<Hitbox>();
    }

    public void SetSnapshots()
    {
       // RollbackManager.intance.()
    }

}
