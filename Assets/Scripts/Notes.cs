using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Notes : MonoBehaviour
{
    public static readonly float C3 = 130.81f;
    public static readonly float Db3 = 138.59f;
    public static readonly float D3 = 146.83f;
    public static readonly float Eb3 = 155.56f;
    public static readonly float E3 = 164.81f;
    public static readonly float F3 = 174.61f;
    public static readonly float Gb3 = 185.00f;
    public static readonly float G3 = 196.00f;
    public static readonly float Ab3 = 207.65f;
    public static readonly float A3 = 220.00f;
    public static readonly float Bb3 = 233.08f;
    public static readonly float B3 = 246.94f;
    public static readonly float C4 = 261.63f;
    public static readonly float Db4 = 277.18f;
    public static readonly float D4 = 293.66f;
    public static readonly float Eb4 = 311.13f;
    public static readonly float E4 = 329.63f;
    public static readonly float F4 = 349.23f;
    public static readonly float Gb4 = 369.99f;
    public static readonly float G4 = 392.00f;
    public static readonly float Ab4 = 415.30f;
    public static readonly float A4 = 440.00f;
    public static readonly float Bb4 = 466.16f;
    public static readonly float B4 = 493.88f;
    public static readonly float C5 = 523.25f;
    public static readonly float Db5 = 554.37f;
    public static readonly float D5 = 587.33f;
    public static readonly float Eb5 = 622.25f;
    public static readonly float E5 = 659.25f;
    public static readonly float F5 = 698.46f;
    public static readonly float Gb5 = 739.99f;
    public static readonly float G5 = 783.99f;
    public static readonly float Ab5 = 830.61f;
    public static readonly float A5 = 880.00f;
    public static readonly float Bb5 = 932.33f;
    public static readonly float B5 = 987.77f;
    public static readonly float C6 = 1046.50f;

    public static readonly string[] NoteNames = { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" };

    public static readonly Dictionary<string, float> notes = new Dictionary<string, float>();
    public static List<string> m_AllNoteNames;
    public static List<float> m_NoteFreqs;

    public static readonly int[] MajorScale = { 0, 2, 2, 1, 2, 2, 2, 1 };
    public static readonly int[] MinorScale = { 0, 2, 1, 2, 2, 1, 3, 1 };

    public static readonly Dictionary<string, int> NoteOffsets = new Dictionary<string, int>();

    private void Awake()
    {
        var i = 0;
        NoteNames.ToList().ForEach(note => NoteOffsets.Add(note, i++));

        notes.Add("C3", 130.81f);
        notes.Add("Db3", 138.59f);
        notes.Add("D3", 146.83f);
        notes.Add("Eb3", 155.56f);
        notes.Add("E3", 164.81f);
        notes.Add("F3", 174.61f);
        notes.Add("Gb3", 185.00f);
        notes.Add("G3", 196.00f);
        notes.Add("Ab3", 207.65f);
        notes.Add("A3", 220.00f);
        notes.Add("Bb3", 233.08f);
        notes.Add("B3", 246.94f);
        notes.Add("C4", 261.63f);
        notes.Add("Db4", 277.18f);
        notes.Add("D4", 293.66f);
        notes.Add("Eb4", 311.13f);
        notes.Add("E4", 329.63f);
        notes.Add("F4", 349.23f);
        notes.Add("Gb4", 369.99f);
        notes.Add("G4", 392.00f);
        notes.Add("Ab4", 415.30f);
        notes.Add("A4", 440.00f);
        notes.Add("Bb4", 466.16f);
        notes.Add("B4", 493.88f);
        notes.Add("C5", 523.25f);
        notes.Add("Db5", 554.37f);
        notes.Add("D5", 587.33f);
        notes.Add("Eb5", 622.25f);
        notes.Add("E5", 659.25f);
        notes.Add("F5", 698.46f);
        notes.Add("Gb5", 739.99f);
        notes.Add("G5", 783.99f);
        notes.Add("Ab5", 830.61f);
        notes.Add("A5", 880.00f);
        notes.Add("Bb5", 932.33f);
        notes.Add("B5", 987.77f);
        notes.Add("C6", 1046.50f);

        m_AllNoteNames = new List<string>(notes.Keys);
        m_NoteFreqs = new List<float>(notes.Values);
    }
}