using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolTest : MonoBehaviour
{
    public KeyCode createKey = KeyCode.A;
    public int initializeSize = 20;
    public float lifeDuraction = 5f;
    public PooledObject prefab = null;
    private ObjectPool objectPool;

    void Start()
    {
        objectPool = gameObject.AddComponent<ObjectPool>();
        objectPool.pooledObjectPrefab = this.prefab;
        objectPool.initalizeSize = this.initializeSize;

    }

    void Update()
    {
        if (Input.GetKeyDown(createKey))
        {

            objectPool.RequestObject().GetComponent<PooledObjectTest>().DestroyTimer(lifeDuraction);

        }
    }
}
