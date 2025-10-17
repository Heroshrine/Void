using System;
using UnityEngine;

namespace Void
{
    [RequireComponent(typeof(MonoBehaviour))]
    public class SetOrder : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        private void OnEnable()
        {
            DoSetOrder();
        }
        
        public void DoSetOrder() => _spriteRenderer.sortingOrder = Mathf.CeilToInt(-transform.position.y * 123);
    }
}