using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class MiniGameData
    {
        public int MedicineChapterId { get; set; }
        public List<MiniGameCase> MiniGameCases { get; set; }
    }

    public class MiniGameCase
    {
        public int CaseId { get; set; }
        public string QuestionText { get; set; }
        public int[] TargetId { get; set; }
        public int Score { get; set; }
    }
}
