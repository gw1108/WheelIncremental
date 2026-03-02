using System;
using UnityEngine;

namespace Wheels
{
    /// <summary>
    /// The representation of a wheel segment.
    /// Visuals are seperated into a different class.
    /// TODO: Consider making this a ScriptableObject
    /// </summary>
    [Serializable]
    public class WheelSegmentData
    {
        public string prizeName = "Unknown Prize";
        public int cashPrize = 1;
        public Color segmentColor = Color.white;
        /// <summary>
        /// Thickness of this segment. Higher number is higher chance of picking this.
        /// </summary>
        public float weight = 1f;

        /*
        public WheelSegmentData(string prizeName, Color segmentColor, float weight = 1f)
        {
            this.prizeName = prizeName;
            this.segmentColor = segmentColor;
            this.weight = weight;
        }
        */
    }
}
