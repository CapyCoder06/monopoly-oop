using UnityEngine;

namespace Monopoly.UnityAdapter
{
    public class PlayerToken : MonoBehaviour
    {
        public string PlayerId = "P1";
        public int Position { get; private set; } = 0;
        public float moveSpeed = 5f;

        private BoardView _boardView;

        public void Init(BoardView boardView, string playerId, int startPos = 0)
        {
            _boardView = boardView;
            PlayerId = playerId;
            Position = startPos;

            transform.position = _boardView.GetTilePosition(Position);

            // Tự đăng ký với GameController
            var controller = FindObjectOfType<GameController>();
            controller?.RegisterPlayer(this);
        }

        public void MoveToTile(int newPosition)
        {
            Position = newPosition % _boardView.Tiles.Length;
            StopAllCoroutines();
            StartCoroutine(MoveSmoothly(_boardView.GetTilePosition(Position)));
        }

        private System.Collections.IEnumerator MoveSmoothly(Vector3 target)
        {
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}
