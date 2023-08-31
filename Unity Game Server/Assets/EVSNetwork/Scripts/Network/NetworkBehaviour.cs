using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkedObject))]
public abstract class NetworkBehaviour : MonoBehaviour
{
    protected NetworkedObject _networkObject;
    protected Action<System.Object> OnObjectAdded;

    protected int id;
    protected string username;

    protected virtual void Awake()
    {
        _networkObject = GetComponent<NetworkedObject>();

        _networkObject.OnOwnerChange += OnOwnerChange;
        OnObjectAdded += OnNewObjectAdded;
    }

    protected virtual void OnDestroy()
    {
        _networkObject.OnOwnerChange += OnOwnerChange;
        OnObjectAdded -= OnNewObjectAdded;
    }

    private void OnOwnerChange(int id, string username)
    {
        this.id = id;
        this.username = username;
    }

    public void PassingObject(object obj)
    {
        OnObjectAdded?.Invoke(obj);
    }

    protected abstract void OnNewObjectAdded(object obj);

}
