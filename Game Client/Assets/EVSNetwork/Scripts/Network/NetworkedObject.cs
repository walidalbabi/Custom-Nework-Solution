using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedObject : MonoBehaviour
{
    public int id;
    public string userName;

    public Action<int,string> OnOwnerChange;

    protected NetworkBehaviour[] networkBehviours;

    protected virtual void Awake()
    {
        networkBehviours = GetComponents<NetworkBehaviour>();

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

    public void SendAction(object obj, Type type)
    {
        if(networkBehviours.Length > 0)
        {
            foreach (var tp in networkBehviours)
            {
                if (tp.GetType().Equals(type))
                {
                    if (tp != null)
                        tp.PassingObject(obj);
                }
            }
        }
    }

    /// <summary>
    /// That should work when placing a network object in the scene to give it unique id, work when we have a merge
    /// of Server and client project, Set that to work in the editor in the future
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
