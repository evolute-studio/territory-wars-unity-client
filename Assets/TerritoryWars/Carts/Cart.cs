using UnityEngine;

namespace TerritoryWars.Carts
{
    public class Cart
    {
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public Vector2 Direction;
        public bool IsMoving = false;
        
        public Cart(GameObject gameObject)
        {
            this.gameObject = gameObject;
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }
    }
}