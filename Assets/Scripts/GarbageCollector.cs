using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCollector : MonoBehaviour
{
    [SerializeField] private float m_FadeSpeed = 1.5f;

    private void OnTriggerEnter(Collider other)
    {
        var movingNote = other.GetComponent<MovingNote>();
        if (movingNote == null) return;
        movingNote.FadeOut(m_FadeSpeed);
    }
}