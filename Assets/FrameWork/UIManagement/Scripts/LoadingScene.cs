using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor.UI;
using UnityEngine.PlayerLoop;

public class LoadingScene : UIScene
{
    public AsyncOperation LoadingOp;
    public Slider loadingGauge;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        loadingGauge.value = LoadingOp.progress;
    }

}
