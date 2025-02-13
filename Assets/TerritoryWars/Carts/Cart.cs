using UnityEngine;

namespace TerritoryWars.Carts
{
    public class Cart
    {
        public GameObject gameObject;
        private Vector2 Direction;
        public bool IsMoving = false;
        
        public Cart(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
        
        public void Initialize()
        {
            
        }
        
        public void Move(Vector2 direction)
        {
            if (Direction == direction) return;
            Direction = direction;
            // if(Direction.x >= 1) transform.localScale = new Vector3(-1, 1, 1);
            // else if(Direction.x < -1) transform.localScale = new Vector3(1, 1, 1);
        }
    }
}