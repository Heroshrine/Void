using UnityEngine;

namespace Void.Curves
{
    [CreateAssetMenu(fileName = "1 to 1 Curve", menuName = "Void/Curves/1 to 1", order = 0)]
    public class OneToOneCurve : BaseCurve
    {
        public override float Evaluate(float t) => t;
    }
}