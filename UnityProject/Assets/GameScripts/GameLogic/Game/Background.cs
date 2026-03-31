using System;
using UnityEngine;

namespace GameLogic
{
    public class Background : MonoBehaviour
    {
        [SerializeField] private Transform bg1;
        
        [SerializeField] private Transform bg2;
        
        [SerializeField] private float bgSpeed = 1;
        
        [SerializeField] private BoxCollider2D playerMoveBoundary = null;

        private readonly float _bgLength = 11.5f;
        
        public BoxCollider2D PlayerMoveBoundary
        {
            get
            {
                return playerMoveBoundary;
            }
        }
        
        private void Update()
        {
            var y1 = bg1.transform.localPosition.y - Time.deltaTime * bgSpeed;
            if (y1 < -_bgLength)
            {
                y1 += _bgLength * 2;
            }
            bg1.transform.localPosition = new Vector3(0, y1, 0);
            var y2 = bg2.transform.localPosition.y - Time.deltaTime * bgSpeed;
            if (y2 < -_bgLength)
            {
                y2 += _bgLength * 2;
            }
            bg2.transform.localPosition = new Vector3(0, y2, 0);
        }
    }
}