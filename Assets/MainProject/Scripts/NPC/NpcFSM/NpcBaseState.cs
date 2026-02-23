using UnityEngine;

namespace GhostStory
{
    public abstract class NpcBaseState
    {
        public abstract void EnterState(NpcController npc);
        public abstract void UpdateState(NpcController npc);
        public abstract void ExitState(NpcController npc);
    }
}
