using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private MusicManager m_MusicManager;
    [SerializeField] private ToasterManager m_ToasterManager;

    [SerializeField] private TMP_Dropdown m_ScaleDropdown;
    [SerializeField] private TMP_Dropdown m_ScaleModeDropdown;
    [SerializeField] private TMP_Text m_PlayButtonLabel;

    [SerializeField] private Image m_ThirdsBackground;

    [SerializeField] private Button m_ReplayButton;

    [SerializeField] private Button m_LoadButton;
    [SerializeField] private Button m_SaveButton;
    [SerializeField] private Button m_QuitPlaybackButton;

    [SerializeField] private GameObject m_RhythmWeightsPanel;

    [SerializeField] private CanvasGroup m_SettingsCanvasGroup;
    [SerializeField] private CanvasGroup m_LoadSaveCanvasGroup;
    [SerializeField] private TMP_Text m_PlaybackLabel;

    [SerializeField] private TMP_Text m_LastSaveLabel;

    private readonly Dictionary<int, string> m_DropdownScales = new Dictionary<int, string>();

    private GameObject m_QuitPlaybackButtonGameObject;

    private bool m_IsPlaying;
    private bool m_IsReplaying;
    private bool m_IsPlayback;

    private MusicSequence m_MusicSequencePlayback;

    // Events handling
    public delegate void StopAction();

    public static event StopAction OnStop;

    private void Awake()
    {
        Screen.fullScreen = false;

        m_ReplayButton.interactable = false;
        m_QuitPlaybackButton.interactable = false;
        m_PlaybackLabel.enabled = false;

        m_QuitPlaybackButtonGameObject = m_QuitPlaybackButton.gameObject;

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

        string destination = Application.persistentDataPath + "/save.dat";
        if (!File.Exists(destination))
        {
            m_LoadButton.interactable = false;
            m_LastSaveLabel.text = "Last save: none";
        }
        else
        {
            m_LastSaveLabel.text = "Last save: " + string.Format("{0:g}", File.GetLastWriteTime(destination));
        }
    }

    private void OnEnable()
    {
        MusicManager.OnPlaybackEnd += OnPlaybackEnd;
    }


    private void OnDisable()
    {
        MusicManager.OnPlaybackEnd -= OnPlaybackEnd;
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

    public void SetBpm(float newVal)
    {
        m_MusicManager.m_Bpm = newVal;
        if (m_IsPlaying) TryReplay();
    }

    public void SetChordsProba(float newVal)
    {
        m_MusicManager.m_ChordsProba = newVal;
    }

    public void SetVolume(float newVal)
    {
        m_MusicManager.m_DefaultVolume = newVal;
    }

    public void SetBeatsPerBar(float newVal)
    {
        m_MusicManager.m_BeatsPerBar = (int) newVal;
    }

    public void SetTimeSpeed(float newVal)
    {
        m_MusicManager.m_TimeMultiplier = newVal;
    }

    public void SetThirds(bool active)
    {
        m_MusicManager.m_EnableThirds = active;
        m_ThirdsBackground.color = active ? Color.cyan : Color.white;
    }

    public void SetSecondsToSave(float newVal)
    {
        m_MusicManager.m_SecondsToKeep = (int) newVal;
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
            if (m_IsPlayback) m_MusicManager.m_CurrentMusicSequence = m_MusicSequencePlayback;
            m_MusicManager.Play(m_IsPlayback);
            m_ReplayButton.interactable = true;
        }

        // Update label if needed
        if (!silentLabel) m_PlayButtonLabel.text = m_IsPlaying ? "Stop" : "Play";
    }

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

    public void Save()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file = File.Exists(destination) ? File.OpenWrite(destination) : File.Create(destination);
        var bf = new BinaryFormatter();
        bf.Serialize(file, m_MusicManager.m_CurrentMusicSequence);
        file.Close();
        m_ToasterManager.ShowToast("Saved!");
        m_LastSaveLabel.text = "Last save: " + string.Format("{0:g}", File.GetLastWriteTime(destination));
        m_LoadButton.interactable = true;
    }

    public void Load()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            m_ToasterManager.ShowToast("Save not found!");
            return;
        }

        var bf = new BinaryFormatter();
        m_MusicSequencePlayback = (MusicSequence) bf.Deserialize(file);
        file.Close();

        // Begin Playback
        m_IsPlayback = true;
        m_LoadButton.interactable = false;
        m_SaveButton.interactable = false;
        m_QuitPlaybackButtonGameObject.SetActive(true);
        m_QuitPlaybackButton.interactable = true;
        m_SettingsCanvasGroup.alpha = 0.2f;
        m_SettingsCanvasGroup.interactable = false;
        m_PlaybackLabel.enabled = true;
        m_LoadSaveCanvasGroup.alpha = 0;
        m_LoadSaveCanvasGroup.interactable = false;
        if (!m_IsPlaying)
        {
            TogglePlay();
        }
        else
        {
            Replay();
        }
    }

    public void QuitPlayback()
    {
        if (m_IsPlaying) TogglePlay();
        m_IsPlayback = false;
        m_LoadButton.interactable = true;
        m_SaveButton.interactable = true;
        m_QuitPlaybackButtonGameObject.SetActive(false);
        m_QuitPlaybackButton.interactable = false;
        m_SettingsCanvasGroup.alpha = 1;
        m_SettingsCanvasGroup.interactable = true;
        m_PlaybackLabel.enabled = false;
        m_LoadSaveCanvasGroup.alpha = 1;
        m_LoadSaveCanvasGroup.interactable = true;
    }

    private void OnPlaybackEnd()
    {
        TogglePlay();
    }
}