using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Data;

namespace ABC
{
    
    public class DBCon
    {
        private MySqlConnection conn = null;

        bool link()
        {
            //链接数据库
            string MySQLString = "server=localhost; user=root; password=; database=network;";
            this.conn = new MySqlConnection(MySQLString);
            try
            {
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("链接数据库不成功");
                return false;
            }
        }

        public void insert(string str)
        {
            if (link())
            {
                //string InsertSql = "insert into pedestrian Values(" + PedID + "," + GroupID + "," + FrameNo + "," + Xcoordination + "," + Ycoordination + ",'" + Behavior + "')";
                MySqlCommand InsertCommand = new MySqlCommand(str, this.conn);
                InsertCommand.ExecuteNonQuery();
            }
         }

        public DataTable select(string str)
        {
            MySqlDataAdapter sda = null;
            DataTable dt = null;
            if (link())
            {
                sda = new MySqlDataAdapter(str, this.conn);
                dt = new DataTable();
                sda.Fill(dt);
            }
            return dt;
        }
        }
  }
