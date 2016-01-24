using UnityEngine;
using System.Collections;

namespace Boardgame {
    [RequireComponent(typeof(CapsuleCollider))]
	public class Piece : MonoBehaviour {
        private Grid grid;
        private GameObject piece;

		[HideInInspector]
		public Cell Cell;
		[SerializeField]
        private GameObject GhostPrefab;
		[SerializeField]
		private GameObject DefaultPrefab;
		[SerializeField]
		private float HighlightOffsetHeight;

        public void Awake()
        {
            grid = GetComponentInParent<Grid>();
			Place(DefaultPrefab);
        }

        public void OnMouseEnter()
        {
			PlayerInteractionManager.Instance.PieceHighlight (this);
        }

        public void OnMouseExit()
        {
			PlayerInteractionManager.Instance.PieceLeave (this);
        }

		public void OnMouseUp() 
		{
			PlayerInteractionManager.Instance.PieceSelect (this);
		}

		public Vector3 GetHighlightPosition() {
			return transform.position + transform.localToWorldMatrix.MultiplyVector (new Vector3 (0, HighlightOffsetHeight, 0));
		}

        private void Place(GameObject prefab)
        {
            piece = Instantiate(prefab);
            piece.transform.SetParent(transform);
            piece.transform.localPosition = Vector3.zero;
        }
    }
}