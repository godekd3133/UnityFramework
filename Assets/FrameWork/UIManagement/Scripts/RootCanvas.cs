using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

public class RootCanvas : MonoBehaviour
{
    private static RootCanvas _instance = null;
    public static RootCanvas instance => _instance; // 싱글톤 접근자

    [SerializeField]
    [FormerlySerializedAs("UICamera")]
    private Camera uiCamera;

    [SerializeField]
    private RectTransform Scene;


    [SerializeField]
    private RectTransform Popup;

    [SerializeField]
    private RectTransform Overlay;

    [SerializeField]
    AssetReferenceGameObject EntryScene; // 최초로 등장시킬 장면 프리팹

    UIScene _currentScene;

    // 현재 scene 클래스를 가져올 수 있다
    public T GetCurrentScene<T>() where T : UIScene
    {
        return _currentScene as T;
    }

    public Camera UICamera => uiCamera;

    void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("RootCanvas instance is already registered");
            return;
        }

        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Bootup().Catch(e => Debug.LogException(e));
    }

    void OnDestroy()
    {
        // 씬 전환될 때 호출됨
        if (_instance == this)
        {
            _instance = null;
        }
    }

    async Task Bootup()
    {
        if (EntryScene == null)
            return;

        await ResourceManager.instance.PrepareLoading();

        // 최초 씬
        _ = SetScene(EntryScene);
    }

    /** SCENE **/
    public Task SetScene(string scenePath)
        => SetSceneInternal(ResourceManager.instance.LoadPrefab(scenePath));

    public Task SetScene(AssetReferenceT<GameObject> sceneRef)
        => SetSceneInternal(ResourceManager.instance.LoadPrefab(sceneRef));

    /** POPUP **/
    public Task<T> AddPopup<T>(string popupPath) where T : UIScene => AddPopupInternal<T>(ResourceManager.instance.LoadPrefab(popupPath));
    public Task<T> AddPopup<T>(AssetReferenceT<GameObject> popupRef) where T : UIScene => AddPopupInternal<T>(ResourceManager.instance.LoadPrefab(popupRef));
    public void RemovePopup(UIScene popup) => Popup.Remove(popup);
    public void RemoveLastPopup() => Popup.Pop();

    /** OVERLAY **/
    public Task<T> AddOverlay<T>(string overlayPath) where T : UIScene => AddOverlayInternal<T>(ResourceManager.instance.LoadPrefab(overlayPath));
    public Task<T> AddOverlay<T>(AssetReferenceT<GameObject> overlayRef) where T : UIScene => AddOverlayInternal<T>(ResourceManager.instance.LoadPrefab(overlayRef));
    public T AddOverlay<T>(T overlay) where T : UIScene => Overlay.Push(overlay);
    public void RemoveOverlay(UIScene overlay) => Overlay.Remove(overlay);

    Task SetSceneInternal(Task<GameObject> task) => task.Then(prefab =>
       {
           if (prefab != null)
           {
               var popup = prefab.GetComponent<UIScene>();
               Debug.Log($"SetScene from prefab {prefab.name}");
               _currentScene = Scene.Replace(prefab.GetComponent<UIScene>());
           }
           else
           {
               Debug.LogError("failed to load scene");

           }
       });





    Task<T> AddPopupInternal<T>(Task<GameObject> task) where T : UIScene => task.Then(prefab =>
    {
        if (prefab != null)
        {
            var popup = prefab.GetComponent<T>();
            Debug.Assert(popup);
            return Popup.Push(popup);
        }
        else
        {
            Debug.LogError("failed to load popup");
            return null;
        }
    });

    Task<T> AddOverlayInternal<T>(Task<GameObject> task) where T : UIScene => task.Then(prefab =>
    {
        if (prefab != null)
        {
            var overlay = prefab.GetComponent<T>();
            Debug.Assert(overlay);
            return Overlay.Push(overlay);
        }
        else
        {
            Debug.LogError("failed to load overlay");
            return null;
        }
    });


}

