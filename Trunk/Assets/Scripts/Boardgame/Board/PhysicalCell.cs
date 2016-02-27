﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Boardgame {
    [RequireComponent(typeof(SphereCollider))]
    public class PhysicalCell : MonoBehaviour {
        private List<GameObject> piece = new List<GameObject>();
        public bool isPile = false;

        public string id;

        public void SetColliderRadius(float radius) {
            GetComponent<SphereCollider>().radius = radius;
        }

        public bool isEmpty() {
            return piece.Count == 0;
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
            if (!isInstantiated) {
                piece.Add(Instantiate(go));
            } else {
                piece.Add(go);
            }
            piece[piece.Count-1].transform.SetParent(transform);
            piece[piece.Count-1].transform.localPosition = Vector3.zero;
        }

        public bool HasPiece() {
            return piece.Count > 0;
        }

        public GameObject RemovePiece() {
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