using UnityEngine;

namespace Backfire
{
    internal interface IParryable
    {
        public void Parry(Vector2 away, bool isPerfect = false);
    }
}