using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HpUI : MonoBehaviour
{
    public Slider hpSlider;
    public float showDuration = 3f; // 3초 동안 데미지 없으면 다시 숨김
    private Coroutine _hideCoroutine;

    public void UpdateHpBar(float currentHp, float maxHp)
    {
        gameObject.SetActive(true);
        if (hpSlider != null) hpSlider.value = currentHp / maxHp;

        // 이미 숨기기 예약이 되어있다면 취소하고 새로 시작
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    public void ResetHpBar()
    {
        if (hpSlider != null) hpSlider.value = 1f; // 100%로 꽉 채움
        gameObject.SetActive(false); // 화면에서 숨김
    }
    
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        gameObject.SetActive(false); // 다시 숨김
        _hideCoroutine = null;
    }
}