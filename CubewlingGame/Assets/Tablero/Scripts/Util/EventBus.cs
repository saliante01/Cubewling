using System;
using UnityEngine;

public static class EventBus
{
    public static Action<CajaTablero> OnExplode;

    public static void Explode(CajaTablero caja)
    {
        OnExplode?.Invoke(caja);
    }
}
