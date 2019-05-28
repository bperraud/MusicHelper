using System;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public struct MusicUnit
    {
        public int[] m_Rhythm;
        public List<int>[] m_Notes;
    }
}