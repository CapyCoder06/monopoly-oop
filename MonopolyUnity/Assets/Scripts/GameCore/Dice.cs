using Monopoly.UnityAdapter;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Dice : MonoBehaviour
{
    public string playerSlotId = "P1";
    public Image diceImage;
    public Sprite[] diceFaces; // 6 sprites tương ứng mặt 1–6
    public float rollDuration = 0.8f;

    private bool isRolling = false;

    public void OnRollButtonClicked()
    {
        if (isRolling) return;

        if (GameBridgeProxy.Bridge == null)
        {
            Debug.LogError("❌ GameBridge chưa được khởi tạo!");
            return;
        }

        GameBridgeProxy.Bridge.RollDice(playerSlotId);
    }

    // Nhận sự kiện từ EventBus (nếu bạn muốn animate thật)
    public void OnDiceRolled(int d1, int d2)
    {
        StartCoroutine(AnimateDice(d1));
    }

    private IEnumerator AnimateDice(int finalValue)
    {
        isRolling = true;

        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            diceImage.sprite = diceFaces[Random.Range(0, 6)];
            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(0.1f);
        }

        diceImage.sprite = diceFaces[finalValue - 1];
        isRolling = false;
    }
}
