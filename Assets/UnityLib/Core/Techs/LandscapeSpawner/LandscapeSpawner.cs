using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle.Core {
    public class LandscapeSpawner : MonoBehaviour {
        [Tooltip("Determines the range of local coordinates in which the spawned blocks move before being destroyed (x must be lower than y)")]
        [SerializeField] private Vector2 _blocksLocalPositionRange;

        [SerializeField] private LandscapeBlock _blockPrefab;
        [SerializeField] private float _currentSpeed = 1;
        [SerializeField] private float _targetSpeed = 1;
        [SerializeField] private float _acceleration = 1;


        private List<LandscapeBlock> _blocks = new List<LandscapeBlock>();

        public void SetCurrentSpeed(float speed) {
            _currentSpeed = speed;
        }

        public void SetTargetSpeed(float speed) {
            _targetSpeed = speed;
        }
        
        private void Update() {
            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.deltaTime * _acceleration);
            _blocks[0].transform.position -= transform.forward * _currentSpeed * Time.deltaTime;
            for (int i = 1; i < _blocks.Count; i++) {
                _blocks[i].transform.position = _blocks[i-1].transform.position + transform.forward * _blocks[i].Length;
            }
            if (transform.InverseTransformPoint(_blocks[0].transform.position).z < _blocksLocalPositionRange.x) {
                Destroy(_blocks[0].gameObject);
                _blocks.RemoveAt(0);
            }
            LandscapeBlock endBlock = _blocks[_blocks.Count - 1];
            if (endBlock.Extendable && transform.InverseTransformPoint(endBlock.transform.position).z < _blocksLocalPositionRange.y) {
                InsertBlockAtTheEnd();
            }
        }

        private bool InsertBlockAtTheEnd() {
            LandscapeBlock endBlock = _blocks[_blocks.Count - 1];
            if (!endBlock.Extendable) {
                return false;
            }
            LandscapeBlock newBlockPrefab = GetExtensionBlock();
            Transform newBlock = Instantiate(newBlockPrefab.transform,
                endBlock.transform.position + transform.forward * (endBlock.Length + newBlockPrefab.Length) / 2, transform.rotation) as Transform;
            newBlock.gameObject.SetActive(true);
            newBlock.SetParent(transform);
            _blocks.Add(newBlock.GetComponent<LandscapeBlock>());
            return true;
        }

        private void InsertBlockAtStart() {
            LandscapeBlock startBlock = _blocks[0];
            LandscapeBlock newBlockPrefab = GetExtensionBlock();
            Transform newBlock = Instantiate(newBlockPrefab.transform,
                startBlock.transform.position - transform.forward * (startBlock.Length + newBlockPrefab.Length) / 2, transform.rotation) as Transform;
            newBlock.gameObject.SetActive(true);
            newBlock.SetParent(transform);
            _blocks.Insert(0, newBlock.GetComponent<LandscapeBlock>());
        }

        private LandscapeBlock GetExtensionBlock() {
            return _blockPrefab;
        }

        public void Initialize(float zOffset, LandscapeBlock initialBlockPrefab) {
            foreach (LandscapeBlock block in _blocks) {
                Destroy(block);
            }
            _blocks.Clear();
            LandscapeBlock newBlock =
                (Instantiate(initialBlockPrefab.transform, transform.position + transform.forward * zOffset, transform.rotation) as Transform)
                .GetComponent<LandscapeBlock>();
            newBlock.gameObject.SetActive(true);
            newBlock.transform.SetParent(transform);
            _blocks.Add(newBlock);
            //Insert blocks to fill the required coordinate range
            Vector3 localPos = transform.InverseTransformPoint(newBlock.transform.position);
            bool blockExtensible = false;
            while (localPos.z < _blocksLocalPositionRange.y && blockExtensible) {
                blockExtensible = InsertBlockAtTheEnd();
                localPos = transform.InverseTransformPoint(_blocks[_blocks.Count - 1].transform.position);
            }
            localPos = transform.InverseTransformPoint(newBlock.transform.position);
            while (localPos.z > _blocksLocalPositionRange.x) {
                InsertBlockAtStart();
                localPos = transform.InverseTransformPoint(_blocks[0].transform.position);
            }
        }

        private void Start() {
            Initialize(0, _blockPrefab);
        }
    }
}
