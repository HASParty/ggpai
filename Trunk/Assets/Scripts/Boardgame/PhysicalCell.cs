using UnityEngine;
using System.Collections;
using System;

namespace Boardgame {
    [RequireComponent(typeof(SphereCollider))]
    public class PhysicalCell : MonoBehaviour {
        private GameObject piece;

        public string id;

        public void SetColliderRadius(float radius) {
            GetComponent<SphereCollider>().radius = radius;
        }

        public void OnMouseEnter() {
            if (HasPiece()) {
                PlayerInteractionManager.Instance.PieceHighlight(this);
            } else {
                PlayerInteractionManager.Instance.CellHighlight(this);
            }
        }

        public void OnMouseExit() {
            if (HasPiece()) {
                PlayerInteractionManager.Instance.PieceLeave(this);
            }
        }

        public void OnMouseUp() {
            if (HasPiece()) {
                PlayerInteractionManager.Instance.PieceSelect(this);
            } else {
                PlayerInteractionManager.Instance.CellSelect(this);
            }
        }

        public void Place(GameObject go, bool isInstantiated = true) {
            if (!HasPiece()) {
                if (!isInstantiated) {
                    piece = Instantiate(go);
                } else {
                    piece = go;
                }
                piece.transform.SetParent(transform);
                piece.transform.localPosition = Vector3.zero;
            }
        }

        public bool HasPiece() {
            return piece != null;
        }

        public GameObject RemovePiece() {
            GameObject ret = piece;
            piece = null;
            return ret;
        }
    }
}