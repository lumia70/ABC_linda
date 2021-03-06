﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using ABC;
using System.Threading.Tasks;
using System.Collections;

namespace ABC
{
    public partial class Form1 : Form
    {
        //———————————————————————[参数]
        pathway allpath = new pathway(); //所有路径的集合
      
        int[] arra = new int[180];         //当前处理的路径
        int[] way=new int[180];            //当前结点所连接的弧段id
        int[] node = new int[180];         //空余可以到达的节点
        bool suc = true;                  //是否成功找到路径

        double length=0,coverage=0,flowindex=0;
        int n=0; //
       
        
        int[] leader = new int[100];       //蜂群
        int[] leaderlimit = new int[100];
        int[] follower = new int[100];
        int[] sconter = new int[100];
        int leadernum=0,followernum=0,sconternum = 0;  //领导、跟随、侦查蜂个数

        SQLchan.MyData dbData;      //An instance of MyData to store db data
        int totalCount = 0;
        //———————————————————————
        public Form1()
        {
            InitializeComponent();
            allpath.num = 0;
        }
        //-------------------------------------------------------------------
        void clearfollower(int SN) //初始化跟随蜂
        {
            for (int i = 1; i <= SN; i++)
                follower[i] = -1;
        }
        void clearpath() //清空当前路径参数
        {
            for (int i = 0; i <= 160; i++)
                arra[i] = -1;
            length = 0;
            coverage = 0; 
            flowindex = 0;
            n = 0; 
        }
        void transferpath(int lbee) //将第n个allpath（leaderbee）的信息传输到当前处理数组中,返回最后一个节点
        {
            length = allpath.length[lbee];
            coverage = allpath.coverage[lbee];
            flowindex = allpath.flowindex[lbee];
            n = allpath.pathnum[lbee];

            for (int i = 0; i <= 160; i++)
                arra[i] = -1;
            for (int i = 1; i <= n; i++)
                arra[allpath.path[lbee,i]] = i;
        }
        void savepath(int i) //将当前数据存储在第i个allpath中
        {
            allpath.pathnum[i] = n;
            allpath.length[i] = length;
            allpath.coverage[i] = coverage;
            allpath.flowindex[i] = flowindex;
            for (int j = 0; j <= 160; j++)
                if (arra[j] != -1) allpath.path[i, arra[j]] = j;

        }
        double max(double a, double b)
        {
            if (a > b) return a;
            else return b;
        }
        double min(double a, double b)
        {
            if (a > b) return b;
            else return a;
        }
        void show()  //显示优解路径
        {
            FileStream fs = new FileStream("C:\\Users\\Lee\\Desktop\\result.txt", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            listBox1.Items.Clear();
            string str="";
            /*bool[] get=new bool[100];
            double maxl = allpath.length[1], minc = allpath.coverage[1], minf = allpath.flowindex[1];
            for (int i = 2; i <= allpath.num; i++)
                if (allpath.length[i] > maxl && allpath.coverage[i] < minc && allpath.flowindex[i] < minf)
                    get[i] = false;
                else
                {
                    get[i] = true;
                    maxl = max(maxl, allpath.length[i]);
                    minc = min(minc, allpath.coverage[i]);
                    minf = min(minf, allpath.flowindex[i]);
                }*/
            for (int i = 1; i <= allpath.num; i++)
            {
                bool show = true;
                for (int j = i + 1; j <= allpath.num; j++)
                {
                    if ((allpath.length[i] >= allpath.length[j]) && ((allpath.coverage[i] / allpath.length[i]) <= (allpath.coverage[j] / allpath.length[j])) && (allpath.flowindex[i] <= allpath.flowindex[j]))
                    { 
                        show = false;
                        break;
                    }
                }
                if (show)
                {
                    str = str + allpath.length[i] + " " + allpath.coverage[i] / allpath.length[i] + " " + allpath.flowindex[i] + " ";
                    for (int j = 1; j <= allpath.pathnum[i]; j++)
                        str = str + allpath.path[i, j] + " ";
                    listBox1.Items.Add(str);

                    sw.WriteLine(str);
                    str = "";
                }
                
            }
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        bool findnode(int l,int fnode,int i) //返回l弧段是否空余，并将空余节点存在数组node中第i个
        {
            //SQLchan db = new SQLchan();  // 数据库类
            //int tnode=0;
            //bool nfree = false;
            //String test2 = "select * from link where linkid=" + l.ToString() + ";";
            //DataTable linkdata = db.Select(test2);  //trap here
            //if (fnode == int.Parse(linkdata.Rows[0][1].ToString())) tnode = int.Parse(linkdata.Rows[0][2].ToString());
            //else tnode = int.Parse(linkdata.Rows[0][1].ToString());
            //if (arra[tnode] == -1) { nfree = true; node[i] = tnode; }
            //return nfree;

            int tnode = 0;
            bool nfree = false;
            //foreach(SQLchan.myLinkElement tmpMyLinkEle in dbData.mMyLinkTable)
            //{
            //    if(l == tmpMyLinkEle.linkId)
            //    {
            //        if (fnode == tmpMyLinkEle.fromNode)
            //            tnode = tmpMyLinkEle.toNode;
            //        else
            //            tnode = tmpMyLinkEle.fromNode;
            //        break;
            //    }
            //}
            if (fnode == dbData.mMyLinkTable[l - 3000].fromNode)
                tnode = dbData.mMyLinkTable[l - 3000].toNode;
            else
                tnode = dbData.mMyLinkTable[l - 3000].fromNode;
            if(arra[tnode] == -1)
            {
                nfree = true;
                node[i] = tnode;
            }
            return nfree;
        }

        int linknum(int a) //数据库返回a链接的弧段个数
        {
            //SQLchan db = new SQLchan();  // 数据库类
            //String test3 = "select linknum from node where nodeid="+a.ToString()+";";
            //DataTable tempDataTable = db.Select(test3);
            //return int.Parse(tempDataTable.Rows[0][0].ToString());

            //foreach(SQLchan.myNodeElement tmpMyNodeEle in dbData.mMyNodeTable)
            //{
            //    if (a == tmpMyNodeEle.nodeId)
            //        return tmpMyNodeEle.linkNum;
            //}
            //return -1;
            return dbData.mMyNodeTable[a].linkNum;
        }

        int linkid(int a,bool free) //数据库读取a中所有弧段名称，并返回没有使用的节点个数
        {
            //SQLchan db = new SQLchan();  // 数据库类
            //int lnum = linknum(a);
            //int NUM=0,nway=0;
            
            //String test4 = "select linkids from node where nodeid=" + a.ToString() + ";";
            //DataTable waystring = db.Select(test4);
            //string str=waystring.Rows[0][0].ToString();
            
            //if (free == false) NUM=lnum;
            //for (int i = 1; i <= lnum; i++)
            //{
            //    nway = int.Parse(str.Substring((i - 1) * 6, 4)); //截取路径信息
            //    if (free) {
            //        if (findnode(nway, a, NUM + 1))
            //            NUM++;
            //        way[NUM] = nway;
            //    }
            //    else
            //        way[i] = nway;
            //}
            //return NUM;

            int lnum = linknum(a);
            int NUM=0,nway=0;
            ArrayList tmpLinkIdsList = new ArrayList();

            //foreach(SQLchan.myNodeElement tmpMyNodeEle in dbData.mMyNodeTable)
            //{
            //    if(a == tmpMyNodeEle.nodeId)
            //    {
            //        tmpLinkIdsList = tmpMyNodeEle.linkIdsList;
            //        break;
            //    }
            //}
            tmpLinkIdsList = dbData.mMyNodeTable[a].linkIdsList;

            if (free == false)
                NUM = lnum;
            for (int i = 1; i <= lnum; i++)
            {
                nway = (int) tmpLinkIdsList[i - 1]; //截取弧段id信息
                if (free) {
                    if (findnode(nway, a, NUM + 1))//初始NUM=0，弧段nway另一节点是否出现在路径中
                    {
                        NUM++;
                        way[NUM] = nway;
                    }
                }
                else
                    way[i] = nway;
            }
            return NUM;
        }

        double[] data(int a, string type)
        {
            //SQLchan db = new SQLchan();  // 数据库类
            //String test5 = "select "+type+" from link where linkid=" + a.ToString() + ";";
            //DataTable str = db.Select(test5);
            //return double.Parse(str.Rows[0][0].ToString());

            double[] tmpcost = { 0, 0, 0};

            //foreach(SQLchan.myLinkElement tmpMyLinkEle in dbData.mMyLinkTable)
            //{
            //    if (a == tmpMyLinkEle.linkId)
            //    {
            //        //switch(type)
            //        //{
            //        //    case "length":
            //        //        return tmpMyLinkEle.length;
            //        //    case "coverage":
            //        //        return tmpMyLinkEle.coverage;
            //        //    case "flowindex":
            //        //        return tmpMyLinkEle.flowindex;
            //        //}
            //        //break;

            //        tmpcost[0] = tmpMyLinkEle.length;
            //        tmpcost[1] = tmpMyLinkEle.coverage;
            //        tmpcost[2] = tmpMyLinkEle.flowindex;
            //    }
            //}
            //log
           // Console.WriteLine("two points are " + dbData.mMyLinkTable[a - 3000].fromNode + " to " + dbData.mMyLinkTable[a - 3000].toNode);
            tmpcost[0] = dbData.mMyLinkTable[a - 3000].length;
            tmpcost[1] = dbData.mMyLinkTable[a - 3000].coverage;
            tmpcost[2] = dbData.mMyLinkTable[a - 3000].flowindex;
            //log
            //Console.WriteLine(a + " de tmpcost is " + tmpcost[0]);
            return tmpcost;
        }
        int findway(int a, int b)//返回以a，b为两端节点的弧段id
        {
            //SQLchan db = new SQLchan();  // 数据库类
            //String test6 = "select linkid from link where fromnode=" + a.ToString() + " and tonode="+ b.ToString() +";";
            //DataTable waystr = db.Select(test6); //trap here
            //if (waystr.Rows.Count == 0)
            //{
            //    test6 = "select linkid from link where fromnode=" + b.ToString() + " and tonode=" + a.ToString() + ";";
            //    waystr = db.Select(test6);
            //}
            //return int.Parse(waystr.Rows[0][0].ToString());

            //foreach(SQLchan.myLinkElement tmpMyLinkEle in dbData.mMyLinkTable)
            for(int i = 0; i < dbData.mMyLinkTable.Count; i++)
            {
                Console.WriteLine("totalcount is " + ++totalCount);
                SQLchan.myLinkElement tmpMyLinkEle = dbData.mMyLinkTable[i];
                //Console.WriteLine("dqz " + i + " times from node = " + tmpMyLinkEle.fromNode + " ,to node = " + tmpMyLinkEle.toNode);
                if (tmpMyLinkEle.fromNode == a)
                {
                    if (tmpMyLinkEle.toNode == b)
                    {
                        //Console.WriteLine("a to b, linkid is " + tmpMyLinkEle.linkId);
                        return tmpMyLinkEle.linkId;
                    }
                    else
                        continue;
                }
                else if (tmpMyLinkEle.toNode == a)
                {
                    if (tmpMyLinkEle.fromNode == b)
                    {
                        //Console.WriteLine("b to a, linkid is " + tmpMyLinkEle.linkId);
                        return tmpMyLinkEle.linkId;
                    }
                    else
                        continue;
                }
                else
                    continue;
                //if ((tmpMyLinkEle.fromNode == a && tmpMyLinkEle.toNode == b) || (tmpMyLinkEle.fromNode == b && tmpMyLinkEle.toNode == a))
                //    return tmpMyLinkEle.linkId;
            }

            return -1;
        }
        int getnode(int a, int b)  //返回与a连接的第b个节点
        {
            //int s=-1;
            //SQLchan db = new SQLchan();  // 数据库类
            //String test7 = "select linkids from node where nodeid=" + a.ToString() + ";";
            //DataTable waystr = db.Select(test7);
            //string str = waystr.Rows[0][0].ToString();

            //int nway = int.Parse(str.Substring((b - 1) * 6, 4));
            //if (findnode(nway, a, 1)) s = node[1]; //trap here
            //return s;

            int s = -1;
            ArrayList tmpLinkIdsList = new ArrayList();

            //foreach(SQLchan.myNodeElement tmpMyNodeEle in dbData.mMyNodeTable)
            //{
            //    if(a == tmpMyNodeEle.nodeId)
            //    {
            //        tmpLinkIdsList = tmpMyNodeEle.linkIdsList;
            //        break;
            //    }
            //}
            tmpLinkIdsList = dbData.mMyNodeTable[a].linkIdsList;
            int nway = (int)tmpLinkIdsList[b - 1];
            if (findnode(nway, a, 1))
                s = node[1];
            return s;
        }
        int numof(int a, int b) //返回b是在a中的第几位
        {
            //int lnum = linknum(a);
            //SQLchan db = new SQLchan();  // 数据库类
            //String test8 = "select linkids from node where nodeid=" + a.ToString() + ";";
            //DataTable wstr = db.Select(test8);
            //string str = wstr.Rows[0][0].ToString();
            //int nway=findway(a,b);
            //for (int i = 1; i <= lnum; i++)
            //    if (int.Parse(str.Substring((i - 1) * 6, 4)) == nway) return i;
            //return 0;

            //Console.WriteLine("numof a = " + a + " b = " + b);
            int lnum = linknum(a);
            ArrayList tmpLinkIdsList = new ArrayList();

            //foreach(SQLchan.myNodeElement tmpMyNodeEle in dbData.mMyNodeTable)
            //{
            //    if (a == tmpMyNodeEle.nodeId)
            //    {
            //        tmpLinkIdsList = tmpMyNodeEle.linkIdsList;
            //        break;
            //    }
            //}
            tmpLinkIdsList = dbData.mMyNodeTable[a].linkIdsList;
            int nway = findway(a, b);
            for (int i = 1; i <= lnum; i++)
                if (nway == (int)tmpLinkIdsList[i - 1])
                    return i;
            return 0;
        }
        //——————————————————————————[算法部分]
        void find(int now, int tn, int a,int b)    //从now的第b个节点找到最终节点 a节点跳过
        {
            int NUM=linkid(now,false);
            if (now == tn)
                suc = true;
            else
            {
                if (a < NUM)
                    for (int i = b + 1; i <= NUM; i++)
                    {
                        if (i == a) continue;
                        int s = getnode(now, i);  //trap here
                        if (s != -1 && arra[s] == -1)
                        {
                            n++;
                            arra[s] = n;
                            //length = length + data(findway(now, s), "length");
                            //coverage = coverage + data(findway(now, s), "coverage");
                            //flowindex = flowindex + data(findway(now, s), "flowindex");
                            double[] tmpcost = data(findway(now, s), "length");
                            length = length + tmpcost[0];
                            coverage = coverage + tmpcost[1];
                            flowindex = flowindex + tmpcost[2];

                            find(s, tn, 0, 0);
                            if (suc) break;
                            n--;
                            arra[s] = -1;
                            //length = length - data(findway(now, s), "length");
                            //coverage = coverage - data(findway(now, s), "coverage");
                            //flowindex = flowindex - data(findway(now, s), "flowindex");  //trap here
                            tmpcost = data(findway(now, s), "length");
                            length = length - tmpcost[0];
                            coverage = coverage - tmpcost[1];
                            flowindex = flowindex - tmpcost[2];
                        }
                    }
            }
        }
        void ourFind()
        {
            
            return;
        }
        void getpath(int lbee,int tn)                 //找到邻域  第lbee只蜂
        {
            transferpath(lbee);
            int nextnode = tn;
            int nownode = tn;
            suc=false;
            while(true)
            {
                if (n == 1) { find(nownode, tn, 0,0); }
                else
                {
                    nextnode = allpath.path[lbee, n];
                    arra[nextnode] = -1; node[1] = 0; n--;
                    nownode = allpath.path[lbee, n];
                    int NUM=numof(nownode,nextnode);
                    if (n==allpath.pathnum[lbee]-1) find(nownode, tn,NUM,NUM);
                    else find(nownode, tn, NUM, 0);
                }
                if (suc) break;  
            }
           
        }

        static int GetRandomSeed()//产生不同随机种子
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);//
            return BitConverter.ToInt32(bytes, 0);
        }

        void ranpath(int a, int b)
        {
            clearpath();
            int nownode = a;
            suc = true;
            n = 1;
            arra[nownode] = n;
            while (nownode != b) 
            {
                int lnum_free=linkid(nownode,true);
                if (lnum_free == 0) {
                    suc =false;
                    break;
                } //没能找到路径

               // long tick = DateTime.Now.Ticks;
               // Random ran = new Random((int)(tick & 0xffffffffL)|(int)(tick>>32)); //随机
                Random ran = new Random(GetRandomSeed());
                int randkey = ran.Next(1, lnum_free+1);

                nownode = node[randkey];
                n++; 
                arra[nownode] = n;
                //length = length + data(way[randkey],"length");
                //coverage = coverage + data(way[randkey], "coverage");
                //flowindex = flowindex + data(way[randkey], "flowindex");
                double[] tmpcost = data(way[randkey], "length");
                length = length + tmpcost[0];
                coverage = coverage + tmpcost[1];
                flowindex = flowindex + tmpcost[2];
                //log
                //Console.WriteLine("after calc length is " + length);
                //Console.WriteLine();
            }
         }

        void initialise(int fnode,int tnode,int SN)
        {
            
            int i=1;
            allpath.num = SN;
            while (i <= SN)
            {
                bool pareto = true; 
                ranpath(fnode,tnode);
                if (suc == false) continue;
                //与已有解比较，不是pareto则不保存
                for (int j = 1; j < i; j++)
                {
                    if ((length >= allpath.length[j] )&& ((coverage / length) <= (allpath.coverage[j] / allpath.length[j]) )&&( flowindex <= allpath.flowindex[j]))
                    { pareto = false; break; }
                }
                if (pareto)
                {
                    savepath(i);
                    leader[i] = i;
                    i++;
                }
                //log
                //Console.WriteLine(i + "pareto=" + pareto);
                //for (int j = 1; j <= allpath.pathnum[i - 1]; j++)
                //    Console.WriteLine(allpath.path[i - 1, j] + " ");
                //Console.WriteLine("a path is generated!!\n");
            }
            clearfollower(SN);
        }
       
        void ranfollower(int a,int NUM)  //按概率分配跟随蜂 (随机) 第a只跟随蜂选择的领导蜂 NUM为领导蜂（路径数量）
        {
            long tick = DateTime.Now.Ticks;
            Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32)); //随机
            int randkey = ran.Next(1, NUM + 1);
            follower[a] = randkey;
        }
         
        void leaderbee(int NUM,int tn,int limit)  //领导蜂 NUM领导蜂数，tn终点
        {
            for (int i = 1; i <= NUM; i++)
            {
                getpath(leader[i],tn);
                if (length <= allpath.length[leader[i]] || (coverage / length) >= (allpath.coverage[leader[i]] / allpath.length[leader[i]]) || flowindex >= allpath.flowindex[leader[i]])
               // if (length <= 1106 || (coverage / length) >= 0.55 || flowindex >= 269)
                {
                    bool same = false; int count;
                    allpath.num++;
                    savepath(allpath.num);
                    for (int j = 1; j <= allpath.num - 1; j++)
                    {
                        count = 0;
                        for (int k = 1; k <= allpath.pathnum[j]; k++)
                            if (allpath.path[j, k] == allpath.path[allpath.num, k]) count++;
                        if (count == allpath.pathnum[j]) { same = true; break; }
                    }
                    if (same) allpath.num--; else leader[i] = allpath.num;
                }
                else leaderlimit[i]++;
                if (leaderlimit[i] >= limit)
                {
                    leadernum--;
                    for (int j = i; j <= leadernum; j++)
                    {
                        leader[j] = leader[j + 1];
                        leaderlimit[j] = leaderlimit[j + 1];
                    }
                    sconternum++;
                    i--; NUM--;
                }
            }
        }
        
        void followbee(int NUM, int tn)  //跟随蜂
        {
            int nownode=tn,nextnode=tn;
            for (int i = 1; i <= NUM; i++)
            {
                ranfollower(i,leadernum);
                getpath(follower[i],tn);
                while (true)
                {
                    bool same = false; int count;
                    savepath(allpath.num+1);
                    for (int j = 1; j <= allpath.num; j++)
                    {
                        count = 0;
                        for (int k = 1; k <= allpath.pathnum[j]; k++)
                            if (allpath.path[j, k] == allpath.path[allpath.num+1, k]) count++;
                        if (count == allpath.pathnum[j]) { same = true; break; }
                    }
                    if (!same) break;
                    getpath(allpath.num + 1, tn);
                }
                leadernum++; allpath.num++;
                leader[leadernum] = allpath.num;
            }
            followernum = 0;
        }

        void scontbee(int NUM, int fn,int tn)  //侦察蜂
        {
            for (int i = 1; i <= NUM; i++)
            {
                leadernum++;
                allpath.num++;
                suc=false;
                while(!suc)
                {  
                     ranpath(fn,tn);
                     if (suc == false) continue;
                     savepath(allpath.num);
                     leader[leadernum] = allpath.num;
                 }  
            }
            sconternum = 0;
        }
      
        private void button2_Click(object sender, EventArgs e)
        {
            int SN = int.Parse(textBox4.Text.ToString());
            int leaderlimit = int.Parse(textBox5.Text.ToString());
            int iteration = int.Parse(textBox6.Text.ToString());
            int fnode = int.Parse(textBox2.Text.ToString());
            int tnode = int.Parse(textBox3.Text.ToString());
            leadernum = followernum = SN;

            //Store data we need in memory from db
            SQLchan db = new SQLchan();  // 数据库类
            dbData = db.StoreInMemory();

            initialise(fnode,tnode,SN);
           
          /*  for (int i = 1; i <= iteration; i++)
            {
               //leaderbee(leadernum, tnode,leaderlimit);
               //followbee(followernum, tnode);
                leaderbee(leadernum, fnode, leaderlimit);
                followbee(followernum, fnode);
               if (sconternum>0) 
                   scontbee(sconternum, fnode, tnode);
            }*/
            show();
        }
        //——————————————————————————
        class pathway   //路径集合类
        { 
            public int num;
            public int[,] path = new int[200, 160];
            public double[] length = new double[200];

            public double[] coverage = new double[200];
            public double[] flowindex = new double[200];
            public int[] pathnum = new int[200];
        }



        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }
    }
}
