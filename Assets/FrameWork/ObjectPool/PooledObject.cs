using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public ObjectPool ManagedPool { get; private set; }

    public void Dispose()
    {
        gameObject.SetActive(false);
    }
}
