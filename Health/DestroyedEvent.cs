using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyedEvent : MonoBehaviour
{
    public event Action<DestroyedEvent> OnDestroyed;

    public void CallDestroyed()
    {
        OnDestroyed?.Invoke(this);
    }
}
