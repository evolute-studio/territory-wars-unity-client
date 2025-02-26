using UnityEngine;

namespace TerritoryWars.Carts
{
    public class OwnerPin
    {
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public Vector2 Direction;
        public bool IsMoving = false;
        
        public OwnerPin(GameObject gameObject)
        {
            this.gameObject = gameObject;
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }
    }
}