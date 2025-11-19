using UnityEngine;

public class TurretBase : MonoBehaviour
{

    [Header("Parameters")] 
    [SerializeField] private int id;    //TODO Serialize is only for testing
    [SerializeField] private Vector2 tileSize = new Vector2(1, 1);
    [SerializeField] private float multiplyer = 5.0f;
    
    
    [SerializeField] private GameObject spriteObject;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteObject.transform.localPosition = new Vector2(tileSize.x * multiplyer, tileSize.x * multiplyer);
    }

    //Set Id
    public void SetId(int id)
    {
        this.id = id;
    }
    
    public int GetId()
    {
        return this.id;
    }
    
    //Set TileSize
    public void SetTileSize(Vector2 tileSize)
    {
        this.tileSize = tileSize;
        spriteObject.transform.localPosition = new Vector2(tileSize.x * multiplyer, tileSize.x * multiplyer);
    }

    //Get tile size
    public Vector2 GetTileSize()
    {
        return tileSize;
    }

    //Set multiplyer
    public void SetMultiplyer(float multiplyer)
    {
        this.multiplyer = multiplyer;
        spriteObject.transform.localPosition = new Vector2(tileSize.x * multiplyer, tileSize.x * multiplyer);
    }
    
    //Get multiplyer
    public float GetMultiplyer()
    {
        return multiplyer;
    }

    public void SetSprite(Sprite sprite)
    {
        spriteObject.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public Sprite GetSprite()
    {
        return spriteObject.GetComponent<SpriteRenderer>().sprite;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteObject.GetComponent<SpriteRenderer>();
    }
    
    void OnValidate()
    {
        if (spriteObject == null)
        {
            Debug.LogWarning("Sprite Object is not assigned", this);
        }
    }
}
