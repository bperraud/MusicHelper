using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToasterManager : MonoBehaviour
{
    [SerializeField] private TMP_Text m_Text;
    [SerializeField] private Animator m_Animator;
    private static readonly int FadeIn = Animator.StringToHash("FadeIn");
    private static readonly int FadeOut = Animator.StringToHash("FadeOut");

    private bool m_ReadyForToast = true;

    private struct Toast
    {
        public string m_Text;
        public float m_Duration;
    }

    private readonly Queue<Toast> m_WaitingToasts = new Queue<Toast>();

    private void Update()
    {
        if (m_WaitingToasts.Count <= 0 || !m_ReadyForToast) return;
        Toast t = m_WaitingToasts.Dequeue();
        m_ReadyForToast = false;
        m_Text.text = t.m_Text;
        m_Animator.SetTrigger(FadeIn);
        StartCoroutine(WaitThenFadeOut(t.m_Duration));
    }

    public void ShowToast(string text, float duration = 1.5f)
    {
        m_WaitingToasts.Enqueue(new Toast { m_Text = text, m_Duration = duration });
    }

    private IEnumerator WaitThenFadeOut(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        m_Animator.SetTrigger(FadeOut);
        yield return new WaitForSecondsRealtime(1);
        m_ReadyForToast = true;
    }
}