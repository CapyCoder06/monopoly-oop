using Monopoly.CrossBridge;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Monopoly.DiceManager
{
    public class UnityDiceAdapter : MonoBehaviour, IDiceBridge
    {
        [Header("🎲 Dice UI References")]
        public Image diceImage1;
        public Image diceImage2;
        public Sprite[] diceFaces; // 6 sprite tương ứng mặt 1–6

        [Header("⚙️ Settings")]
        public float rollDuration = 0.5f; // thời gian tung xúc xắc

        private System.Random random = new System.Random();

        public (int d1, int d2, int sum, bool isDouble) Roll()
        {
            // Tung xúc xắc thật (sau khi hiệu ứng xong)
            int d1 = random.Next(1, 7);
            int d2 = random.Next(1, 7);
            int sum = d1 + d2;
            bool isDouble = (d1 == d2);

            // Bắt đầu hiệu ứng xoay
            StartCoroutine(RollAnimation(d1, d2));

            Debug.Log($"🎲 Dice rolled → {d1} & {d2} (sum={sum}, double={isDouble})");
            return (d1, d2, sum, isDouble);
        }

        private IEnumerator RollAnimation(int final1, int final2)
        {
            float elapsed = 0f;
            while (elapsed < rollDuration)
            {
                int temp1 = random.Next(1, 7);
                int temp2 = random.Next(1, 7);

                UpdateDiceImages(temp1, temp2);
                yield return new WaitForSeconds(0.05f);
                elapsed += 0.05f;
            }

            // Hiển thị kết quả thật
            UpdateDiceImages(final1, final2);
        }

        private void UpdateDiceImages(int d1, int d2)
        {
            if (diceFaces == null || diceFaces.Length < 6)
                return;

            if (diceImage1 != null)
                diceImage1.sprite = diceFaces[d1 - 1];

            if (diceImage2 != null)
                diceImage2.sprite = diceFaces[d2 - 1];
        }
    }
}
