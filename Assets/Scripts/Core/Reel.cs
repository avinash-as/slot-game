using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotGame.Symbols;
using SlotGame.Utilities;

namespace SlotGame.Core
{
    public class Reel : MonoBehaviour
    {
        [SerializeField] private int visibleCount = 3;
        [SerializeField] private float symbolSpacing = 1.2f;
        [SerializeField] private Vector3 startPosition = new Vector3(0f, 1.2f, 0f);

        [SerializeField] private float spinSpeed = 22f;
        [SerializeField] private float accelTime = 0.25f;
        [SerializeField] private float decelTime = 0.45f;

        [SerializeField] private List<SymbolData> symbolPool;

        private SymbolView[] _buffer;
        private int _bufferSize;
        private bool _isSpinning;
        private bool _requestStop;
        private SymbolData[] _targetSymbols;

        public bool IsSpinning => _isSpinning;
        public SymbolData[] CurrentSymbols { get; private set; }

        public void SetSymbolPool(List<SymbolData> pool) => symbolPool = pool;

        public void Initialize(SymbolView prefab, Transform parent)
        {
            _bufferSize = visibleCount + 2;
            _buffer = new SymbolView[_bufferSize];
            CurrentSymbols = new SymbolData[visibleCount];

            for (int i = 0; i < _bufferSize; i++)
            {
                var v = Instantiate(prefab, parent);
                v.transform.localPosition = startPosition + Vector3.down * (i * symbolSpacing);
                _buffer[i] = v;
            }
            RandomizeAll();
        }

        private void RandomizeAll()
        {
            for (int i = 0; i < _bufferSize; i++)
                _buffer[i].SetData(PickRandomSymbol());
        }

        private SymbolData PickRandomSymbol()
        {
            if (symbolPool == null || symbolPool.Count == 0) return null;
            int[] weights = new int[symbolPool.Count];
            for (int i = 0; i < symbolPool.Count; i++) weights[i] = symbolPool[i].weight;
            return symbolPool[RNGService.WeightedIndex(weights)];
        }

        public void StartSpin()
        {
            if (_isSpinning) return;
            _isSpinning = true;
            _requestStop = false;
            StartCoroutine(SpinRoutine());
        }

        public void Stop(SymbolData[] targetSymbols)
        {
            if (!_isSpinning) return;
            _targetSymbols = targetSymbols;
            _requestStop = true;
        }

        private IEnumerator SpinRoutine()
        {
            float speed = 0f;
            float t = 0f;

            while (t < accelTime && !_requestStop)
            {
                t += Time.deltaTime;
                speed = Mathf.Lerp(0f, spinSpeed, t / accelTime);
                yield return MoveStep(speed);
            }

            speed = spinSpeed;
            while (!_requestStop)
                yield return MoveStep(speed);

            t = 0f;
            while (t < decelTime)
            {
                t += Time.deltaTime;
                speed = Mathf.Lerp(spinSpeed, 0f, t / decelTime);
                yield return MoveStep(speed);
            }

            SnapToTargets();
            _isSpinning = false;
        }

        private IEnumerator MoveStep(float speed)
        {
            for (int i = 0; i < _bufferSize; i++)
                _buffer[i].transform.localPosition += Vector3.down * (speed * Time.deltaTime * symbolSpacing);

            for (int i = 0; i < _bufferSize; i++)
            {
                if (_buffer[i].transform.localPosition.y < startPosition.y - (visibleCount + 1) * symbolSpacing)
                {
                    float maxY = float.MinValue;
                    foreach (var v in _buffer) maxY = Mathf.Max(maxY, v.transform.localPosition.y);
                    _buffer[i].transform.localPosition = new Vector3(startPosition.x, maxY + symbolSpacing, startPosition.z);
                    _buffer[i].SetData(PickRandomSymbol());
                }
            }
            yield return null;
        }

        private void SnapToTargets()
        {
            System.Array.Sort(_buffer, (a, b) => b.transform.localPosition.y.CompareTo(a.transform.localPosition.y));
            for (int i = 0; i < _bufferSize; i++)
            {
                _buffer[i].transform.localPosition = startPosition + Vector3.down * (i * symbolSpacing);
                if (i < visibleCount)
                {
                    if (_targetSymbols != null && i < _targetSymbols.Length)
                        _buffer[i].SetData(_targetSymbols[i]);
                    CurrentSymbols[i] = _buffer[i].Data;
                }
            }
        }
    }
}