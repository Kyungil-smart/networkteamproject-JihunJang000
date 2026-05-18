using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    [Header("연결할 스킬 이미지 (위에 덮어씌울 밝은 원본)")]
    public Image brightSkillImage; // 🚨 이제 까만 막이 아니라 진짜 스킬 아이콘을 연결합니다!

    private float _maxCooldown;
    private float _currentCooldown;
    private bool _isOnCooldown = false;

    private void Update()
    {
        if (_isOnCooldown)
        {
            _currentCooldown -= Time.deltaTime;
            
            float fillRatio = (_maxCooldown - _currentCooldown) / _maxCooldown;
            brightSkillImage.fillAmount = fillRatio;

            if (_currentCooldown <= 0f)
            {
                _isOnCooldown = false;
                brightSkillImage.fillAmount = 1f; // 쿨타임 끝나면 100% 꽉 찬 밝은 상태로 고정!
            }
        }
    }

    public void StartCooldown(float cooldownTime)
    {
        _maxCooldown = cooldownTime;
        _currentCooldown = cooldownTime;
        _isOnCooldown = true;
        
        brightSkillImage.fillAmount = 0f; 
    }
}