using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public ObjectPool ManagedPool;

    public void Dispose()
    {
        gameObject.SetActive(false);
    }
}
