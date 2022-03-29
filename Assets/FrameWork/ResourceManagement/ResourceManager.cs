using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

public class AssetCache<T> : IDisposable where T : Object
{
    // address -> operation_handle dict
    Dictionary<string, AsyncOperationHandle<T>> dict = new Dictionary<string, AsyncOperationHandle<T>>();
    
    public void Dispose()
    {
        foreach (var pair in dict)
        {
            Addressables.Release(pair.Value);
        }
    }

    public T Get(AssetReferenceT<T> reference)
    {
        if (reference?.RuntimeKeyIsValid() == true)
        {
            var handle = Addressables.LoadResourceLocationsAsync(reference);
            if (handle.IsValid() && handle.IsDone)
            {
                var address = handle.GetAwaiter().GetResult().FirstOrDefault()?.PrimaryKey ?? "(null)";
                return Get(address);
            }
        }

        // Debug.Assert(false, $"invalid {typeof(T)} reference");
        return default;
    }

    public T Get(string address)
    {
        // check address
        if (dict.TryGetValue(address, out var handle))
        {
            Debug.Assert(handle.IsDone, $"key {address} is not loaded yet");
            return handle.GetAwaiter().GetResult();
        }

        Debug.Assert(false, $"cannot find {typeof(T)} key {address}");
        return default;
    }



    public async Task<T> Store(AssetReferenceT<T> reference)
    {
        if (reference?.RuntimeKeyIsValid() == true)
        {
            var locations = await Addressables.LoadResourceLocationsAsync(reference, typeof(T));
            return await Store(locations.FirstOrDefault());
        }

        // Debug.Assert(false, $"cannot find {typeof(T)} key {reference}");
        return default;
    }


    public async Task<IList<T>> Store(AssetLabelReference labelReference)
    {
        if (labelReference?.RuntimeKeyIsValid() == true)
        {
            var locations = await Addressables.LoadResourceLocationsAsync(labelReference, typeof(T));
            var result = new List<T>(locations.Count);
            var tasks = locations.Select(loc => Store(loc).Then(asset =>
            {
                if (asset != null)
                    result.Add(asset);
            }));

            // 실제 로드 수행
            await Task.WhenAll(tasks);

            return result;
        }

        Debug.Assert(false, $"label reference {labelReference} is not valid");
        return null;
    }

    public async Task<T> Store(string address)
    {
        if (dict.ContainsKey(address))
            return await dict[address];

        if (string.IsNullOrEmpty(address))
            return default;

        var locations = await Addressables.LoadResourceLocationsAsync(address, typeof(T));
        return await Store(locations.FirstOrDefault());
    }



    public async Task<T> Store(IResourceLocation location)
    {
        if (location == null)
        {
            Debug.LogWarning("null location");
            return default;
        }

        try
        {
            var handle = dict[location.PrimaryKey] = Addressables.LoadAssetAsync<T>(location);
            var loadedAsset = await handle;
            if (loadedAsset != null)
            {
                dict[location.PrimaryKey] = handle;
                // Debug.Log($"{location.PrimaryKey} / {location.InternalId}");
            }


            return loadedAsset;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return default;
        }
    }
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;

    public AssetCache<GameObject> Prefabs { get; private set; }
    public AssetCache<GameObject> Effects { get; private set; }
    public AssetCache<Sprite> Sprites { get; private set; }
    public AssetCache<ScriptableObject> ScriptableObjects { get; private set; }

    public HashSet<Task> _tasks { get; private set; }
    TaskCompletionSource<bool> _initTask;

    public int loadQueueCount => _tasks.Count;
    public bool emptyLoadQueue => _tasks.Count == 0;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            _tasks = new HashSet<Task>();
            Prefabs = new AssetCache<GameObject>();
            Effects = new AssetCache<GameObject>();
            Sprites = new AssetCache<Sprite>();
            ScriptableObjects = new AssetCache<ScriptableObject>();

            TaskScheduler.UnobservedTaskException += ProcessUnhandledException;
            Debug.Log("Awake", this);

            // 싱글톤
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            Prefabs.Dispose();
            Effects.Dispose();
            Sprites.Dispose();
            ScriptableObjects.Dispose();
            TaskScheduler.UnobservedTaskException -= ProcessUnhandledException;
            instance = null;
        }
    }

    void CheckLoaded()
    {
        Debug.Assert(emptyLoadQueue);
    }

    // 태스크 내에서 처리되지 않은 예외를 보고해주는 함수
    void ProcessUnhandledException(object sender, UnobservedTaskExceptionEventArgs args) => args.Exception.Handle(e =>
    {
        Debug.LogError($"error occured: {sender}");
        Debug.LogException(e);
        return true;
    });

    //최초 1회 로딩 전에 호출해야될 기능이 있다면 (중복호출 X)
    public async Task PrepareLoading()
    {

        if (_initTask == null)
        {
            _initTask = new TaskCompletionSource<bool>();

            //여기에 사전 작업목록 작성
            await Addressables.InitializeAsync();


            _initTask.SetResult(true);
        }
        else
        {
            await _initTask.Task;
        }
    }

    async Task<T> trackLoadingStatus<T>(Task<T> operation)
    {
        _tasks.Add(operation);

        try
        {
            return await operation;
        }
        catch (Exception e)
        {
            Debug.LogError($"failed to load: {e.Message}");
            return default;
        }
        finally
        {
            _tasks.Remove(operation);
        }
    }

    public Task<GameObject> LoadEffect(AssetReferenceT<GameObject> reference) => trackLoadingStatus(Effects.Store(reference));
    public Task<GameObject> LoadEffect(string address) => trackLoadingStatus(Effects.Store(address));

    public Task<GameObject> LoadPrefab(AssetReferenceT<GameObject> reference) => trackLoadingStatus(Prefabs.Store(reference));
    public Task<GameObject> LoadPrefab(string address) => trackLoadingStatus(Prefabs.Store(address));

    public Task<Sprite> LoadSprite(AssetReferenceT<Sprite> reference) => trackLoadingStatus(Sprites.Store(reference));
    public Task<Sprite> LoadSprite(string address) => trackLoadingStatus(Sprites.Store(address));

    public Task<ScriptableObject> LoadScriptableObject(AssetReferenceT<ScriptableObject> reference) => trackLoadingStatus(ScriptableObjects.Store(reference));
    public Task<ScriptableObject> LoadScriptableObject(string address) => trackLoadingStatus(ScriptableObjects
    .Store(address));

}



