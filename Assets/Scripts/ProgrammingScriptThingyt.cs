using UnityEngine; 
// @formatter:off
// ReSharper disable UseNameofExpression
 // ReSharper disable once ArrangeTypeMemberModifiers

namespace Backfire {
    public class Programming : TeamBehaviour {
        public void Start() {
            InvokeRepeating("MakeGames", 24f ,7f);
        }

        void MakeGames() 
        {
            return;
        }
    }
}

public class TeamBehaviour : MonoBehaviour {}