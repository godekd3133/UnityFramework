using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    public static SceneLoader current { get; private set; }

    public readonly List<Func<Task>> AdditionalLoadingTasks = new List<Func<Task>>();
    string _scene;
    public static Task LoadScene(string scene)
    {
        if (current != null)
            throw new Exception("already running");

        current = new SceneLoader() { _scene = scene };
        return current.DoLoad()
        .Catch(e => Debug.LogException(e))
        .Finally(() =>
        {
            Debug.Log("loading finished");
            current = null;
        });
    }

    async Task DoLoad()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // 로딩 화면 추가
        Scene loadingScene = SceneManager.LoadScene("LoadingScene", new LoadSceneParameters()
        {
            loadSceneMode = LoadSceneMode.Additive,
            localPhysicsMode = LocalPhysicsMode.None,
        });
        await UniTask.Delay(3000);

        // GetRootGameObjects를 제대로 하기 위해서는 한 프레임 쉬어줘야 한다
        await UniTask.NextFrame();
        var loadingHandler = loadingScene.GetRootGameObjects().Where(go => go.GetComponent<LoadingScene>() != null).ToList();
        Debug.Assert(loadingHandler.Count == 1, $"loading scene should has a LoadingScene instance, got {loadingHandler.Count}");

        // 기존 씬 제거
        Debug.Log($"unloading {currentScene.name}");
        await SceneManager.UnloadSceneAsync(currentScene);
        Debug.Log($"unload finished");

        // 새로운 씬 추가
        Debug.Log($"loading {_scene}");
        var loadingOp = SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Additive);
        loadingOp.allowSceneActivation = false;
        if (loadingHandler.Count == 1)
        {
            loadingHandler[0].GetComponent<LoadingScene>().LoadingOp = loadingOp;
        }
        // 추가 로딩
        if (AdditionalLoadingTasks.Count != 0)
        {
            Debug.Log($"executing {AdditionalLoadingTasks.Count} additional tasks");
            await Task.WhenAll(AdditionalLoadingTasks.Select(creator => creator()));
        }

        Debug.Log("loading finished");
        // 로딩 화면 제거
        loadingOp.allowSceneActivation = true;
        await loadingOp;
        await SceneManager.UnloadSceneAsync(loadingScene);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_scene));
    }
}