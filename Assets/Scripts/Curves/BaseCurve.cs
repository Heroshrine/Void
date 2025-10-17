using UnityEngine;

namespace Void.Curves
{
    public abstract class BaseCurve : ScriptableObject
    {
        public abstract float Evaluate(float t);
    }
}