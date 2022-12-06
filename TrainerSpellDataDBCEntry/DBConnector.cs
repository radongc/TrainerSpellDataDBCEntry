using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TrainerSpellDataDBCEntry
{
    class DBConnector
    {
        private string m_connectionString = "";

        public MySqlConnection SQLConnection { get; private set; }

        public DBConnector(string host, string db, string uid, string pwd)
        {
            m_connectionString = $"Server={host};Database={db};uid={uid};pwd={pwd}";
        }

        public int Start()
        {
            if (m_connectionString == "")
            {
                return -1;
            }
            else
            {
                SQLConnection = new MySqlConnection(m_connectionString);

                SQLConnection.Open();

                if (SQLConnection.State == ConnectionState.Open)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}
