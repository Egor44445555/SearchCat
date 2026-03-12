using UnityEngine;

public class Cat : MonoBehaviour
{
    [SerializeField] Sprite successImage;
    [SerializeField] GameObject particle;

    SpriteRenderer spriteRenderer;
    bool isActive = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (UIManager.main != null)
        {
            UIManager.main.IncreaseMaxSearchCount();
        }
    }
    
    public void SuccessItem()
    {
        spriteRenderer.sprite = successImage;
        Instantiate(particle, transform.position, Quaternion.identity);
        gameObject.tag = "Untagged";
        isActive = true;
    }

    public bool IsActive()
    {
        return isActive;
    }
}
