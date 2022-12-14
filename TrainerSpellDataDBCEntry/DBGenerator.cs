using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace TrainerSpellDataDBCEntry
{
    enum TrainerType
    {
        CLASS,
        TALENT,
        PROFESSION
    }

    public partial class DBGenerator : Form
    {
        DBConnector m_dbcConnector;

        int TALENT_TRAINER_ID = 1000;

        int[] SKILL_LINES_TRAINER = { 6, 8, 26, 38, 39, 40, 50, 51, 56, 78, 96, 120, 130, 134, 163, 184, 198, 199, 237, 238, 239, 241, 243, 244, 245, 246, 247, 253, 254, 255, 256, 258, 259, 260, 261, 262, 263, 264, 267, 269, 271, 353, 354 };
        int[] SKILL_LINES_TALENT = { 222, 230, 231, 233, 234 };

        // 51, 242, 181 removed
        int[] SKILL_LINES_PROFESSION = { 129, 142, 164, 165, 171, 182, 185, 186, 197, 202, 249, 356 };

        Dictionary<string, int> m_skillNameToID = new Dictionary<string, int>();

        Dictionary<int, float> m_spellLevelToPrice = new Dictionary<int, float>(); // price is in copper, so price of 80 is 80 copper, price of 200 is 2 silver, etc.

        Dictionary<string, int> m_catsAgilityTrainerSpells = new Dictionary<string, int>();

        //Dictionary<int, string> SKILL_LINE_NAMES = new Dictionary<int, string>();

        public DBGenerator()
        {
            InitializeComponent();
        }

        private void DBGenerator_Load(object sender, EventArgs e)
        {
            m_dbcConnector = new DBConnector("localhost", "alpha_dbc", "root", "root");

            if (m_dbcConnector.Start() == 0)
            {
                MessageBox.Show("Connection with database established.");

                m_dbcConnector.SQLConnection.Close();

                PopulateLevelToPriceDict(ref m_spellLevelToPrice);
            }
            else
            {
                MessageBox.Show("Connection could not be established.");
            }

            // Weapon skills
            m_skillNameToID.Add("2H Axe", 172);
            m_skillNameToID.Add("2H Mace", 160);
            m_skillNameToID.Add("2H Sword", 55);
            m_skillNameToID.Add("Axe", 44);
            m_skillNameToID.Add("Mace", 54);
            m_skillNameToID.Add("Sword", 43);
            m_skillNameToID.Add("Crossbow", 226);
            m_skillNameToID.Add("Bow", 45);
            m_skillNameToID.Add("Dagger", 173);
            m_skillNameToID.Add("Throwing", 176);
            m_skillNameToID.Add("Gun", 46);
            m_skillNameToID.Add("Staff", 136);

            // Magic talents
            m_skillNameToID.Add("Fire", 8);
            m_skillNameToID.Add("Frost", 6);
            m_skillNameToID.Add("Holy", 56);
            m_skillNameToID.Add("Nature", 96);
            m_skillNameToID.Add("Shadow", 78);
            m_skillNameToID.Add("Wand", 228);

            m_catsAgilityTrainerSpells.Add("Rank 1", 4179);
            m_catsAgilityTrainerSpells.Add("Rank 2", 4180);
            m_catsAgilityTrainerSpells.Add("Rank 3", 4181);
            m_catsAgilityTrainerSpells.Add("Rank 4", 4182);
            m_catsAgilityTrainerSpells.Add("Rank 5", 4183);
            m_catsAgilityTrainerSpells.Add("Rank 6", 4184);
            m_catsAgilityTrainerSpells.Add("Rank 7", 4185);
            m_catsAgilityTrainerSpells.Add("Rank 8", 4186);
            m_catsAgilityTrainerSpells.Add("Rank 9", 5062);
            m_catsAgilityTrainerSpells.Add("Rank 10", 5068);
            m_catsAgilityTrainerSpells.Add("Rank 11", 5063);
            m_catsAgilityTrainerSpells.Add("Rank 12", 5064);
            m_catsAgilityTrainerSpells.Add("Rank 13", 5065);
            m_catsAgilityTrainerSpells.Add("Rank 14", 5066);
            m_catsAgilityTrainerSpells.Add("Rank 15", 5067);

        }

        private void buttonGenerateSpellInfo_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> SKILL_LINE_NAMES = GetSkillLineNames(SKILL_LINES_TRAINER);
            List<KeyValuePair<int, int>> SKILL_LINE_ABILITIES = GetSkillLineAbilities(SKILL_LINES_TRAINER);
            List<SpellInfo> spellList = new List<SpellInfo>();
            
            foreach(KeyValuePair<int, int> pair in SKILL_LINE_ABILITIES)
            {
                SpellInfo spell = new SpellInfo();

                spell.SkillLineID = pair.Key;
                spell.ID = pair.Value;

                string skillLineName = "";

                for (int i = 0; i < SKILL_LINES_TRAINER.Length; i++)
                {
                    if (pair.Key == SKILL_LINES_TRAINER[i])
                    {
                        skillLineName = SKILL_LINE_NAMES[SKILL_LINES_TRAINER[i]];
                    }
                }

                spell.SkillLine = skillLineName;

                spellList.Add(spell);
            }

            List<string> spellLines = new List<string>();

            spellLines.Add("ID,TRAIN ID,NAME,RANK,PREV SPELL,FIRST SPELL,LEVEL,COST,CLASS,CLASS MASK,TRAINER ID,SKILL,SKILL ID");

            PopulateSpellInfo(ref spellList, TrainerType.CLASS);
            PopulateClassInfo(ref spellList);
            PopulateTrainerSpellIDs(ref spellList, TrainerType.CLASS);
            ResolveChainInfo(ref spellList);

            foreach(SpellInfo spell in spellList)
            {
                //spellLines[iterator] = $"SpellID: {spell.ID}, Name: {spell.Name}, Rank: {spell.Rank}, Level: {spell.BaseLevel}, Skill: {spell.SkillLine}";
                spellLines.Add($"{spell.ID},{spell.TrainSpellID},{spell.Name},{spell.RankText},{spell.PrevSpellInChain},{spell.FirstSpellInChain},{spell.BaseLevel},{spell.CopperCost},{spell.Class},{spell.ClassMask},{spell.TrainerID},{spell.SkillLine},{spell.SkillLineID}");
            }

            WriteToLogFile(spellLines, "spell_info.csv");

            GenerateSQLFiles(spellList, TrainerType.CLASS);
        }

        private void button_generateTalentSpells_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> SKILL_LINE_NAMES = GetSkillLineNames(SKILL_LINES_TALENT);
            List<KeyValuePair<int, int>> SKILL_LINE_ABILITIES = GetSkillLineAbilities(SKILL_LINES_TALENT);
            List<SpellInfo> talentList = new List<SpellInfo>();

            foreach (KeyValuePair<int, int> pair in SKILL_LINE_ABILITIES)
            {
                SpellInfo spell = new SpellInfo();

                spell.SkillLineID = pair.Key;
                spell.ID = pair.Value;

                string skillLineName = "";

                for (int i = 0; i < SKILL_LINES_TALENT.Length; i++)
                {
                    if (pair.Key == SKILL_LINES_TALENT[i])
                    {
                        skillLineName = SKILL_LINE_NAMES[SKILL_LINES_TALENT[i]];
                    }
                }

                spell.SkillLine = skillLineName;

                talentList.Add(spell);
            }

            List<string> spellLines = new List<string>();

            spellLines.Add("ID,TRAIN ID,NAME,RANK,PREV SPELL,FIRST SPELL,LEVEL,COST,CLASS,CLASS MASK,TRAINER ID,SKILL,SKILL ID");

            PopulateSpellInfo(ref talentList, TrainerType.TALENT);
            PopulateTrainerSpellIDs(ref talentList, TrainerType.TALENT);
            ResolveChainInfo(ref talentList);

            foreach (SpellInfo spell in talentList)
            {
                if (spell.Name.Contains("Cat"))
                {
                    spell.TrainSpellID = m_catsAgilityTrainerSpells[spell.RankText];
                }

                //spellLines[iterator] = $"SpellID: {spell.ID}, Name: {spell.Name}, Rank: {spell.Rank}, Level: {spell.BaseLevel}, Skill: {spell.SkillLine}";
                spellLines.Add($"{spell.ID},{spell.TrainSpellID},{spell.Name},{spell.RankText},{spell.PrevSpellInChain},{spell.FirstSpellInChain},{spell.BaseLevel},{spell.CopperCost},{spell.Class},{spell.ClassMask},{spell.TrainerID},{spell.SkillLine},{spell.SkillLineID}");
            }

            WriteToLogFile(spellLines, "spell_info_talent.csv");

            GenerateSQLFiles(talentList, TrainerType.TALENT);
        }

        private void button_generateProfessionSpells_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> SKILL_LINE_NAMES = GetSkillLineNames(SKILL_LINES_PROFESSION);
            List<KeyValuePair<int, int>> SKILL_LINE_ABILITIES = GetSkillLineAbilities(SKILL_LINES_PROFESSION);
            List<SpellInfo> professionList = new List<SpellInfo>();

            foreach (KeyValuePair<int, int> pair in SKILL_LINE_ABILITIES)
            {
                SpellInfo spell = new SpellInfo();

                spell.SkillLineID = pair.Key;
                spell.ID = pair.Value;

                string skillLineName = "";

                for (int i = 0; i < SKILL_LINES_PROFESSION.Length; i++)
                {
                    if (pair.Key == SKILL_LINES_PROFESSION[i])
                    {
                        skillLineName = SKILL_LINE_NAMES[SKILL_LINES_PROFESSION[i]];
                    }
                }

                spell.SkillLine = skillLineName;

                professionList.Add(spell);
            }

            List<string> spellLines = new List<string>();

            spellLines.Add("ID,TRAIN ID,NAME,RANK,PREV SPELL,FIRST SPELL,LEVEL,SKILL,SKILL ID");

            PopulateSpellInfo(ref professionList, TrainerType.PROFESSION);
            PopulateTrainerSpellIDs(ref professionList, TrainerType.PROFESSION);
            ResolveChainInfo(ref professionList);

            foreach (SpellInfo spell in professionList)
            {
                spellLines.Add($"{spell.ID},{spell.TrainSpellID},{spell.Name},{spell.RankText},{spell.PrevSpellInChain},{spell.FirstSpellInChain},{spell.BaseLevel},{spell.SkillLine},{spell.SkillLineID}");
            }

            WriteToLogFile(spellLines, "spell_info_profession.csv");

            GenerateSQLFiles(professionList, TrainerType.PROFESSION);
        }

        private void GenerateSQLFiles(List<SpellInfo> spellList, TrainerType trainerType)
        {
            if (trainerType == TrainerType.CLASS)
            {
                List<string> lowLvlNpcTrainerLines = new List<string>();
                List<string> npcTrainerLines = new List<string>();
                List<string> spellChainLines = new List<string>();

                List<string> druidTrainerLines = new List<string>();
                List<string> lowLvlDruidTrainerLines = new List<string>();
                List<string> druidSpellChainLines = new List<string>();

                List<KeyValuePair<int, int>> alreadyUsedSpells = new List<KeyValuePair<int, int>>();

                foreach (SpellInfo spell in spellList)
                {
                    if (spell.TrainerID > 0 && spell.TrainSpellID > 0)
                    {
                        if (!alreadyUsedSpells.Contains(new KeyValuePair<int, int>(spell.TrainerID, spell.TrainSpellID)))
                        {
                            if (spell.TrainerID == 17 || spell.LowLvlTrainerID == 16)
                            {
                                druidTrainerLines.Add($"INSERT INTO `trainer_template` (`template_entry`, `spell`, `playerspell`, `spellcost`, `talentpointcost`, `skillpointcost`, `reqskill`, `reqskillvalue`, `reqlevel`) VALUES ({spell.TrainerID}, {spell.TrainSpellID}, {spell.ID}, {spell.CopperCost}, {spell.TalentCost}, {spell.SkillCost}, 0, 0, {spell.BaseLevel});");
                                druidSpellChainLines.Add($"INSERT INTO `spell_chain` (`spell_id`, `prev_spell`, `first_spell`, `rank`, `req_spell`) VALUES ({spell.ID}, {spell.PrevSpellInChain}, {spell.FirstSpellInChain}, {spell.Rank}, 0);");

                                if (spell.BaseLevel <= 6)
                                {
                                    lowLvlDruidTrainerLines.Add($"INSERT INTO `trainer_template` (`template_entry`, `spell`, `playerspell`, `spellcost`, `talentpointcost`, `skillpointcost`, `reqskill`, `reqskillvalue`, `reqlevel`) VALUES ({spell.LowLvlTrainerID}, {spell.TrainSpellID}, {spell.ID}, {spell.CopperCost}, {spell.TalentCost}, {spell.SkillCost}, 0, 0, {spell.BaseLevel});");
                                }

                                alreadyUsedSpells.Add(new KeyValuePair<int, int>(spell.TrainerID, spell.TrainSpellID));
                            }
                            else
                            {
                                npcTrainerLines.Add($"INSERT INTO `trainer_template` (`template_entry`, `spell`, `playerspell`, `spellcost`, `talentpointcost`, `skillpointcost`, `reqskill`, `reqskillvalue`, `reqlevel`) VALUES ({spell.TrainerID}, {spell.TrainSpellID}, {spell.ID}, {spell.CopperCost}, {spell.TalentCost}, {spell.SkillCost}, 0, 0, {spell.BaseLevel});");
                                spellChainLines.Add($"INSERT INTO `spell_chain` (`spell_id`, `prev_spell`, `first_spell`, `rank`, `req_spell`) VALUES ({spell.ID}, {spell.PrevSpellInChain}, {spell.FirstSpellInChain}, {spell.Rank}, 0);");

                                if (spell.BaseLevel <= 6)
                                {
                                    lowLvlNpcTrainerLines.Add($"INSERT INTO `trainer_template` (`template_entry`, `spell`, `playerspell`, `spellcost`, `talentpointcost`, `skillpointcost`, `reqskill`, `reqskillvalue`, `reqlevel`) VALUES ({spell.LowLvlTrainerID}, {spell.TrainSpellID}, {spell.ID}, {spell.CopperCost}, {spell.TalentCost}, {spell.SkillCost}, 0, 0, {spell.BaseLevel});");
                                }

                                alreadyUsedSpells.Add(new KeyValuePair<int, int>(spell.TrainerID, spell.TrainSpellID));
                            }
                        }
                    }
                }

                WriteToLogFile(druidTrainerLines, "trainer_template_inserts_druid.sql");
                WriteToLogFile(lowLvlDruidTrainerLines, "trainer_template_inserts_druid_lowlvl.sql");
                WriteToLogFile(druidSpellChainLines, "spell_chain_inserts_druid.sql");

                WriteToLogFile(npcTrainerLines, "trainer_template_inserts.sql");
                WriteToLogFile(lowLvlNpcTrainerLines, "trainer_template_inserts_lowlvl.sql");
                WriteToLogFile(spellChainLines, "spell_chain_inserts.sql");
            }
            else if (trainerType == TrainerType.TALENT)
            {
                List<string> npcTrainerLines = new List<string>();
                List<string> spellChainLines = new List<string>();

                List<KeyValuePair<int, int>> alreadyUsedSpells = new List<KeyValuePair<int, int>>();

                foreach (SpellInfo spell in spellList)
                {
                    if (spell.TrainSpellID > 0)
                    {
                        if (!alreadyUsedSpells.Contains(new KeyValuePair<int, int>(spell.TrainerID, spell.TrainSpellID)))
                        {
                            npcTrainerLines.Add($"REPLACE INTO `trainer_template` (`template_entry`, `spell`, `playerspell`, `spellcost`, `talentpointcost`, `skillpointcost`, `reqskill`, `reqskillvalue`, `reqlevel`) VALUES ({TALENT_TRAINER_ID}, {spell.TrainSpellID}, {spell.ID}, 0, {spell.TalentCost}, 0, {spell.RequiredSkill}, {(spell.RequiredSkill != 0 ? 1 : 0)}, 0);");
                            spellChainLines.Add($"REPLACE INTO `spell_chain` (`spell_id`, `prev_spell`, `first_spell`, `rank`, `req_spell`) VALUES ({spell.ID}, {spell.PrevSpellInChain}, {spell.FirstSpellInChain}, {spell.Rank}, 0);");

                            alreadyUsedSpells.Add(new KeyValuePair<int, int>(spell.TrainerID, spell.TrainSpellID));
                        }
                    }
                }

                WriteToLogFile(npcTrainerLines, "trainer_template_inserts_talent.sql");
                WriteToLogFile(spellChainLines, "spell_chain_inserts_talent.sql");
            }
            else if (trainerType == TrainerType.PROFESSION)
            {
                List<string> npcTrainerLines = new List<string>();
                List<string> spellChainLines = new List<string>();

                List<KeyValuePair<int, int>> alreadyUsedSpells = new List<KeyValuePair<int, int>>();

                foreach (SpellInfo spell in spellList)
                {
                    if (spell.TrainSpellID > 0)
                    {
                        if (!alreadyUsedSpells.Contains(new KeyValuePair<int, int>(spell.TrainerID, spell.TrainSpellID)))
                        {
                            npcTrainerLines.Add($"REPLACE INTO `trainer_template` (`template_entry`, `spell`, `playerspell`, `spellcost`, `talentpointcost`, `skillpointcost`, `reqskill`, `reqskillvalue`, `reqlevel`) VALUES (36, {spell.TrainSpellID}, {spell.ID}, 250, 0, 0, {spell.SkillLineID}, 10, 1);");
                            spellChainLines.Add($"REPLACE INTO `spell_chain` (`spell_id`, `prev_spell`, `first_spell`, `rank`, `req_spell`) VALUES ({spell.ID}, 0, {spell.ID}, 1, 555);");

                            alreadyUsedSpells.Add(new KeyValuePair<int, int>(spell.TrainerID, spell.TrainSpellID));
                        }
                    }
                }

                WriteToLogFile(npcTrainerLines, "trainer_template_inserts_profession.sql");
                WriteToLogFile(spellChainLines, "spell_chain_inserts_profession.sql");
            }
        }

        private Dictionary<int, string> GetSkillLineNames(int[] skillLines)
        {
            Dictionary<int, string> skillLineNames = new Dictionary<int, string>();

            string values = "(";

            for (int i = 0; i < skillLines.Length; i++)
            {
                if (i < skillLines.Length - 1)
                {
                    values += skillLines[i].ToString() + ", ";
                }
                else
                {
                    values += skillLines[i].ToString();
                }
            }

            values += ")";

            string sqlQuery = $"SELECT ID, DisplayName_enUS FROM skillline WHERE ID IN {values}";

            m_dbcConnector.SQLConnection.Open();

            try
            {
                var cmd = new MySqlCommand(sqlQuery, m_dbcConnector.SQLConnection);

                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    KeyValuePair<int, string> skillLinePair = new KeyValuePair<int, string>(-1, "");

                    if (!rdr.IsDBNull(0) && !rdr.IsDBNull(1))
                    {
                        skillLinePair = new KeyValuePair<int, string>(Convert.ToInt32(rdr.GetString(0)), rdr.GetString(1));
                    }

                    if (skillLinePair.Key != -1 && skillLinePair.Value != "")
                    {
                        skillLineNames.Add(skillLinePair.Key, skillLinePair.Value);
                    }
                }

                rdr.Close();
            }
            catch (Exception b)
            {
                MessageBox.Show(b.ToString());
            }

            m_dbcConnector.SQLConnection.Close();

            return skillLineNames;
        }

        private List<KeyValuePair<int, int>> GetSkillLineAbilities(int[] skillLines)
        {
            List<KeyValuePair<int, int>> skillLineAbilities = new List<KeyValuePair<int, int>>();

            string values = "(";

            for (int i = 0; i < skillLines.Length; i++)
            {
                if (i < skillLines.Length - 1)
                {
                    values += skillLines[i].ToString() + ", ";
                }
                else
                {
                    values += skillLines[i].ToString();
                }
            }

            values += ")";

            string sqlQuery = $"SELECT SkillLine, Spell, ClassMask FROM skilllineability WHERE SkillLine IN {values}";

            m_dbcConnector.SQLConnection.Open();

            try
            {
                var cmd = new MySqlCommand(sqlQuery, m_dbcConnector.SQLConnection);

                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    KeyValuePair<int, int> skillLinePair = new KeyValuePair<int, int>(-1, -1);

                    if (!rdr.IsDBNull(0) && !rdr.IsDBNull(1))
                    {
                        skillLinePair = new KeyValuePair<int, int>(Convert.ToInt32(rdr.GetString(0)), Convert.ToInt32(rdr.GetString(1)));
                    }

                    if (skillLinePair.Key != -1 && skillLinePair.Value != -1)
                    {
                        skillLineAbilities.Add(skillLinePair);
                    }
                }

                rdr.Close();
            }
            catch (Exception b)
            {
                MessageBox.Show(b.ToString());
            }

            m_dbcConnector.SQLConnection.Close();

            return skillLineAbilities;
        }

        // In the 2003 game guide, spell prices do for the most part match eachother's level to cost, however there are various exceptions. for now, they will all be equal
        // Odd numbers assumed, not validated (abilities are rarely able to be trained on odd # levels)
        private void PopulateLevelToPriceDict(ref Dictionary<int, float> levelToPriceDict)
        {
            levelToPriceDict.Add(0, 0);
            levelToPriceDict.Add(1, 10);
            levelToPriceDict.Add(2, 50);
            levelToPriceDict.Add(3, 60);
            levelToPriceDict.Add(4, 80); // 80 copper
            levelToPriceDict.Add(5, 90);
            levelToPriceDict.Add(6, 100); // 1s
            levelToPriceDict.Add(7, 150);
            levelToPriceDict.Add(8, 200); // 2s
            levelToPriceDict.Add(9, 250);
            levelToPriceDict.Add(10, 300); // 3s
            levelToPriceDict.Add(11, 500);
            levelToPriceDict.Add(12, 800); // 8s
            levelToPriceDict.Add(13, 1000);
            levelToPriceDict.Add(14, 1200); // 12s
            levelToPriceDict.Add(15, 1600);
            levelToPriceDict.Add(16, 1800); // 18s // different from gameguide
            levelToPriceDict.Add(17, 2000);
            levelToPriceDict.Add(18, 2200); // 20s // different from gameguide
            levelToPriceDict.Add(19, 2400);
            levelToPriceDict.Add(20, 2600); // 26s // different from gameguide
            levelToPriceDict.Add(21, 3000);
            levelToPriceDict.Add(22, 3400); // 34s
            levelToPriceDict.Add(23, 4400);
            levelToPriceDict.Add(24, 5300); // 53s
            levelToPriceDict.Add(25, 6000);
            levelToPriceDict.Add(26, 6700); // 67s
            levelToPriceDict.Add(27, 7400);
            levelToPriceDict.Add(28, 8100);
            levelToPriceDict.Add(29, 7900);
            levelToPriceDict.Add(30, 8000);
            levelToPriceDict.Add(32, 11000); // 1g10s
            levelToPriceDict.Add(34, 18000);
            levelToPriceDict.Add(35, 16000);
            levelToPriceDict.Add(36, 15000);
            levelToPriceDict.Add(38, 23000);
            levelToPriceDict.Add(40, 16000);
            levelToPriceDict.Add(42, 30000);
            levelToPriceDict.Add(44, 25000);
            levelToPriceDict.Add(45, 29000);
            levelToPriceDict.Add(46, 37000);
            levelToPriceDict.Add(48, 42000);
            levelToPriceDict.Add(50, 35000);
            levelToPriceDict.Add(52, 53000);
            levelToPriceDict.Add(54, 58000);
            levelToPriceDict.Add(55, 62000);
            levelToPriceDict.Add(56, 64000);
            levelToPriceDict.Add(58, 53000);
            levelToPriceDict.Add(60, 47000);
        }

        private void PopulateSpellInfo(ref List<SpellInfo> spellList, TrainerType trainerType)
        {
            if (trainerType == TrainerType.CLASS)
            {
                foreach (SpellInfo spell in spellList)
                {
                    string sqlQuery = $"SELECT BaseLevel, Name_enUS, NameSubtext_enUS FROM spell WHERE ID = {spell.ID}";

                    m_dbcConnector.SQLConnection.Open();

                    try
                    {
                        var cmd = new MySqlCommand(sqlQuery, m_dbcConnector.SQLConnection);

                        MySqlDataReader rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(0) && !rdr.IsDBNull(1) && !rdr.IsDBNull(2))
                            {
                                spell.BaseLevel = Convert.ToInt32(rdr.GetString(0));
                                spell.Name = rdr.GetString(1);
                                spell.RankText = rdr.GetString(2);
                            }
                        }

                        rdr.Close();
                    }
                    catch (Exception b)
                    {
                        MessageBox.Show(b.ToString());
                    }

                    if (spell.BaseLevel == 0)
                    {
                        spell.BaseLevel = 1;
                    }

                    if (spell.BaseLevel % 2 == 0 || spell.BaseLevel == 1)
                    {
                        spell.CopperCost = m_spellLevelToPrice[spell.BaseLevel];
                    }

                    m_dbcConnector.SQLConnection.Close();
                }
            }
            else if (trainerType == TrainerType.TALENT)
            {
                foreach (SpellInfo spell in spellList)
                {
                    string sqlQuery = $"SELECT Name_enUS, NameSubtext_enUS FROM spell WHERE ID = {spell.ID}";

                    m_dbcConnector.SQLConnection.Open();

                    try
                    {
                        var cmd = new MySqlCommand(sqlQuery, m_dbcConnector.SQLConnection);

                        MySqlDataReader rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(0) && !rdr.IsDBNull(1))
                            {
                                spell.Name = rdr.GetString(0);
                                spell.RankText = rdr.GetString(1);
                            }
                        }

                        rdr.Close();
                    }
                    catch (Exception b)
                    {
                        MessageBox.Show(b.ToString());
                    }

                    if (spell.SkillLineID == 222)
                    {
                        foreach (KeyValuePair<string, int> skill in m_skillNameToID)
                        {
                            if (spell.Name.Contains(skill.Key))
                            {
                                spell.RequiredSkill = m_skillNameToID[skill.Key];
                            }
                        }
                    }
                    else if (spell.SkillLineID == 233)
                    {
                        foreach (KeyValuePair<string, int> skill in m_skillNameToID)
                        {
                            if (spell.Name.Contains(skill.Key) && !spell.Name.Contains("Resist"))
                            {
                                spell.RequiredSkill = m_skillNameToID[skill.Key];
                            }
                        }
                    }

                    spell.TrainerID = TALENT_TRAINER_ID;

                    spell.TalentCost = 0; // this can be resolved within the emulator itself

                    m_dbcConnector.SQLConnection.Close();
                }
            }
            else if (trainerType == TrainerType.PROFESSION)
            {
                foreach (SpellInfo spell in spellList)
                {
                    string sqlQuery = $"SELECT Name_enUS, NameSubtext_enUS FROM spell WHERE ID = {spell.ID}";

                    m_dbcConnector.SQLConnection.Open();

                    try
                    {
                        var cmd = new MySqlCommand(sqlQuery, m_dbcConnector.SQLConnection);

                        MySqlDataReader rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(0) && !rdr.IsDBNull(1))
                            {
                                spell.Name = rdr.GetString(0);
                                spell.RankText = rdr.GetString(1);
                            }
                        }

                        rdr.Close();
                    }
                    catch (Exception b)
                    {
                        MessageBox.Show(b.ToString());
                    }

                    spell.TrainerID = 0;

                    spell.TalentCost = 0; // this can be resolved within the emulator itself

                    m_dbcConnector.SQLConnection.Close();
                }
            }
        }

        private void PopulateClassInfo(ref List<SpellInfo> spellList)
        {
            m_dbcConnector.SQLConnection.Open();

            foreach (SpellInfo spell in spellList)
            {
                string sqlQuery = $"SELECT ClassMask FROM skilllineability WHERE Spell = {spell.ID}";

                try
                {
                    var cmd = new MySqlCommand(sqlQuery, m_dbcConnector.SQLConnection);

                    MySqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        if (!rdr.IsDBNull(0))
                        {
                            spell.ClassMask = Convert.ToInt32(rdr.GetString(0));
                        }
                    }

                    rdr.Close();
                }
                catch (Exception b)
                {
                    MessageBox.Show(b.ToString());
                }

                switch (spell.ClassMask)
                {
                    case 0:
                        spell.Class = "None";
                        break;
                    case 1:
                        spell.Class = "Warrior";
                        break;
                    case 2:
                        spell.Class = "Paladin";
                        break;
                    case 4:
                        spell.Class = "Hunter";
                        break;
                    case 8:
                        spell.Class = "Rogue";
                        break;
                    case 16:
                        spell.Class = "Priest";
                        break;
                    case 64:
                        spell.Class = "Shaman";
                        break;
                    case 128:
                        spell.Class = "Mage";
                        break;
                    case 256:
                        spell.Class = "Warlock";
                        break;
                    case 1024:
                        spell.Class = "Druid";
                        break;

                    default:
                        spell.Class = "NONE/UNKNOWN";
                        break;
                }

                // Figure out max lvl trainer id
                switch (spell.Class)
                {
                    // Can't resolve these
                    case "None":
                    case "NONE/UNKNOWN":
                    default:
                        spell.TrainerID = 0;
                        break;

                    // Can resolve these
                    case "Warrior":
                        spell.TrainerID = 23;
                        spell.LowLvlTrainerID = 22;
                        break;
                    case "Paladin":
                        spell.TrainerID = 29;
                        spell.LowLvlTrainerID = 28;
                        break;
                    case "Hunter":
                        spell.TrainerID = 20;
                        spell.LowLvlTrainerID = 19;
                        break;
                    case "Rogue":
                        spell.TrainerID = 26;
                        spell.LowLvlTrainerID = 25;
                        break;
                    case "Priest":
                        spell.TrainerID = 8;
                        spell.LowLvlTrainerID = 7;
                        break;
                    case "Shaman":
                        spell.TrainerID = 10;
                        spell.LowLvlTrainerID = 11;
                        break;
                    case "Mage":
                        spell.TrainerID = 1;
                        spell.LowLvlTrainerID = 5;
                        break;
                    case "Warlock":
                        spell.TrainerID = 14;
                        spell.LowLvlTrainerID = 13;
                        break;
                    case "Druid":
                        spell.TrainerID = 17;
                        spell.LowLvlTrainerID = 16;
                        break;
                }
            }

            m_dbcConnector.SQLConnection.Close();
        }

        private void PopulateTrainerSpellIDs(ref List<SpellInfo> spellList, TrainerType trainerType)
        {
            if (trainerType == TrainerType.CLASS || trainerType == TrainerType.PROFESSION)
            {
                foreach (SpellInfo spell in spellList)
                {
                    // SELECT * FROM spell WHERE Description_enUS = "" AND Name_enUS = "Frostbolt" AND NameSubtext_enUS = "Rank 4" AND EffectTriggerSpell_1 = 7322 OR EffectTriggerSpell_2 = 7322 OR EffectTriggerSpell_3 = 7322
                    string sqlQuery = $"SELECT ID FROM spell WHERE (Effect_1 = 36 AND EffectTriggerSpell_1 = {spell.ID}) OR (Effect_2 = 36 AND EffectTriggerSpell_2 = {spell.ID}) OR (Effect_3 = 36 AND EffectTriggerSpell_3 = {spell.ID})";

                    m_dbcConnector.SQLConnection.Open();

                    try
                    {
                        var cmd = new MySqlCommand(sqlQuery, m_dbcConnector.SQLConnection);

                        MySqlDataReader rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(0))
                            {
                                spell.TrainSpellID = rdr.GetInt32(0);
                            }
                        }

                        rdr.Close();
                    }
                    catch (Exception b)
                    {
                        MessageBox.Show(b.ToString());
                    }

                    m_dbcConnector.SQLConnection.Close();
                }
            }
            else if (trainerType == TrainerType.TALENT)
            {
                foreach (SpellInfo spell in spellList)
                {
                    string sqlQuery = $"SELECT ID FROM spell WHERE Name_enUS = \"{spell.Name}\" AND NameSubtext_enUS = \"{spell.RankText}\" AND Effect_1 = 36 AND EffectTriggerSpell_1 = {spell.ID} OR EffectTriggerSpell_2 = {spell.ID} OR EffectTriggerSpell_3 = {spell.ID}";

                    m_dbcConnector.SQLConnection.Open();

                    try
                    {
                        var cmd = new MySqlCommand(sqlQuery, m_dbcConnector.SQLConnection);

                        MySqlDataReader rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(0))
                            {
                                spell.TrainSpellID = rdr.GetInt32(0);
                            }
                        }

                        rdr.Close();
                    }
                    catch (Exception b)
                    {
                        MessageBox.Show(b.ToString());
                    }

                    m_dbcConnector.SQLConnection.Close();
                }
            }
        }

        private void ResolveChainInfo(ref List<SpellInfo> spellList)
        {
            foreach (SpellInfo spell in spellList)
            {
                int myRank = GetRankNumFromRank(spell.RankText);

                if (myRank == 0)
                {
                    myRank = 1;
                }

                spell.Rank = myRank;

                if (myRank == 1)
                {
                    spell.FirstSpellInChain = spell.ID;
                    spell.PrevSpellInChain = 0;
                }
                else if (myRank > 1)
                {
                    foreach (SpellInfo spell2 in spellList)
                    {
                        int theirRank = GetRankNumFromRank(spell2.RankText);

                        if (theirRank == myRank - 1 && spell2.Name == spell.Name)
                        {
                            spell.PrevSpellInChain = spell2.ID;

                            if (theirRank == 1)
                            {
                                spell.FirstSpellInChain = spell2.ID;
                            }
                            else if (theirRank > 1)
                            {
                                spell.FirstSpellInChain = spell2.FirstSpellInChain;
                            }
                        }
                    }
                }
            }
        }

        private int GetRankNumFromRank(string rankText)
        {
            int myRank = 0;

            if (rankText.Contains("Rank"))
            {
                myRank = Int32.Parse(rankText.Last().ToString());
            }

            return myRank;
        }

        private async Task WriteToLogFile(List<string> lines, string fileName)
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\AlphaProject\\SpellGen";

            // Write the specified text asynchronously to a new file named "WriteTextAsync.txt".
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, fileName)))
            {
                foreach(string line in lines)
                {
                    await outputFile.WriteLineAsync(line); 
                }
            }
        }
    }
}
