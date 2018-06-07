using System.Collections.Generic;

namespace Russia2018.Model
{
    public class MoveResult
    {
        public MoveResult()
        {
        }
        
        public List<DiscoidPosition> DiscoidPositions { get; set; }
        public bool IsTurnOver { get; set; }
    }
}
