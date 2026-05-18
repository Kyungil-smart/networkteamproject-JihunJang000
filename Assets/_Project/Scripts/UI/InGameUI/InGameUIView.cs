using System;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIView : MonoBehaviour
{
    [Header("인게임 판넬들")]
    public GameObject hudPanel;
    public GameObject gameOverPanel;

    [Header("종료 화면 UI")]
    public Button returnToLobbyButton;

    [Header("스킬 쿨타임 부품들")]
    public SkillCooldownUI q_Cooldown;
    public SkillCooldownUI e_Cooldown;
    public SkillCooldownUI r_Cooldown;
    
    public Action OnReturnToLobbyClicked;

    private void Awake()
    {
        returnToLobbyButton.onClick.AddListener(() => OnReturnToLobbyClicked?.Invoke());
    }

    public void ShowGameOverPanel()
    {
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowHUDPanel()
    {
        hudPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void StartCooldownUI(int skillSlot, float time)
    {
        if (skillSlot == 0) q_Cooldown.StartCooldown(time);
        else if (skillSlot == 1) e_Cooldown.StartCooldown(time);
        else if (skillSlot == 2) r_Cooldown.StartCooldown(time);
    }
}