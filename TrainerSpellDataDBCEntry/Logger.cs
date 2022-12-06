using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace TrainerSpellDataDBCEntry
{
    class Logger
    {
        private ListBox m_logBox;

        public Logger(ListBox logBox)
        {
            m_logBox = logBox;
        }

        public void Log(string logMsg, Color logColor)
        {
            m_logBox.Items.Insert(0, logMsg);
            //m_logBox.Items[0].BackColor = logColor;
        }
    }
}
