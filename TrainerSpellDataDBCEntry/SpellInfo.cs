using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainerSpellDataDBCEntry
{
    class SpellInfo
    {
        public int ID { get; set; }
        public int TrainSpellID { get; set; }
        public int AltTrainSpellID { get; set; }
        public string Name { get; set; }
        public string RankText { get; set; }
        public int Rank { get; set; }
        public int BaseLevel { get; set; }
        public float CopperCost { get; set; }
        public float SkillCost { get; set; }
        public float TalentCost { get; set; }
        public int ClassMask { get; set; }
        public string Class { get; set; }
        public int TrainerID { get; set; }
        public int LowLvlTrainerID { get; set; }
        public int FirstSpellInChain { get; set; }
        public int PrevSpellInChain { get; set; }
        public int SkillLineID { get; set; }
        public string SkillLine { get; set; }
    }
}
