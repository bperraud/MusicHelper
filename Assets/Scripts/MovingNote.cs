using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingNote : MonoBehaviour
{
    private float m_Speed = 1f;
    private float m_RotateSpeed = 0.1f;
    private Animator m_Animator;
    private SpriteRenderer m_SpriteRenderer;
    private static readonly int Pulse = Animator.StringToHash("Pulse");
    private static readonly int TinyPulse = Animator.StringToHash("TinyPulse");

    private bool m_IsFading;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Transform tr = transform;
        tr.position += Vector3.right * m_Speed * Time.deltaTime;
        tr.rotation *= Quaternion.AngleAxis(360 * m_RotateSpeed * Time.deltaTime, Vector3.forward);
    }

    private void OnEnable()
    {
        MusicManager.OnFirstBeat += TryPulse;
        MusicManager.OnPulse += TryTinyPulse;
    }

    private void OnDisable()
    {
        MusicManager.OnFirstBeat -= TryPulse;
        MusicManager.OnPulse -= TryTinyPulse;
    }

    public void Init(Vector3 localPos, float speed)
    {
        Transform tr = transform;
        tr.localPosition = localPos;
        tr.rotation *= Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
        m_Speed = speed;
        m_RotateSpeed += Random.Range(-0.05f, 0.2f);
        m_SpriteRenderer.color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
    }

    private void TryPulse()
    {
        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Empty") && !m_Animator.IsInTransition(0))
            m_Animator.SetTrigger(Pulse);
    }

    private void TryTinyPulse()
    {
        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Empty") && !m_Animator.IsInTransition(0))
            m_Animator.SetTrigger(TinyPulse);
    }

    public void FadeOut(float fadeSpeed)
    {
        if (m_IsFading) return;
        StartCoroutine(FadeThenDestroy(fadeSpeed));
    }

    private IEnumerator FadeThenDestroy(float fadeSpeed)
    {
        m_IsFading = true;
        var t = 0f;
        Color fromColor = m_SpriteRenderer.color;
        while (t <= 1f)
        {
            m_SpriteRenderer.color = Color.Lerp(fromColor, new Color(fromColor.r, fromColor.g, fromColor.b, 0), t);
            t += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        Destroy(gameObject);
    }
}