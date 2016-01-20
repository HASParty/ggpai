using UnityEngine;
using System.Collections;

namespace Boardgame {
    [RequireComponent(typeof(CapsuleCollider))]
	public class Piece : MonoBehaviour {
        private Grid grid;
        private GameObject piece;

        public GameObject HighlightPrefab;
        public GameObject GhostPrefab;
        public GameObject DefaultPrefab;

        private Mode mode;

        private enum Mode
        {
            Highlight,
            Default,
            Ghost,
            None
        }

        public void Awake()
        {
            grid = GetComponentInParent<Grid>();
            mode = Mode.None;
            ShowNormal();
        }

        public void ShowHighlight()
        {
            if (mode == Mode.Highlight) return;
            if (piece != null) Destroy(piece);
            mode = Mode.Highlight;
            Place(HighlightPrefab);
        }

        public void ShowNormal()
        {
            if (mode == Mode.Default) return;
            if (piece != null) Destroy(piece);
            mode = Mode.Default;
            Place(DefaultPrefab);
        }

        public void ShowGhost()
        {
            if (mode == Mode.Ghost) return;
            if (piece != null) Destroy(piece);
            mode = Mode.Ghost;
            Place(GhostPrefab);
        }

        public void OnMouseEnter()
        {
            if (mode == Mode.Default) ShowNormal(); ShowHighlight();
        }

        public void OnMouseExit()
        {
            if(mode == Mode.Highlight) ShowNormal();
        }

        private void Place(GameObject prefab)
        {
            piece = Instantiate(prefab);
            piece.transform.SetParent(transform);
            piece.transform.localPosition = Vector3.zero;
        }
    }
}