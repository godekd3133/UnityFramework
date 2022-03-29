using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class IntroScene : UIScene
{
    public Text logLoading;
    public Slider LoadingGauge;
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
    }

    public override void OnAdd()
    {
        base.OnAdd();
        StartLoadProcess();
    }

    public Task StartLoadProcess() => UpdateGameDataFlow().Then(DownLoadResourceFlow)
                                                          .Then(UseLoginFlow)
                                                          .Then(CompletedLoadFlow)
                                                          .Finally(() => RootCanvas.instance.SetScene("Scene/TitleScene.prefab"))
                                                          .Catch(e => Debug.LogException(e));

    private async Task UpdateGameDataFlow()
    {

        TaskOperation operation = new TaskOperation();

        //TODO 여기에 로딩 큐를 추가하세요
        //EX) operation.AddRange(UnitList.units.Select(data => ResourceManager.instance.LoadScriptableObject(data.Item2)));
        //EX) operation.Add(ResourceManager.instance.LoadScriptableObject("Dsadsa"));

        //   operation.AddRange(UnitList.units.Select(data => ResourceManager.instance.LoadScriptableObject(data.Item2).Then((res) => DataList.SetUnits(data.Item1, res))));

        //////////////////////////////
        operation.Run();
        while (true)
        {
            logLoading.text = $"리소스 갱신 중 : {operation.op * 100f}%";

            if (operation.isRuntimeOperation == false) break;

            await UniTask.NextFrame();
        }
        await UniTask.Delay(2000);
    }
    private async Task DownLoadResourceFlow()
    {
        logLoading.text = "리소스 다운로드 중";
        await UniTask.Delay(2000);
    }
    private async Task UseLoginFlow()
    {
        logLoading.text = "리소스 캐싱 중";
        await ResourceManager.instance.PrepareLoading();
        await UniTask.Delay(2000);
    }

    private async Task CompletedLoadFlow()
    {
        logLoading.text = "로그인중";
        await UniTask.Delay(2000);
    }
}
