using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.WSA;

public class task : MonoBehaviour
{
    public int CallCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        //Java Script TAP Model
        Task myTask = Task.Run(() => TestMethod(1000))
                          .Then(() => TestMethod(2000)).Catch(ExceptionCatched)// Exception발생 Task
                          .Then(() => TestMethod(3000)); //만약 여기가 Then이었다면 skip
        // Finally는 이전 Task에서 Exception이 발생해도 해당 task를 force-processing한다.
        // Then은 이전 Task에서 Exception이 발생하면 해당 task를 skip한다.
    }
    public async Task TestMethod(int delay)
    {

        await UniTask.Delay(delay);
        Debug.Log($"Called {++CallCount}: wait {delay} seconds and call Function ");
        await UniTask.NextFrame();
        if (CallCount == 2)
            c();
    }
    public void c()
    { c(); }

    public void ExceptionCatched(Exception exception)
    {
        Debug.LogException(exception);
    }
}
