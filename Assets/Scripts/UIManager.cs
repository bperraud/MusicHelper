using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public MusicManager m_MusicManager;

    public TMP_Dropdown m_ScaleDropdown;
    public TMP_Dropdown m_ScaleModeDropdown;
    public TMP_Text m_PlayButtonLabel;

    public Slider m_ChordsSlider;
    public Slider m_VolumeSlider;
    public Slider m_BPMSlider;
    public Slider m_BeatsSlider;
    public Slider m_TimeSpeedSlider;

    public Toggle m_ThirdsToggle;

    public Button m_ReplayButton;

    public GameObject m_RhythmWeightsPanel;

    private readonly Dictionary<int, string> m_DropdownScales = new Dictionary<int, string>();

    private bool m_IsPlaying;
    private bool m_IsReplaying;

    // Events handling
    public delegate void StopAction();

    public static event StopAction OnStop;

    private void Awake()
    {
        m_ReplayButton.interactable = false;

        var rhythmWeightSliders = m_RhythmWeightsPanel.GetComponentsInChildren<Slider>();
        var i = 0;
        rhythmWeightSliders.ToList().ForEach(slider => SetRhythmWeightByIndex(i++, slider));
    }

    private void Start()
    {
        // Set up scales
        var i = 0;
        m_ScaleDropdown.options.Clear();
        Notes.NoteNames.ToList().ForEach(n =>
        {
            m_ScaleDropdown.options.Add(new TMP_Dropdown.OptionData(n));
            m_DropdownScales.Add(i++, n);
        });
    }

    private void SetRhythmWeightByIndex(int index, Slider slider)
    {
        slider.onValueChanged.AddListener(newVal => SetRhythmWeight(index, newVal));
    }

    private void SetRhythmWeight(int index, float newVal)
    {
        m_MusicManager.m_Rhythms.m_Weights[index] = (int) newVal;
    }

    public void SetScale()
    {
        string scaleName;
        m_DropdownScales.TryGetValue(m_ScaleDropdown.value, out scaleName);
        m_MusicManager.m_Scale = scaleName;
        // Add "minor" suffix
        if (m_ScaleModeDropdown.value == 1)
            m_MusicManager.m_Scale += "m";
        if (m_IsPlaying) TryReplay();
    }

    [UsedImplicitly]
    public void SetBpm()
    {
        m_MusicManager.m_Bpm = m_BPMSlider.value;
        if (m_IsPlaying) TryReplay();
    }

    [UsedImplicitly]
    public void SetChordsProba()
    {
        m_MusicManager.m_ChordsProba = m_ChordsSlider.value;
    }

    [UsedImplicitly]
    public void SetVolume()
    {
        m_MusicManager.m_DefaultVolume = m_VolumeSlider.value;
    }

    [UsedImplicitly]
    public void SetBeatsPerBar()
    {
        m_MusicManager.m_BeatsPerBar = (int) m_BeatsSlider.value;
    }

    [UsedImplicitly]
    public void SetTimeSpeed()
    {
        m_MusicManager.m_TimeMultiplier = m_TimeSpeedSlider.value;
    }

    [UsedImplicitly]
    public void SetThirds(bool active)
    {
        m_MusicManager.m_EnableThirds = active;
    }

    public void TogglePlay(bool silentLabel = false)
    {
        m_IsPlaying = !m_IsPlaying;
        // Stopping
        if (!m_IsPlaying)
        {
            if (OnStop != null) OnStop();
            m_ReplayButton.interactable = false;
        }
        // Playing
        else
        {
            // Update label if needed
            if (!silentLabel) m_PlayButtonLabel.text = m_IsPlaying ? "Stop" : "Play";
            m_MusicManager.Play();
            m_ReplayButton.interactable = true;
        }
    }

    [UsedImplicitly]
    public void Replay()
    {
        if (!m_IsPlaying) return;
        TryReplay();
    }

    private void TryReplay()
    {
        if (m_IsReplaying) return;
        m_IsReplaying = true;
        StopCoroutine("ReplayCoroutine");
        StartCoroutine(ReplayCoroutine());
    }

    private IEnumerator ReplayCoroutine()
    {
        TogglePlay(true);
        // Security wait time before replay
        yield return new WaitForSeconds(0.5f);
        TogglePlay(true);
        m_IsReplaying = false;
    }
}