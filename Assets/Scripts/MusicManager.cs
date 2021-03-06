﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour
{
    [HideInInspector] public string m_Scale = "C";
    [HideInInspector] public float m_Bpm = 120.0f;
    [HideInInspector] public float m_ChordsProba = 0.1f;
    [HideInInspector] public int m_BeatsPerBar = 4;
    [HideInInspector] public float m_DefaultVolume = 0.1f;
    [HideInInspector] public float m_TimeMultiplier = 1;
    [HideInInspector] public bool m_EnableThirds;
    [HideInInspector] public int m_SecondsToKeep = 5;

    [HideInInspector] public Rhythms m_Rhythms;

    private const float FirstBeatVolumeOffset = 0.1f;

    public NotesEmitter m_NotesEmitter;

    public float m_ElapsedTime;
    public float m_PerlinNoise; // Range: from 0f to 1f

    private bool m_KeepPlaying;

    private readonly List<Oscillator> m_Oscillators = new List<Oscillator>();

    private int m_NotesNb;
    private int m_BeatCounter;
    private float m_WaitTimeForA16Th;
    private float m_NextWait;

    private int[] m_CurrentRhythm;
    private int m_CurrentRhythmPos;

    private IEnumerator m_PlayMelodyCoroutine;

    [HideInInspector] public MusicSequence m_CurrentMusicSequence;
    private MusicUnit m_CurrentMusicUnit;
    private int m_CurrentPlaybackMusicUnitIndex;
    [HideInInspector] public int m_MaxMusicUnitsToKeep;

    private bool m_IsPlayback;

    // Events handling
    public delegate void StopAction();

    public delegate void FirstBeatAction();

    public delegate void PulseAction();

    public delegate void PlaybackEndAction();

    public static event StopAction OnStop;
    public static event FirstBeatAction OnFirstBeat;
    public static event PulseAction OnPulse;

    public static event PlaybackEndAction OnPlaybackEnd;

    private void Awake()
    {
        m_Rhythms = GetComponent<Rhythms>();
        m_Bpm = 120.0f;
        m_NextWait = 1;

        m_CurrentMusicSequence = new MusicSequence
        {
            m_Bpm = m_Bpm,
            m_Scale = m_Scale,
            m_DefaultVolume = m_DefaultVolume,
            m_TimeMultiplier = m_TimeMultiplier,
            m_BeatsPerBar = m_BeatsPerBar,
            m_MusicUnits = new Queue<MusicUnit>(),
        };
    }

    private void Start()
    {
        // Create oscillators
        var template = new GameObject();
        for (var i = 0; i < Notes.notes.Count; i++)
        {
            GameObject newOscillator = Instantiate(template, transform.position, Quaternion.identity);
            newOscillator.name = Notes.m_AllNoteNames[i];
            newOscillator.transform.SetParent(transform);
            newOscillator.AddComponent<AudioSource>();
            newOscillator.AddComponent<Oscillator>();
            newOscillator.GetComponent<Oscillator>().m_Note = newOscillator.name;
        }

        Destroy(template);

        CalculateWaitingByBpm();
    }

    private void Update()
    {
        m_ElapsedTime = Time.time;
        float timeScaleFixer = m_WaitTimeForA16Th / m_NextWait;

        m_PerlinNoise = Mathf.PerlinNoise(m_ElapsedTime * m_TimeMultiplier * timeScaleFixer, 0.5f);
    }

    private void OnEnable()
    {
        UIManager.OnStop += Stop;
    }


    private void OnDisable()
    {
        UIManager.OnStop -= Stop;
    }

    private void CalculateWaitingByBpm()
    {
        m_WaitTimeForA16Th = 60 / (m_Bpm * 4);
    }

    private void InitRhythmCounters()
    {
        m_BeatCounter = 0;
        m_CurrentRhythmPos = 0;
    }

    public void Play(bool isPlayback = false)
    {
        KeepScaleOscillators();
        Stop();
        m_IsPlayback = isPlayback;
        m_PlayMelodyCoroutine = PlayMelody();

        if (!m_IsPlayback)
        {
            m_CurrentMusicSequence = new MusicSequence
            {
                m_Bpm = m_Bpm,
                m_Scale = m_Scale,
                m_DefaultVolume = m_DefaultVolume,
                m_TimeMultiplier = m_TimeMultiplier,
                m_BeatsPerBar = m_BeatsPerBar,
                m_MusicUnits = new Queue<MusicUnit>(),
            };

            float barsPerMin = m_Bpm / m_BeatsPerBar;
            float barsPerSec = barsPerMin / 60;
            int barsToKeep = Mathf.CeilToInt(barsPerSec * m_SecondsToKeep);
            m_MaxMusicUnitsToKeep = barsToKeep * m_BeatsPerBar;
        }
        else
        {
            m_Bpm = m_CurrentMusicSequence.m_Bpm;
            m_Scale = m_CurrentMusicSequence.m_Scale;
            m_DefaultVolume = m_CurrentMusicSequence.m_DefaultVolume;
            m_TimeMultiplier = m_CurrentMusicSequence.m_TimeMultiplier;
            m_BeatsPerBar = m_CurrentMusicSequence.m_BeatsPerBar;
        }

        StartCoroutine(m_PlayMelodyCoroutine);
    }

    private void Stop()
    {
        m_KeepPlaying = false;
        if (m_PlayMelodyCoroutine != null)
        {
            StopCoroutine(m_PlayMelodyCoroutine);
            m_PlayMelodyCoroutine = null;
        }

        if (OnStop != null) OnStop();
    }

    private void KeepScaleOscillators()
    {
        // Link oscillators
        m_Oscillators.Clear();
        var scaleMode = m_Scale.EndsWith("m") ? Notes.MinorScale : Notes.MajorScale;
        string scaleName = m_Scale.EndsWith("m") ? m_Scale.Remove(m_Scale.Length - 1) : m_Scale;

        int offset;
        Notes.NoteOffsets.TryGetValue(scaleName, out offset);

        var i = 0;
        var j = 0;
        var k = 0;

        foreach (Transform child in transform)
        {
            if (k < offset)
            {
                k++;
                continue;
            }

            if (scaleMode[j] == 0 || i == scaleMode[j] - 1)
            {
                m_Oscillators.Add(child.GetComponent<Oscillator>());
                i = 0;
                j++;
                if (j == scaleMode.Length - 1)
                {
                    j = 0;
                }
            }
            else
            {
                i++;
            }
        }

        m_NotesNb = m_Oscillators.Count;
    }

    private IEnumerator PlayMelody()
    {
        InitRhythmCounters();
        CalculateWaitingByBpm();
        yield return new WaitForSeconds(1);

        m_KeepPlaying = true;

        if (m_IsPlayback)
        {
            m_CurrentPlaybackMusicUnitIndex = 0;
            if (m_CurrentMusicSequence.m_MusicUnits.Count == 0)
            {
                m_KeepPlaying = false;
                if (OnPlaybackEnd != null) OnPlaybackEnd();
                yield break;
            }

            m_CurrentMusicUnit = m_CurrentMusicSequence.m_MusicUnits.ToList()[m_CurrentPlaybackMusicUnitIndex];
            m_CurrentRhythm = m_CurrentMusicUnit.m_Rhythm;
        }
        else
        {
            // Pick up first a random rhythm
            m_CurrentRhythm = m_Rhythms.m_Rhythms[m_Rhythms.GetRandomWeightedIndex()];

            // Set up saving data
            m_CurrentMusicUnit = new MusicUnit
            {
                m_Rhythm = m_CurrentRhythm,
                m_Notes = new List<int>[m_CurrentRhythm.Length]
            };
            m_CurrentMusicSequence.m_MusicUnits.Enqueue(m_CurrentMusicUnit);
            if (m_CurrentMusicSequence.m_MusicUnits.Count >= m_MaxMusicUnitsToKeep)
                m_CurrentMusicSequence.m_MusicUnits.Dequeue();
        }

        while (m_KeepPlaying)
        {
            // Calculate next rhythm wait time
            m_NextWait = m_CurrentRhythm[m_CurrentRhythmPos] * m_WaitTimeForA16Th;

            float duration = m_NextWait / 2;
            bool firstBeat = m_BeatCounter == 0;
            if (!m_IsPlayback)
            {
                // Calculate next note
                if (Mathf.Approximately(m_PerlinNoise, 0f)) m_PerlinNoise = 1f;

                int nextNote = ((int) Mathf.Ceil(m_PerlinNoise * m_NotesNb)) - 1;
                // Ensure we stay inside boundaries
                if (nextNote < 0) nextNote = 0;

                // Play note(s)
                PlayNotes(nextNote, duration, firstBeat);
            }
            else
            {
                if (m_CurrentMusicUnit.m_Notes[m_CurrentRhythmPos] == null)
                {
                    m_KeepPlaying = false;
                    if (OnPlaybackEnd != null) OnPlaybackEnd();
                    break;
                }

                m_CurrentMusicUnit.m_Notes[m_CurrentRhythmPos].ForEach(note => PlayNote(note, duration, firstBeat));
            }


            // -------------

            m_CurrentRhythmPos++;
            // End of rhythm, we need to pick up a new rhythm
            if (m_CurrentRhythmPos >= m_CurrentRhythm.Length)
            {
                if (m_IsPlayback)
                {
                    m_CurrentPlaybackMusicUnitIndex++;
                    if (m_CurrentMusicSequence.m_MusicUnits.Count == m_CurrentPlaybackMusicUnitIndex)
                    {
                        m_KeepPlaying = false;
                        if (OnPlaybackEnd != null) OnPlaybackEnd();
                        break;
                    }

                    m_CurrentMusicUnit = m_CurrentMusicSequence.m_MusicUnits.ToList()[m_CurrentPlaybackMusicUnitIndex];
                    m_CurrentRhythm = m_CurrentMusicUnit.m_Rhythm;
                }
                else
                {
                    m_CurrentRhythm = m_Rhythms.m_Rhythms[m_Rhythms.GetRandomWeightedIndex()];
                }

                m_CurrentRhythmPos = 0;

                if (!m_IsPlayback)
                {
                    m_CurrentMusicUnit = new MusicUnit
                    {
                        m_Rhythm = m_CurrentRhythm,
                        m_Notes = new List<int>[m_CurrentRhythm.Length]
                    };
                    m_CurrentMusicSequence.m_MusicUnits.Enqueue(m_CurrentMusicUnit);
                    if (m_CurrentMusicSequence.m_MusicUnits.Count >= m_MaxMusicUnitsToKeep)
                        m_CurrentMusicSequence.m_MusicUnits.Dequeue();
                }

                // Increment beat as well
                m_BeatCounter++;
                m_BeatCounter = Mathf.Min(m_BeatCounter, m_BeatCounter % m_BeatsPerBar);
            }

            yield return new WaitForSeconds(m_NextWait);
        }
    }

    private void PlayNotes(int mainNoteIndex, float duration, bool firstBeat)
    {
        if (!m_IsPlayback)
        {
            m_CurrentMusicUnit.m_Notes[m_CurrentRhythmPos] = new List<int>();
        }

        // Main, always
        PlayNote(mainNoteIndex, duration, firstBeat);

        // Triad, only one first beat notes
        if (m_CurrentRhythmPos == 0 && Random.Range(0f, 1f) <= m_ChordsProba)
        {
            int third = mainNoteIndex + 2;
            int fifth = mainNoteIndex + 4;
            // Can go upwards
            if (fifth < m_NotesNb)
            {
                PlayNote(third, duration, firstBeat);
                PlayNote(fifth, duration, firstBeat);
            }
            // Otherwise, play a reversed triad
            else
            {
                int rFifth = mainNoteIndex - 3;
                int rThird = mainNoteIndex - 5;
                PlayNote(rFifth, duration, firstBeat);
                PlayNote(rThird, duration, firstBeat);
            }
        }
        else if (m_EnableThirds)
        {
            int third = mainNoteIndex + 2;
            if (third < m_NotesNb)
                PlayNote(third, duration, firstBeat);
        }
    }

    private void PlayNote(int noteIndex, float duration, bool firstBeat)
    {
        float vol = m_DefaultVolume;
        if (firstBeat) vol += FirstBeatVolumeOffset;
        m_Oscillators[noteIndex].PlayNote(duration, vol);
        m_NotesEmitter.EmitNote(noteIndex, m_Oscillators.Count);
        if (firstBeat && m_CurrentRhythmPos == 0 && OnFirstBeat != null) OnFirstBeat();
        else if (m_CurrentRhythmPos == 0 && OnPulse != null) OnPulse();

        if (!m_IsPlayback)
        {
            m_CurrentMusicUnit.m_Notes[m_CurrentRhythmPos].Add(noteIndex);
        }
    }
}