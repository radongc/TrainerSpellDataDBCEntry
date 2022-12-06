
namespace TrainerSpellDataDBCEntry
{
    partial class DBGenerator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonGenerateSpellInfo = new System.Windows.Forms.Button();
            this.button_generateTalentSpells = new System.Windows.Forms.Button();
            this.button_generateProfessionSpells = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonGenerateSpellInfo
            // 
            this.buttonGenerateSpellInfo.Location = new System.Drawing.Point(11, 12);
            this.buttonGenerateSpellInfo.Name = "buttonGenerateSpellInfo";
            this.buttonGenerateSpellInfo.Size = new System.Drawing.Size(196, 45);
            this.buttonGenerateSpellInfo.TabIndex = 0;
            this.buttonGenerateSpellInfo.Text = "Generate Trainer Spells";
            this.buttonGenerateSpellInfo.UseVisualStyleBackColor = true;
            this.buttonGenerateSpellInfo.Click += new System.EventHandler(this.buttonGenerateSpellInfo_Click);
            // 
            // button_generateTalentSpells
            // 
            this.button_generateTalentSpells.Location = new System.Drawing.Point(11, 63);
            this.button_generateTalentSpells.Name = "button_generateTalentSpells";
            this.button_generateTalentSpells.Size = new System.Drawing.Size(196, 45);
            this.button_generateTalentSpells.TabIndex = 1;
            this.button_generateTalentSpells.Text = "Generate Talent Spells";
            this.button_generateTalentSpells.UseVisualStyleBackColor = true;
            this.button_generateTalentSpells.Click += new System.EventHandler(this.button_generateTalentSpells_Click);
            // 
            // button_generateProfessionSpells
            // 
            this.button_generateProfessionSpells.Location = new System.Drawing.Point(11, 114);
            this.button_generateProfessionSpells.Name = "button_generateProfessionSpells";
            this.button_generateProfessionSpells.Size = new System.Drawing.Size(196, 45);
            this.button_generateProfessionSpells.TabIndex = 2;
            this.button_generateProfessionSpells.Text = "Generate Profession Spells";
            this.button_generateProfessionSpells.UseVisualStyleBackColor = true;
            this.button_generateProfessionSpells.Click += new System.EventHandler(this.button_generateProfessionSpells_Click);
            // 
            // DBGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(219, 252);
            this.Controls.Add(this.button_generateProfessionSpells);
            this.Controls.Add(this.button_generateTalentSpells);
            this.Controls.Add(this.buttonGenerateSpellInfo);
            this.Name = "DBGenerator";
            this.Text = "DBGenerator";
            this.Load += new System.EventHandler(this.DBGenerator_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonGenerateSpellInfo;
        private System.Windows.Forms.Button button_generateTalentSpells;
        private System.Windows.Forms.Button button_generateProfessionSpells;
    }
}