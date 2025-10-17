using UnityEngine;

namespace Void
{
    public class Mineable : MonoBehaviour
    {
        [SerializeField] private SetOrder _orderSetter;

        public void Refresh()
        {
            _orderSetter.DoSetOrder();
        }

        public void LockPosition() => _orderSetter.DoSetOrder();
    }
}