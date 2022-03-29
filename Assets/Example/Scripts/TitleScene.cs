using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor.UI;

public class TitleScene : UIScene
{
    public Button startButton;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        startButton.onClick.AddListener(OnStartGame);
    }

    public void OnStartGame()
    {
        SceneLoader.LoadScene("IngameScene");
        SceneLoader.current.AdditionalLoadingTasks.Add(OnLoadGame);
    }

    public async Task OnLoadGame()
    {
        await ResourceManager.instance.PrepareLoading();

        TaskOperation operation = new TaskOperation();

        //TODO 여기에 로딩 큐를 추가하세요
        //EX) operation.AddRange(UnitList.units.Select(data => ResourceManager.instance.LoadScriptableObject(data.Item2)));
        //EX) operation.Add(ResourceManager.instance.LoadScriptableObject("Dsadsa"));   


        //////////////////////////////
        operation.Run();
        while (true)
        {
            if (operation.isRuntimeOperation == false) break;

            await UniTask.NextFrame();
        }
    }
}
