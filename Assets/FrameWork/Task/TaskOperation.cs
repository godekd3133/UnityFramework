using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

public class TaskOperation
{
    private HashSet<Task> tasks;

    public Action onFinishTask;
    public float op { get { return (float)taskQueueCount / (float)tasks.Count(); } }
    public int taskQueueCount = 0;
    public bool isRuntimeOperation;

    public TaskOperation()
    {
        tasks = new HashSet<Task>();
    }
    public bool Add(Task task)
    {
        if (isRuntimeOperation) throw new Exception("TaskOperation is already running.");
        return tasks.Add(task);
    }
    public bool Add(Action task)
    {
        if (isRuntimeOperation) throw new Exception("TaskOperation is already running.");
        return tasks.Add(new Task(task));
    }
    public void AddRange(IEnumerable<Task> task)
    {
        if (isRuntimeOperation) throw new Exception("TaskOperation is already running.");
        foreach (var each in task) tasks.Add(each);
    }
    public async Task Run()
    {
        isRuntimeOperation = true;
        await Task.WhenAll(tasks.Select(task => task.Then(async () =>
        {
            await UniTask.Delay(UnityEngine.Random.Range(10, 50) * 100);
            taskQueueCount++;
            onFinishTask?.Invoke();
        })));
        isRuntimeOperation = false;
    }

    public void Clear()
    {
        tasks.Clear();
        onFinishTask = null;
        isRuntimeOperation = false;
        taskQueueCount = 0;
    }
}
