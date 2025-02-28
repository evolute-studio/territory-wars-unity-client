using System.Collections;
using UnityEngine;


public sealed class Coroutines: MonoBehaviour
{
    private static Coroutines _instance
    {
        get
        {
            if (m_instance == null)
            {
                var go = new GameObject("[COROUTINE MANAGER]");
                m_instance = go.AddComponent<Coroutines>();
                DontDestroyOnLoad(go);
            }
            return m_instance;
        }
    }
    
    private static Coroutines m_instance;
    
    public static Coroutine StartRoutine(IEnumerator routine)
    {
        return _instance.StartCoroutine(routine);
    }
    
    public static void StopRoutine(Coroutine routine)
    {
        if (routine == null)
        {
            return;
        }
        _instance.StopCoroutine(routine);
    }
}
