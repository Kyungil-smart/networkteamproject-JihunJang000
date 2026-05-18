using UnityEngine;

// 싱글톤 선언만 
public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}