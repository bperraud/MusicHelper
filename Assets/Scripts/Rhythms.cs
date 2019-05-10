using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rhythms : MonoBehaviour
{
    private static readonly int[] Quarter = { 4 };
    private static readonly int[] EightEight = { 2, 2 };
    private static readonly int[] SixthSixthSixthSixth = { 1, 1, 1, 1 };

    private static readonly int[] EightSixthSixth = { 2, 1, 1 };
    private static readonly int[] SixthSixthEight = { 1, 1, 2 };

    // Syncope rhythms
    private static readonly int[] SixthEightSixth = { 1, 2, 1 };

    private static readonly int[] EightDottedSixth = { 3, 1 };
    private static readonly int[] SixthEightDotted = { 1, 3 };

    public List<int[]> m_Rhythms = new List<int[]>();

    [SerializeField]
    public int[] m_Weights;

    private void Start()
    {
        m_Rhythms.Add(Quarter);
        m_Rhythms.Add(EightEight);
        m_Rhythms.Add(SixthSixthSixthSixth);

        m_Rhythms.Add(EightSixthSixth);
        m_Rhythms.Add(SixthSixthEight);

        m_Rhythms.Add(SixthEightSixth);

        m_Rhythms.Add(EightDottedSixth);
        m_Rhythms.Add(SixthEightDotted);

        m_Weights = new[] { 1, 1, 1, 1, 1, 0, 0, 0 };
    }

    public int GetRandomWeightedIndex()
    {
        // Get the total sum of all the weights.
        int weightSum = m_Weights.Sum();

        // Step through all the possibilities, one by one, checking to see if each one is selected.
        var index = 0;
        int lastIndex = m_Weights.Length - 1;
        while (index < lastIndex)
        {
            // Do a probability check with a likelihood of weights[index] / weightSum.
            if (Random.Range(0, weightSum) < m_Weights[index])
            {
                return index;
            }

            // Remove the last item from the sum of total untested weights and try again.
            weightSum -= m_Weights[index++];
        }

        // No other item was selected, so return very last index.
        return index;
    }
}