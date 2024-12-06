using UnityEngine;

public class PlayerHUDToggle : MonoBehaviour
{
    // UI를 나타내고 숨길 대상 프리팹 (Player HUD)
    public GameObject playerHUD;

    // Update는 매 프레임마다 호출됨
    void Update()
    {
        // "i" 버튼이 눌렸는지 확인
        if (Input.GetKeyDown(KeyCode.I))
        {
            // 현재 playerHUD의 활성화 상태를 토글
            playerHUD.SetActive(!playerHUD.activeSelf);
        }
    }
}