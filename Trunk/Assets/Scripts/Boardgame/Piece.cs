using UnityEngine;
using System.Collections;

namespace Boardgame {
    [RequireComponent(typeof(CapsuleCollider))]
	public class Piece : MonoBehaviour {
        private GameObject piece;

		[HideInInspector]
		public Cell Cell;
		[SerializeField]
        private GameObject ghostPrefab;
		[SerializeField]
		private GameObject defaultPrefab;

        public void Awake()
        {
			Place(defaultPrefab);
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
			return transform.position;
		}

        public Vector3 GetSelectPosition()
        {
            return transform.position;
        }

        private void Place(GameObject prefab)
        {
            piece = Instantiate(prefab);
            piece.transform.SetParent(transform);
            piece.transform.localPosition = Vector3.zero;
        }
    }
}