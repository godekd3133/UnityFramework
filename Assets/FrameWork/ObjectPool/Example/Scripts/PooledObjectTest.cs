using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PooledObjectTest : MonoBehaviour
{
    void Awake()
    {
        Debug.Log($"Created New PooledObject {gameObject.name}");
    }

    void OnDisable()
    {
        Debug.Log($"Disposed PooledObject {gameObject.name}");
    }

    void OnEnable()
    {
        Debug.Log($"Enabled PooledObject {gameObject.name}");
    }

    public async Task DestroyTimer(float duration)
    {
        await UniTask.Delay((int)(duration * 1000f));
        GetComponent<PooledObject>().Dispose();
    }
}
