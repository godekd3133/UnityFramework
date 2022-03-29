using UnityEngine;

public static class GameObjectExtension
{
    public static T GetComponentNoAlloc<T>(this GameObject gameObject)
    {
        return gameObject.TryGetComponent(out T component) ? component : default;
    }

    public static T GetComponentNoAlloc<T>(this Component component)
    {
        return component.TryGetComponent(out T result) ? result : default;
    }
}