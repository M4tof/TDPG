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
        spriteObject.transform.localPosition = new Vector2(tileSize.x * multiplayer, tileSize.x * multiplayer);
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
        spriteObject.transform.localPosition = new Vector2(tileSize.x * multiplayer, tileSize.x * multiplayer);
    }

    //Get tile size
    public Vector2 GetTileSize()
    {
        return tileSize;
    }

    //Set Multiplayer
    public void SetMultiplayer(float multiplayer)
    {
        this.multiplayer = multiplayer;
        spriteObject.transform.localPosition = new Vector2(tileSize.x * multiplayer, tileSize.x * multiplayer);
    }
    
    //Get Multiplayer
    public float GetMultiplayer()
    {
        return multiplayer;
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
