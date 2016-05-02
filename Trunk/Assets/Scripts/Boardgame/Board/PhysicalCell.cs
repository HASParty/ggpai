using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Boardgame {
    /// <summary>
    /// Interactable physical representation of a cell
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class PhysicalCell : MonoBehaviour {
        private List<GameObject> piece = new List<GameObject>();
        public bool isPile = false;

        public string id;

        public void SetColliderRadius(float radius) {
            SphereCollider col = GetComponent<SphereCollider>();
            col.radius = radius;
            col.center = new Vector3(col.center.x, col.center.y - radius/2, col.center.z);
        }

        public bool isEmpty() {
            return piece.Count == 0;
        }

        public void OnLookAt() {
            if (HasPiece()) {
                PlayerInteractionManager.Instance.PieceHighlight(this);
            } else {
                PlayerInteractionManager.Instance.CellHighlight(this);
            }
        }

        public void OnLookAway() {
            if (HasPiece()) {
                PlayerInteractionManager.Instance.PieceLeave(this);
            }
        }

        public void OnSelect() {
            if (HasPiece()) {
                PlayerInteractionManager.Instance.PieceSelect(this);
            } else {
                PlayerInteractionManager.Instance.CellSelect(this);
            }
        }

        public void Place(GameObject go, bool isInstantiated = true) {
            if (!isInstantiated) {
                piece.Add(Instantiate(go));
            } else {
                piece.Add(go);
            }
            piece[piece.Count-1].transform.SetParent(transform);
            piece[piece.Count-1].transform.localPosition = Vector3.zero;
            piece[piece.Count - 1].transform.localRotation = Quaternion.identity;
        }

        public bool HasPiece() {
            return piece.Count > 0;
        }

        public void Clear() {
            while(piece.Count > 0) {
                GameObject ret = piece[piece.Count - 1];
                piece.RemoveAt(piece.Count - 1);
                Destroy(ret);
            }
        }

        public GameObject RemovePiece() {
            if(piece.Count == 0) {
                Debug.LogWarning("No piece to remove at "+id);
                return null;
            }
            GameObject ret = piece[piece.Count-1];
            piece.RemoveAt(piece.Count - 1);
            if (isPile) {
                Destroy(ret.GetComponent<Rigidbody>());
                ret.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            return ret;
        }
    }
}