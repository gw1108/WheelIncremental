using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Wheels
{
    public class Wheel : MonoBehaviour
    {
        [Header("Wheel Settings")]
        [SerializeField] private float wheelRadius = 2.5f;
        [SerializeField] private Material segmentMaterial;
        [SerializeField] private TMP_FontAsset font;

        [Header("Segments")]
        [Tooltip("Fill this in with the initial segments this wheel will have on Start.")]
        [SerializeField] private List<WheelSegmentData> initialSegments = new List<WheelSegmentData>();

        // TODO: Better event system, better payload, etc
        public Action<Wheel, int> OnWheelSpinCompleted;

        private readonly List<WheelSegmentData> _segments = new List<WheelSegmentData>();
        private readonly List<WheelSegmentVisual> _segmentVisuals = new List<WheelSegmentVisual>();
        private float _currentRotation = 0f;
        private Coroutine _spinCoroutine;

        private float FinalRotationSpeed = 10f;

        private void Start()
        {
            _segments.AddRange(initialSegments);
            RebuildWheel();
        }

        private void RebuildWheel()
        {
            ClearExistingSegments();

            // Make sure there is SOMETHING to build
            float totalWeight = CalculateTotalWeight();
            if (totalWeight <= 0)
            {
                return;
            }

            float currentAngle = 0f;
            foreach (WheelSegmentData segmentData in _segments)
            {
                float sweepAngle = 360f * (segmentData.weight / totalWeight);
                CreateSegmentVisual(segmentData, currentAngle, sweepAngle);
                currentAngle += sweepAngle;
            }
        }

        /// <summary>
        /// Deletes all segment game objects so that the wheel is empty after this is done.
        /// </summary>
        private void ClearExistingSegments()
        {
            foreach (WheelSegmentVisual visual in _segmentVisuals)
            {
                if (visual != null)
                {
                    Destroy(visual.gameObject);
                }
            }
            _segmentVisuals.Clear();
        }

        /// <summary>
        /// Calculates the total weight of the wheel. Each segment has a relative weight on the wheel and we determine
        /// their size based on those weights.
        /// </summary>
        /// <returns></returns>
        private float CalculateTotalWeight()
        {
            float total = 0f;
            foreach (WheelSegmentData segment in _segments)
            {
                total += segment.weight;
            }
            return total;
        }

        /// <summary>
        /// Creates a new game object and adds it to our segment tracking array.
        /// </summary>
        private void CreateSegmentVisual(WheelSegmentData data, float startAngle, float sweepAngle)
        {
            GameObject segmentObj = new GameObject($"Segment_{data.prizeName}");
            segmentObj.transform.SetParent(transform);
            segmentObj.transform.localPosition = Vector3.zero;
            segmentObj.transform.localRotation = Quaternion.identity;
            segmentObj.transform.localScale = Vector3.one;

            WheelSegmentVisual visual = segmentObj.AddComponent<WheelSegmentVisual>();
            visual.Initialize(data, startAngle, sweepAngle, wheelRadius, segmentMaterial, font);

            _segmentVisuals.Add(visual);
        }

        // TODO: Handle dyanmic segments better
        public void AddSegment(WheelSegmentData newSegment)
        {
            _segments.Add(newSegment);
            RebuildWheel();
        }

        public void RemoveSegment(int index)
        {
            if (index >= 0 && index < _segments.Count)
            {
                _segments.RemoveAt(index);
                RebuildWheel();
            }
        }

        public void UpdateSegmentWeight(int index, float newWeight)
        {
            if (index >= 0 && index < _segments.Count)
            {
                _segments[index].weight = Mathf.Max(0.1f, newWeight);
                RebuildWheel();
            }
        }

        public void SpinWheel(float spinSpeed = 720f)
        {
            // Don't spin if you're already spinning
            if (_spinCoroutine != null)
            {
                return;
            }

            _spinCoroutine = StartCoroutine(SpinCoroutine(spinSpeed));
        }

        private System.Collections.IEnumerator SpinCoroutine(float initialSpeed)
        {
            float currentSpeed = initialSpeed;
            float deceleration = initialSpeed / 3f;

            while (currentSpeed > 10f)
            {
                _currentRotation += currentSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Euler(0, 0, _currentRotation);

                currentSpeed -= deceleration * Time.deltaTime;
                yield return null;
            }

            // Lock in the winning segment, then compute the rotation that centres the pointer on it.
            int selectedIndex = DetermineSelectedSegment();
            float segmentCenterAngle = CalculateSegmentCenterAngle(selectedIndex);

            // Invert the pointer formula: pointerAngle = (450 - normalizedRotation) % 360
            // => normalizedRotation = (450 - segmentCenterAngle + 360) % 360
            float desiredNormalized = (450f - segmentCenterAngle + 360f) % 360f;
            float currentNormalized = (_currentRotation % 360f + 360f) % 360f;

            float delta = desiredNormalized - currentNormalized;
            if (delta > 180f) delta -= 360f;
            if (delta < -180f) delta += 360f;

            float targetAngle = _currentRotation + delta;

            while (Mathf.Abs(_currentRotation - targetAngle) > 0.05f)
            {
                _currentRotation = Mathf.Lerp(_currentRotation, targetAngle, Time.deltaTime * FinalRotationSpeed);
                transform.rotation = Quaternion.Euler(0, 0, _currentRotation);
                yield return null;
            }

            _currentRotation = targetAngle;
            transform.rotation = Quaternion.Euler(0, 0, _currentRotation);

            Debug.Log($"Landed on: {_segments[selectedIndex].prizeName}");

            _spinCoroutine = null;

            OnWheelSpinCompleted?.Invoke(this, _segments[selectedIndex].cashPrize);
        }

        /// <summary>
        /// Returns the midpoint angle (in segment-space degrees) of the segment at the given index.
        /// </summary>
        private float CalculateSegmentCenterAngle(int segmentIndex)
        {
            float totalWeight = CalculateTotalWeight();
            float currentAngle = 0f;

            for (int i = 0; i < _segments.Count; i++)
            {
                float sweepAngle = 360f * (_segments[i].weight / totalWeight);
                if (i == segmentIndex)
                {
                    return currentAngle + sweepAngle / 2f;
                }
                currentAngle += sweepAngle;
            }

            return 0f;
        }

        private int DetermineSelectedSegment()
        {
            float normalizedRotation = (_currentRotation % 360f + 360f) % 360f;
            float pointerAngle = (450f - normalizedRotation) % 360f;

            float totalWeight = CalculateTotalWeight();
            float currentAngle = 0f;

            for (int i = 0; i < _segments.Count; i++)
            {
                float sweepAngle = 360f * (_segments[i].weight / totalWeight);
                float endAngle = currentAngle + sweepAngle;

                if (pointerAngle >= currentAngle && pointerAngle < endAngle)
                {
                    return i;
                }

                currentAngle = endAngle;
            }

            return 0;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Vector3 pointerPos = transform.position + Vector3.up * (wheelRadius + 0.5f);
            Gizmos.DrawLine(pointerPos, pointerPos + Vector3.down * 0.5f);
            Gizmos.DrawSphere(pointerPos + Vector3.down * 0.5f, 0.1f);
        }
#endif
    }
}
