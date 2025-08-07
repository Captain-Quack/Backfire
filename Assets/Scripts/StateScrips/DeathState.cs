namespace Backfire
{
    public class DeathState : PlayerState
    {
        public override PlayerStateType StateType => PlayerStateType.Death;
        public DeathState(PlayerController player) : base(player) {}

        public override void Update()
        {
            
        }
        public override void FixedUpdate() { }
        
        public override void OnExit() { }
        
        public override void OnEnter() { }
    }
}