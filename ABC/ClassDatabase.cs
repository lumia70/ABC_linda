using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections;

namespace ABC
{
    public class Database
    {
        /*数据库操作的类
         *数据库连接
         *调用方法
         *SQLClass db = new SQLClass();
          string sql = "@要执行的SQL语句";
          int i = db.Insert(sql);
          if (i > 0)
          {
             MessageBox.Show("插入数据成功");
          }
         * 
         * 
         */
        public MySqlConnection getconn()
        {
            string myStrSqlconn = "server=localhost;user=root; password=; database=network;";
            MySqlConnection myconn = new MySqlConnection(myStrSqlconn);
            return myconn;
        }
    }
    public class SQLchan:Database
    {
        public int Insert(string sql)
        {
            MySqlConnection conn = null;
            MySqlCommand cmd = null;
            try
            {
                conn = this.getconn();
                conn.Open();
                cmd = new MySqlCommand(sql, conn);
                int i = cmd.ExecuteNonQuery();
                conn.Close();
                return i;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable Select(string sql)
        {
            MySqlConnection mysqlconn = null;
            MySqlDataAdapter sda = null;
            DataTable dt = null;
            try
            {
                mysqlconn = this.getconn();
                sda = new MySqlDataAdapter(sql, mysqlconn);
                dt = new DataTable();
                sda.Fill(dt);  //trap here
                return dt;
            }
            catch (Exception)
            {

                throw;
            }

        }
        
        public MyData StoreInMemory()
        {
            MyData myData = new MyData(); 
            //Store link table info;
            DataTable linkDataInfo = Select("select * from link");
            for(int i = 0; i < linkDataInfo.Rows.Count; i++)
            {
                myLinkElement tmpMyLinkElement = new myLinkElement();
                tmpMyLinkElement.linkId = int.Parse(linkDataInfo.Rows[i][0].ToString());
                tmpMyLinkElement.fromNode = int.Parse(linkDataInfo.Rows[i][1].ToString());
                tmpMyLinkElement.toNode = int.Parse(linkDataInfo.Rows[i][2].ToString());
                tmpMyLinkElement.length = double.Parse(linkDataInfo.Rows[i][3].ToString());
                tmpMyLinkElement.coverage = double.Parse(linkDataInfo.Rows[i][4].ToString());
                tmpMyLinkElement.flowindex = double.Parse(linkDataInfo.Rows[i][5].ToString());
                myData.mMyLinkTable.Add(tmpMyLinkElement);
            }

            //Store node table info
            DataTable nodeDataInfo = Select("select * from node");
            for(int i = 0; i < nodeDataInfo.Rows.Count; i++)
            {
                myNodeElement tmpMyNodeElement = new myNodeElement();
                tmpMyNodeElement.nodeId = int.Parse(nodeDataInfo.Rows[i][0].ToString());
                tmpMyNodeElement.pointX = double.Parse(nodeDataInfo.Rows[i][1].ToString());
                tmpMyNodeElement.pointY = double.Parse(nodeDataInfo.Rows[i][2].ToString());
                tmpMyNodeElement.linkNum = int.Parse(nodeDataInfo.Rows[i][3].ToString());
                string str = nodeDataInfo.Rows[i][4].ToString();
                for(int j = 1; j <= tmpMyNodeElement.linkNum; j++)
                {
                    tmpMyNodeElement.linkIdsList.Add(int.Parse(str.Substring((j - 1) * 6, 4)));
                }
                myData.mMyNodeTable.Add(tmpMyNodeElement);
            }
            return myData;    
        }

        public class MyData   //Our data struct to store db data in memory
        { 
            public List<myLinkElement> mMyLinkTable = new List<myLinkElement>();
            public List<myNodeElement> mMyNodeTable = new List<myNodeElement>();
            //public int num;
            //public int[,] path = new int[200, 160];
            //public double[] length = new double[200];

            //public double[] coverage = new double[200];
            //public double[] flowindex = new double[200];
            //public int[] pathnum = new int[200];
        }

        public class myLinkElement
        {
            public int linkId;
            public int fromNode;
            public int toNode;
            public double length;
            public double coverage;
            public double flowindex;
        }

        public class myNodeElement
        {
            public int nodeId;
            public double pointX;
            public double pointY;
            public int linkNum;
            public ArrayList linkIdsList = new ArrayList();
        }
    }
}
