using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Wheels
{
    public class Wheel : MonoBehaviour
    {
        [Header("Wheel Settings")]
        [SerializeField] private float wheelRadius = 4f;
        [SerializeField] private Material segmentMaterial;
        [SerializeField] private TMP_FontAsset font;

        [SerializeField] private WheelSegmentData purpleSegmentData;
        [SerializeField] private WheelSegmentData blueSegmentData;

        [SerializeField] private Transform rouletteBall;

        public Action<Wheel, WheelSegmentData> OnWheelSpinCompleted;

        private readonly List<WheelSegmentData> _segments = new List<WheelSegmentData>();
        private readonly List<WheelSegmentVisual> _segmentVisuals = new List<WheelSegmentVisual>();
        private float _currentRotation = 0f;
        private float _ballRotation = 0f;
        private Vector3 ballPositionVector;
        private Coroutine _spinCoroutine;

        private float FinalRotationSpeed = 10f;

        private void Start()
        {
            ballPositionVector = rouletteBall.localPosition;
            FullRebuildWheel();
        }

        public void FullRebuildWheel()
        {
            _segments.Clear();
            _segments.AddRange(Player.Instance.GetWheelSegmentData());
            if (Player.Instance.unlocksPurpleAccumulator)
            {
                purpleSegmentData.segmentColor = Color.purple;
                purpleSegmentData.wedgeTypeColor = WheelColor.purple;
                _segments.Insert(Player.Instance.IndexOfPurpleAccumulator, purpleSegmentData);
            }
            if (Player.Instance.unlocksBlueAccumulator)
            {
                blueSegmentData.segmentColor = Color.blue;
                purpleSegmentData.wedgeTypeColor = WheelColor.blue;
                _segments.Insert(Player.Instance.IndexOfBlueAccumulator, blueSegmentData);
            }
            RebuildWheel();

            rouletteBall.localPosition = ballPositionVector;
            rouletteBall.rotation = Quaternion.identity;
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

        public bool IsSpinning()
        {
            return _spinCoroutine != null;
        }

        public void ForceStopWheel()
        {
            if (_spinCoroutine == null)
            {
                Debug.LogWarning("Tried to force stop wheel that wasn't spinning.");
                return;
            }
            StopCoroutine(_spinCoroutine);
            int selectedIndex = DetermineSelectedSegment();
            Debug.Log($"Force stopped on: {_segments[selectedIndex].prizeName}");

            _spinCoroutine = null;

            OnWheelSpinCompleted?.Invoke(this, _segments[selectedIndex]);
            Player.Instance.OnWheelSpinComplete(_segments[selectedIndex]);
        }

        public void SpinWheel(float spinSpeed, float ballSpeed)
        {
            // Don't spin if you're already spinning
            if (IsSpinning())
            {
                return;
            }

            _spinCoroutine = StartCoroutine(SpinCoroutine(spinSpeed, ballSpeed));
        }

        private float currentSpeed
        {
            get
            {
                if (ServiceLocator.Instance.CheatManager.SmoothWheelEnabled)
                {
                    return 100f;
                }
                return m_currentSpeed;
            }
            set
            {
                m_currentSpeed = value;
            }
        }
        private float m_currentSpeed;
        private float currentBallSpeed
        {
            get
            {
                if (ServiceLocator.Instance.CheatManager.SmoothWheelEnabled)
                {
                    return 100f;
                }
                return m_ballSpeed;
            }
            set
            {
                m_ballSpeed = value;
            }
        }
        private float m_ballSpeed;

        private System.Collections.IEnumerator SpinCoroutine(float initialSpeed, float initialBallSpeed)
        {
            currentSpeed = initialSpeed;
            currentBallSpeed = initialBallSpeed;
            float deceleration = initialSpeed / 3f;

            _ballRotation = 270f;
            rouletteBall.localPosition = Quaternion.AngleAxis(_ballRotation, Vector3.forward) * ballPositionVector;

            while (currentSpeed > 10f)
            {
                _currentRotation += currentSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Euler(0, 0, _currentRotation);

                if (rouletteBall != null)
                {
                    _ballRotation -= currentBallSpeed * Time.deltaTime;
                    rouletteBall.localPosition = Quaternion.AngleAxis(_ballRotation, Vector3.forward) * ballPositionVector;
                    //rouletteBall.rotation = Quaternion.Euler(0, 0, _ballRotation);
                }

                currentSpeed -= deceleration * Time.deltaTime;
                currentBallSpeed -= deceleration * Time.deltaTime;
                yield return null;
            }

            // Lock in the winning segment, then compute the rotation that centres the pointer on it.
            int selectedIndex = DetermineSelectedSegment();
            int ballIndex = 0;
            float segmentCenterAngle = CalculateSegmentCenterAngle(selectedIndex);

            // Invert the pointer formula: pointerAngle = (450 - normalizedRotation) % 360
            // => normalizedRotation = (450 - segmentCenterAngle + 360) % 360
            float desiredNormalized = (450f - segmentCenterAngle + 360f) % 360f;
            _currentRotation = (_currentRotation % 360f + 360f) % 360f;
            _ballRotation = (_ballRotation % 360f + 360f) % 360f;

            float delta = desiredNormalized - _currentRotation;
            if (delta > 180f) delta -= 360f;
            if (delta < -180f) delta += 360f;

            float targetAngle = _currentRotation + delta;
            float targetBallAngle = _ballRotation - delta;

            while (Mathf.Abs(_currentRotation - targetAngle) > 0.05f)
            {
                _currentRotation = Mathf.Lerp(_currentRotation, targetAngle, Time.deltaTime * FinalRotationSpeed);
                transform.rotation = Quaternion.Euler(0, 0, _currentRotation);

                yield return null;
            }

            _currentRotation = targetAngle;
            transform.rotation = Quaternion.Euler(0, 0, _currentRotation);

            if (rouletteBall != null)
            {
                ballIndex = DetermineSelectedSegmentFromBall();
            }

            Debug.Log($"Landed on: {_segments[selectedIndex].prizeName}");
            Debug.Log($"Ball landed on: {ballIndex}, which has prizeName: {_segments[ballIndex].prizeName}");

            _spinCoroutine = null;

            OnWheelSpinCompleted?.Invoke(this, _segments[selectedIndex]);
            Player.Instance.OnWheelSpinComplete(_segments[selectedIndex]);
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

        /// <summary>
        /// Determines the selected segment based on the roulette ball's position relative to the wheel.
        /// The ball orbits in the parent's local space, so its angle must be offset by the wheel's
        /// current rotation to arrive at segment-space, matching the convention used by DetermineSelectedSegment.
        /// </summary>
        private int DetermineSelectedSegmentFromBall()
        {
            float ballAngleDeg = Mathf.Atan2(rouletteBall.localPosition.y, rouletteBall.localPosition.x) * Mathf.Rad2Deg;
            float wheelNormalized = (_currentRotation % 360f + 360f) % 360f;
            float pointerAngle = ((ballAngleDeg - wheelNormalized + 360f) % 360f + 360f) % 360f;
            Debug.Log($"Ball segment-space angle: {pointerAngle} (ballAngle: {ballAngleDeg}, wheelRotation: {wheelNormalized})");

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
