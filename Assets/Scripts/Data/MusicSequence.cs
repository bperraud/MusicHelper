using System;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public struct MusicSequence
    {
        public string m_Scale;
        public float m_Bpm;
        public int m_BeatsPerBar;
        public float m_DefaultVolume;
        public float m_TimeMultiplier;

        public Queue<MusicUnit> m_MusicUnits;
    }
}