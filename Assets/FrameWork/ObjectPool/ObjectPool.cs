using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public PooledObject pooledObjectPrefab;
    public LinkedList<PooledObject> pooledObjects { get; private set; }

    public int initalizeSize = 10;

    private void Awake()
    {
        this.pooledObjects = new LinkedList<PooledObject>();
    }
    private void Start()
    {
        for (int i = 0; i < initalizeSize; i++)
            CreateNewObject();
    }

    public GameObject RequestObject()
    {
        Debug.Assert(pooledObjectPrefab != null, "PooledObjectPrefab is null", this);

        PooledObject poolObject = null;

        var findResult = FindUseableObject();
        if (findResult == null)
            poolObject = CreateNewObject();
        else
        {
            poolObject = findResult.Value;
            //node를 바로 넣어주면 O(1)의 속도로 제거할 수 있다.
            pooledObjects.Remove(findResult);
            pooledObjects.AddFirst(poolObject);
            poolObject.transform.SetAsFirstSibling();
        }
        poolObject.gameObject.SetActive(true);
        poolObject.transform.position = Vector3.zero;
        poolObject.transform.localScale = Vector3.one;
        poolObject.transform.rotation = Quaternion.identity;

        return poolObject.gameObject;
    }

    private LinkedListNode<PooledObject> FindUseableObject()
    {
        LinkedListNode<PooledObject> iteratorBottomUp = pooledObjects.First;
        LinkedListNode<PooledObject> iteratorTopDown = pooledObjects.Last;

        while (iteratorBottomUp.Next != null)
        {
            if (iteratorTopDown.Value.gameObject.activeSelf == false)
                return iteratorTopDown;
            if (iteratorBottomUp.Value.gameObject.activeSelf == false)
                return iteratorBottomUp;
            iteratorBottomUp = iteratorBottomUp.Next;
            iteratorTopDown = iteratorTopDown.Previous;
        }
        return null;
    }
    private PooledObject CreateNewObject()
    {
        Debug.Assert(pooledObjectPrefab != null, "PooledObjectPrefab is null", this);

        PooledObject newObject = PooledObject.Instantiate(pooledObjectPrefab, this.transform);
        pooledObjects.AddFirst(newObject);
        newObject.gameObject.name = $"PooledObject ({pooledObjects.Count})";
        newObject.gameObject.SetActive(false);
        newObject.transform.SetParent(newObject.transform);
        return newObject;
    }
}
