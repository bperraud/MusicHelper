using UnityEngine;
using Random = UnityEngine.Random;

public class NotesEmitter : MonoBehaviour
{
    [SerializeField] private float m_WidthEmission = 6f;
    [SerializeField] private float m_BaseSpeed = 1f;

    [SerializeField] private MusicManager m_MusicManager;
    [SerializeField] private GameObject[] m_NoteObjects;

    public void EmitNote(int noteIndex, int maxNotes)
    {
        float emissionPos = (maxNotes - noteIndex) * m_WidthEmission / maxNotes;
        GameObject randomNote = m_NoteObjects[Random.Range(0, m_NoteObjects.Length)];
        GameObject newNote = Instantiate(randomNote, Vector3.zero, Quaternion.identity, transform);
        newNote.GetComponent<MovingNote>().Init(
            Vector3.down * emissionPos,
            m_BaseSpeed * m_MusicManager.m_Bpm / 120
        );
    }
}