using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedObject : MonoBehaviour
{
    public int id;
    public string userName;

    public Action<int, string> OnOwnerChange;

    private void Awake()
    {
        if (!NetworkManager.networkObjects.ContainsKey(id))
        {
            NetworkManager.networkObjects.Add(id, this);
        }
    }

    public virtual void Initialize(int id, string userName)
    {
        this.id = id;
        this.userName = userName;

        OnOwnerChange?.Invoke(id, userName);
    }

    /// <summary>
    /// That should work when placing a network object in the scene to give it unique id, work when we have a merge
    /// of Server and client project
    /// </summary>
    private void BakeNetworkObject()
    {
        if (id == 0)
        {
            var obj = NetworkManager.InitializeNetworkObject(this);

            Initialize(obj.id, obj.userName);
        }
    }
}
