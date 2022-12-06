using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

namespace TrainerSpellDataDBCEntry
{
    public partial class MainForm : Form
    {
        Logger m_logger;
        DBConnector m_dbcConnector;
        DBConnector m_worldConnector;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*listViewHistory.Scrollable = true;
            listViewHistory.View = View.Details;

            ColumnHeader header = new ColumnHeader();
            header.Text = "History";
            header.Name = "mainCol";
            header.Width = listViewHistory.Width;
            header.TextAlign = HorizontalAlignment.Left;
            listViewHistory.Columns.Add(header);*/

            m_logger = new Logger(listBoxHistory);
        }

        private void buttonConnectDB_Click(object sender, EventArgs e)
        {
            m_worldConnector = new DBConnector("localhost", "alpha_world", "root", "root");
            m_dbcConnector = new DBConnector("localhost", "alpha_dbc", "root", "root");

            if (m_worldConnector.Start() == 0 && m_dbcConnector.Start() == 0)
            {
                MessageBox.Show("Connection with database established.");

                m_worldConnector.SQLConnection.Close();
                m_dbcConnector.SQLConnection.Close();
            }
            else
            {
                MessageBox.Show("Connection could not be established.");
            }
        }

        private void buttonSearchSpell_Click(object sender, EventArgs e)
        {
            m_dbcConnector.SQLConnection.Open();

            if (m_dbcConnector == null || m_dbcConnector.SQLConnection.State != ConnectionState.Open)
            {
                MessageBox.Show("No connection to alpha_dbc open, cannot search for spells.");
                return;
            }

            textBoxResultSpellName.Text = "";
            textBoxResultSpellRank.Text = "";

            string sql = $"SELECT Name_enUS, NameSubtext_enUS, Description_enUS FROM spell WHERE ID = {textBoxSearchSpellID.Text}";

            try
            {
                var cmd = new MySqlCommand(sql, m_dbcConnector.SQLConnection);

                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(0))
                    {
                        textBoxResultSpellName.Text = rdr.GetString(0);
                    }

                    if (!rdr.IsDBNull(1))
                    {
                        textBoxResultSpellRank.Text = rdr.GetString(1);
                    }

                    if (!rdr.IsDBNull(2))
                    {
                        richTextBox1.Text = rdr.GetString(2);
                    }
                }

                rdr.Close();

                m_logger.Log($"[SEARCH] [Spell]: {textBoxSearchSpellID.Text}, Name: {textBoxResultSpellName.Text}, Rank: {textBoxResultSpellRank.Text}", Color.Yellow);
            }
            catch (Exception b)
            {
                MessageBox.Show(b.ToString());
            }

            m_dbcConnector.SQLConnection.Close();
        }

        private void buttonSearchNPC_Click(object sender, EventArgs e)
        {
            m_worldConnector.SQLConnection.Open();

            if (m_worldConnector == null || m_worldConnector.SQLConnection.State != ConnectionState.Open)
            {
                MessageBox.Show("No connection to alpha_world open, cannot search for npcs.");
                return;
            }

            textBoxResultNPCName.Text = "";
            textBoxResultNPCSubname.Text = "";

            string sql = $"SELECT name, subname, trainer_id, level_max FROM creature_template WHERE entry = {textBoxSearchNPCEntryID.Text}";

            try
            {
                var cmd = new MySqlCommand(sql, m_worldConnector.SQLConnection);

                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(0))
                    {
                        textBoxResultNPCName.Text = rdr.GetString(0);
                    }

                    if (!rdr.IsDBNull(1))
                    {
                        textBoxResultNPCSubname.Text = rdr.GetString(1);
                    }

                    if (!rdr.IsDBNull(2))
                    {
                        textBoxTrainerId.Text = rdr.GetString(2);
                    }

                    if (!rdr.IsDBNull(3))
                    {
                        textBoxTrainerLevel.Text = rdr.GetString(3);
                    }
                }

                rdr.Close();

                m_logger.Log($"[SEARCH] [NPC]: {textBoxSearchNPCEntryID.Text}, Name: {textBoxResultNPCName.Text}, Subname: {textBoxResultNPCSubname.Text}, Level: {textBoxTrainerLevel.Text}, Trainer ID: {textBoxTrainerId.Text}", Color.Yellow);
            }
            catch (Exception b)
            {
                MessageBox.Show(b.ToString());
            }

            m_worldConnector.SQLConnection.Close();
        }

        private void buttonInsertIntoDB_Click(object sender, EventArgs e)
        {
            if (textBoxTrainerTemplateID.Text == "")
                textBoxTrainerTemplateID.Text = "0";

            if (textBoxSpellID.Text == "")
                textBoxSpellID.Text = "0";

            if (textBoxSpellCost.Text == "")
                textBoxSpellCost.Text = "0";

            if (textBoxTalentPointCost.Text == "")
                textBoxTalentPointCost.Text = "0";

            if (textBoxSkillPointCost.Text == "")
                textBoxSkillPointCost.Text = "0";

            if (textBoxReqSkillID.Text == "")
                textBoxReqSkillID.Text = "0";

            if (textBoxReqSkillVal.Text == "")
                textBoxReqSkillVal.Text = "0";

            if (textBoxReqLevel.Text == "")
                textBoxReqLevel.Text = "0";

            if (textBoxSpellID.Text == "")
                textBoxSpellID.Text = "0";

            if (textBoxPrevSpellID.Text == "")
                textBoxPrevSpellID.Text = "0";

            if (textBoxFirstSpellID.Text == "")
                textBoxFirstSpellID.Text = "0";

            if (textBoxSpellRank.Text == "")
                textBoxSpellRank.Text = "0";

            if (textBoxReqSpell.Text == "")
                textBoxReqSpell.Text = "0";

            bool trainerInsertResult = SQLInsertNpcTrainer(textBoxTrainerTemplateID.Text, textBoxSpellID.Text, textBoxSpellCost.Text, textBoxTalentPointCost.Text, textBoxSkillPointCost.Text, textBoxReqSkillID.Text, textBoxReqSkillVal.Text, textBoxReqLevel.Text);
            bool spellChainInsertResult = SQLInsertSpellChain(textBoxSpellID.Text, textBoxPrevSpellID.Text, textBoxFirstSpellID.Text, textBoxSpellRank.Text, textBoxReqSpell.Text);
        }

        private void buttonInsertTrainerOnly_Click(object sender, EventArgs e)
        {
            if (textBoxTrainerTemplateID.Text == "")
                textBoxTrainerTemplateID.Text = "0";

            if (textBoxSpellID.Text == "")
                textBoxSpellID.Text = "0";

            if (textBoxSpellCost.Text == "")
                textBoxSpellCost.Text = "0";

            if (textBoxTalentPointCost.Text == "")
                textBoxTalentPointCost.Text = "0";

            if (textBoxSkillPointCost.Text == "")
                textBoxSkillPointCost.Text = "0";

            if (textBoxReqSkillID.Text == "")
                textBoxReqSkillID.Text = "0";

            if (textBoxReqSkillVal.Text == "")
                textBoxReqSkillVal.Text = "0";

            if (textBoxReqLevel.Text == "")
                textBoxReqLevel.Text = "0";

            bool trainerInsertResult = SQLInsertNpcTrainer(textBoxTrainerTemplateID.Text, textBoxSpellID.Text, textBoxSpellCost.Text, textBoxTalentPointCost.Text, textBoxSkillPointCost.Text, textBoxReqSkillID.Text, textBoxReqSkillVal.Text, textBoxReqLevel.Text);
        }

        private void buttonInsertChainOnly_Click(object sender, EventArgs e)
        {
            if (textBoxSpellID.Text == "")
                textBoxSpellID.Text = "0";

            if (textBoxPrevSpellID.Text == "")
                textBoxPrevSpellID.Text = "0";

            if (textBoxFirstSpellID.Text == "")
                textBoxFirstSpellID.Text = "0";

            if (textBoxSpellRank.Text == "")
                textBoxSpellRank.Text = "0";

            if (textBoxReqSpell.Text == "")
                textBoxReqSpell.Text = "0";

            bool spellChainInsertResult = SQLInsertSpellChain(textBoxSpellID.Text, textBoxPrevSpellID.Text, textBoxFirstSpellID.Text, textBoxSpellRank.Text, textBoxReqSpell.Text);
        }

        private bool SQLInsertNpcTrainer(string templateId, string spellId, string spellCost, string talentCost, string skillCost, string reqSkillId, string reqSkillVal, string reqLevel)
        {
            bool success = true;

            m_worldConnector.SQLConnection.Open();

            try
            {
                string cmdString = $"INSERT INTO training_info Values('{templateId}', '{spellId}', '{spellCost}', '{talentCost}', '{skillCost}', '{reqSkillId}', '{reqSkillVal}', '{reqLevel}')";

                DialogResult dr = MessageBox.Show($"Insert trainer spell {textBoxSpellID.Text} into training_info?",
                      "SQL Insert Confirmation", MessageBoxButtons.YesNo);

                switch (dr)
                {
                    case DialogResult.Yes:

                        MySqlCommand trainerCmd = new MySqlCommand(cmdString, m_worldConnector.SQLConnection);
                        trainerCmd.ExecuteNonQuery();

                        MessageBox.Show($"Inserted spell {textBoxSpellID.Text} into training_info.");

                        break;
                    case DialogResult.No:
                        break;
                }

            }
            catch (Exception b)
            {
                MessageBox.Show(b.ToString());

                success = false;
            }

            m_worldConnector.SQLConnection.Close();

            string trainerInsertLogMsg = "";

            if (success)
            {
                trainerInsertLogMsg = $"[INSERT] [training_info] [SUCCEEDED]: Spell {textBoxSpellID.Text} inserted into training_info for trainer template {textBoxTrainerTemplateID.Text}.";
                m_logger.Log(trainerInsertLogMsg, Color.Green);
            }
            else
            {
                trainerInsertLogMsg = $"[INSERT] [training_info] [FAILED]: Spell {textBoxSpellID.Text} not inserted into training_info for trainer template {textBoxTrainerTemplateID.Text}.";
                m_logger.Log(trainerInsertLogMsg, Color.Red);
            }

            return success;
        }

        private bool SQLInsertSpellChain(string spellId, string prevSpellId, string firstSpellId, string spellRank, string reqSpell)
        {
            bool success = true;

            m_worldConnector.SQLConnection.Open();

            try
            {

                string cmdString = $"INSERT INTO spell_chain Values('{spellId}', '{prevSpellId}', '{firstSpellId}', '{spellRank}', '{reqSpell}')";

                DialogResult dr = MessageBox.Show($"Insert spell chain {textBoxSpellID.Text} into spell_chain?",
                      "SQL Insert Confirmation", MessageBoxButtons.YesNo);

                switch (dr)
                {
                    case DialogResult.Yes:

                        MySqlCommand chainCmd = new MySqlCommand(cmdString, m_worldConnector.SQLConnection);
                        chainCmd.ExecuteNonQuery();

                        MessageBox.Show($"Inserted spell {textBoxSpellID.Text} into spell_chain.");

                        break;
                    case DialogResult.No:
                        break;
                }
            }
            catch (Exception b)
            {
                MessageBox.Show(b.ToString());

                success = false;
            }

            m_worldConnector.SQLConnection.Close();

            string spellChainInsertLogMsg = "";

            if (success)
            {
                spellChainInsertLogMsg = $"[INSERT] [spell_chain] [SUCCEEDED]: Rank {textBoxSpellRank.Text} spell {textBoxSpellID.Text} inserted into spell_chain.";
                m_logger.Log(spellChainInsertLogMsg, Color.Green);
            }
            else
            {
                spellChainInsertLogMsg = $"[INSERT] [spell_chain] [FAILED]: Rank {textBoxSpellRank.Text} spell {textBoxSpellID.Text} not inserted into spell_chain.";
                m_logger.Log(spellChainInsertLogMsg, Color.Red);
            }

            return success;
        }

        private void buttonOpenSpellGenForm_Click(object sender, EventArgs e)
        {
            DBGenerator dbGenForm = new DBGenerator();
            dbGenForm.Show();
        }
    }
}
