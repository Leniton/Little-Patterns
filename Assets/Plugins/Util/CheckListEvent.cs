using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckListEvent
{
    public float progress => total > 0 ? current / (float)total : 0;
    public int current = 0;
    public int total = 1;
    private bool clearOnComplete;

    public Action onComplete;

    public CheckListEvent(int size = 1,bool clearOnComplete = true)
    {
        total = size;
        this.clearOnComplete = clearOnComplete;
    }

    public void MarkProgress()
    {
        current++;
        if (current >= total)
        {
            if(clearOnComplete) ClearProgress();
            onComplete?.Invoke();
        }
    }

    public void ClearProgress() => current = 0;
}
