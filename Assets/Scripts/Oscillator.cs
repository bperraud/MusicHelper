using System.Collections;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    public string m_Note = "A4";
    public float m_Gain;
    public float m_Volume = 0.1f;

    private const double SamplingFrequency = 48000.0;
    private float m_Frequency;
    private double m_Increment;
    private double m_Phase;

    private IEnumerator m_PlayNoteCoroutine;

    private void Start()
    {
        Notes.notes.TryGetValue(m_Note, out m_Frequency);
    }

    private void OnEnable()
    {
        MusicManager.OnStop += Stop;
    }


    private void OnDisable()
    {
        MusicManager.OnStop -= Stop;
    }

    private void Stop()
    {
        m_Gain = 0;
        if (m_PlayNoteCoroutine != null) StopCoroutine(m_PlayNoteCoroutine);
        m_PlayNoteCoroutine = null;
    }

    public void PlayNote(float time, float volume)
    {
        // Only one note at a time
        if (m_PlayNoteCoroutine != null) return;
        m_PlayNoteCoroutine = PlayNoteCoroutine(time, volume);
        StartCoroutine(m_PlayNoteCoroutine);
    }

    private IEnumerator PlayNoteCoroutine(float time, float volume)
    {
        m_Gain = volume;
        yield return new WaitForSeconds(time);
        m_Gain = 0;
        m_PlayNoteCoroutine = null;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        m_Increment = m_Frequency * 2.0 * Mathf.PI / SamplingFrequency;

        for (var i = 0; i < data.Length; i += channels)
        {
            m_Phase += m_Increment;

            if (m_Gain * Mathf.Sin((float) m_Phase) >= 0)
            {
                data[i] = m_Gain * 0.6f;
            }
            else
            {
                data[i] = -m_Gain * 0.6f;
            }

            // data[i] = gain * Mathf.Sin((float) phase);

            if (channels == 2)
            {
                data[i + 1] = data[i];
            }

            if (m_Phase > Mathf.PI * 2)
            {
                m_Phase = 0.0;
            }
        }
    }
}