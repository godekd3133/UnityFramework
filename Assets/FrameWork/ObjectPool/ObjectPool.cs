using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Permissions;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [HideInInspector]
    public PooledObject pooledObjectPrefab;

    public List<PooledObject> availableObjects { get; private set; }

    public int initalizeSize = 10;

    private void Awake()
    {
        this.availableObjects = new List<PooledObject>();
        this.transform.hierarchyCapacity = initalizeSize * 2;
    }

    private void Start()
    {
        for (int i = 0; i < initalizeSize; i++)
            CreateNewObject();
    }

    public GameObject RequestObject()
    {
        Debug.Assert(pooledObjectPrefab != null, "PooledObjectPrefab is null", this);

        if (availableObjects.Count == 0)
            CreateNewObject();

        PooledObject availableObject = availableObjects.Last();
        availableObjects.RemoveAt(availableObjects.Count - 1);
        //node를 바로 넣어주면 O(1)의 속도로 제거할 수 있다.
        availableObject.transform.SetAsFirstSibling();

        availableObject.gameObject.SetActive(true);
        availableObject.transform.position = Vector3.zero;
        availableObject.transform.localScale = Vector3.one;
        availableObject.transform.rotation = Quaternion.identity;

        return availableObject.gameObject;
    }

    private PooledObject CreateNewObject()
    {
        Debug.Assert(pooledObjectPrefab != null, "PooledObjectPrefab is null", this);

        PooledObject newObject = PooledObject.Instantiate(pooledObjectPrefab, this.transform);
        availableObjects.Add(newObject);
        newObject.ManagedPool = this;
        newObject.gameObject.name = pooledObjectPrefab.name;
        newObject.gameObject.SetActive(false);
        newObject.transform.SetParent(newObject.transform);
        return newObject;
    }
}
