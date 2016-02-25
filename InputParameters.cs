
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using System.Data.SQLite;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;

namespace mng
{
    public partial class InputParameters : UserControl
    {
        #region поля
        //объект класса ..., нужен для обращения к базе данных
        private sqliteclass sqlite;// = new sqliteclass();
        private string filename_inp_path = @"../../../inpar_Kulygin_2.sqlite";
        private DataSet dsPar { get; set; }
        
        //для панели, где отображаются значения параметров
        int selectInpar_id { get; set; }

        //список справочников
        private List<string> handbookList;
        private Dictionary<int,int> tvmwList_hb_id;
        //список баз данных
        private List<DataSet> dslist;
        private TreeView treeViewMW = new TreeView();
        private TreeNode nodeSelect = new TreeNode();
        private TreeNode nodeCheck = new TreeNode();
        private TreeNode nodeOldSelect = new TreeNode();

        //новый параметр
        private TreeNode newNode;
        //флаг нажатия на чекбокс(событие) 
        private bool flagCheck;
        //для открытия диалогового окна 
        private bool flagQuestionSave = false;
        private bool flagIzmenenie { get; set; }

        private int countItemDB { get; set; }
        private int countDGV { get; set; }
        private int typeDGV { get; set; }

        const string message = "Сохранить изменения параметра?";
        const string caption = "Вопрос на миллион";
        #endregion

        #region отображение
        //отображение панели для 0 и 1 уровня
        private void otobrajenie01()
        {
            cleanWorkingPanel();
            treeViewCheck.Visible = false;
            addParam.Visible = true;
            radioButtonCb.Visible = false;
            radioButtonTb.Visible = false;
            panelForData.Visible = false;
            buttonCancelPar.Visible = false;
            buttonCreatePar.Visible = false;
            flagIzmenenie = true;
        }
        //отображение панели для 2 уровня
        private void otobrajenie2()
        {
            treeViewCheck.Visible = true;
            treeViewCheck.Enabled = true;
            addParam.Visible = false;
            radioButtonCb.Visible = true;
            radioButtonTb.Visible = true;
            buttonCancelPar.Visible = true;
            buttonCreatePar.Visible = true;
        }

        public InputParameters()
        {
            InitializeComponent();
            otobrajenie01();
            flagIzmenenie = true;
        }
        #endregion

        #region методы мввью
        internal void createTreeViewMW(List<string> handbookList, TreeView treeViewMW)
        {
            //открытие всех баз данных и добавление в список
            sqlite = new sqliteclass();
            dsPar = sqlite.dataSetParamLoader(filename_inp_path);
            //connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;");
            dslist = new List<DataSet>();
            for (int i = 0; i < handbookList.Count; i++)
            {
                dslist.Add(sqlite.dataSetLoader(handbookList[i]));
            }
            //принимаем все значения из мэйнвиндоу 
            this.handbookList = handbookList;
            this.treeViewMW = treeViewMW;
            tvmwList_hb_id = new Dictionary<int, int>();

            createrTVMW();
            createTreeViewCheck();
            otobrajenie01();
        }
        
        //построение тривью
        public void createrTVMW()
        {
            try
            {
                treeViewMW.Nodes.Clear();
                treeViewMW.Nodes.Add("Общие параметры");
                for (int i = 0; i < handbookList.Count; i++)
                {
                    //treeViewMW.Nodes.Add(handbookList[i]);
                    treeViewMW.Nodes.Add(dslist[i].Tables["HB"].Rows[0]["HB_name"].ToString());
                    treeViewMW.Nodes[i + 1].Tag = dslist[i].Tables["HB"].Rows[0]["HB_id"].ToString();
                    tvmwList_hb_id.Add(Int32.Parse(treeViewMW.Nodes[i + 1].Tag.ToString()), i + 1);

                    treeViewMW.Nodes[i + 1].Nodes.Add("Общие параметры");
                    foreach (DataRow dr in dslist[i].Tables["LST_CLASS"].Rows)
                    {
                        treeViewMW.Nodes[i + 1].Nodes.Add(dr["CLASS_ID"].ToString(), dr["CLASS_NAME"].ToString());
                    }
                }
                constrParam();
                treeViewMW.Nodes[1].Expand();
            }
            catch { }
        }

        //заполняет определенный справочник своими параметрами
        public void constrParam()
        {
            try
            {
                foreach (DataRow dr in dsPar.Tables["sl_table"].Rows)
                {
                    //переменные из таблицы sl_table, будем использовать для таблицы lst_inpar, чтобы взять данные
                    int class_id = Int32.Parse(dr["class_id"].ToString());
                    int inpar_id = Int32.Parse(dr["inpar_id"].ToString());
                    int hb_id = Int32.Parse(dr["handbook_id"].ToString());
                    int numInList_hb;
                    tvmwList_hb_id.TryGetValue(hb_id, out numInList_hb);
                    if (hb_id == -1)
                        treeViewMW.Nodes[0].Nodes.Add(dsPar.Tables["LST_INPAR"].Rows[inpar_id - 1]["NAME"].ToString());
                    else if (class_id == -1)
                        treeViewMW.Nodes[numInList_hb].Nodes[0].Nodes.Add(dsPar.Tables["LST_INPAR"].Rows[inpar_id - 1]["NAME"].ToString());
                    else
                        treeViewMW.Nodes[numInList_hb].Nodes[class_id].Nodes.Add(dsPar.Tables["LST_INPAR"].Rows[inpar_id - 1]["NAME"].ToString());
                }
            }
            catch { }
        }
        #endregion

        private void proverkaNaIzmenenie()
        {
            try
            {
                int inpar_id = specificCell();
                if (inpar_id != 0)
                {
                    using (SQLiteConnection connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;Read Only=True;", true))
                    {
                        //connpar.Open();
                        string type, name, shortName, min, max;
                        string cmd = @"SELECT *FROM LST_INPAR WHERE INPAR_ID = @inpar_id";
                        using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                        {
                            command.Parameters.AddWithValue("@inpar_id", inpar_id);
                            connpar.Open();
                            using (SQLiteDataReader rdr = command.ExecuteReader())
                            {
                                rdr.Read();
                                type = rdr[1].ToString();
                                name = rdr[2].ToString();
                                shortName = rdr[3].ToString();
                                min = rdr[4].ToString();
                                max = rdr[5].ToString();
                                rdr.Close();
                            }
                            connpar.Close();
                        }
                        if (radioButtonCb.Checked)
                        {
                            //получение количества строк в базе и дгв для стравнения
                            string cmd3 = @"SELECT COUNT(*) FROM LST_ITEM WHERE INPAR_ID = @inpar_id";
                            using (SQLiteCommand command3 = new SQLiteCommand(cmd3, connpar))
                            {
                                command3.Parameters.AddWithValue("@inpar_id", inpar_id);
                                connpar.Open();
                                using (SQLiteDataReader rdr3 = command3.ExecuteReader())
                                {
                                    rdr3.Read();
                                    this.countItemDB = Int32.Parse(rdr3[0].ToString());
                                    rdr3.Close();
                                }
                                connpar.Close();
                            }
                            this.countDGV = dataGridView.RowCount - 1;

                            if (!cbEdit.Checked)
                                this.typeDGV = 0;
                            else
                                this.typeDGV = 1;
                        }
                        else
                            if (!tbInt.Checked)
                                this.typeDGV = 2;
                            else
                                this.typeDGV = 3;

                        //сходятся ли данные dgv с базой
                        string cmd2 = @"SELECT ITEM_NAME, ITEM_POSITION FROM LST_ITEM WHERE INPAR_ID = @inpar_id";
                        using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                        {
                            command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                            string item_name;
                            string item_position;
                            connpar.Open();
                            using (SQLiteDataReader rdr2 = command2.ExecuteReader())
                            {
                                int indexC = 0;
                                if (this.typeDGV == 0)
                                {
                                    if (this.countItemDB == this.countDGV)
                                    {
                                        try
                                        {
                                            while (rdr2.Read())
                                            {
                                                item_name = rdr2[0].ToString();
                                                item_position = rdr2[1].ToString();
                                                if (item_name != dataGridView.Rows[Int32.Parse(item_position) - 1].Cells[0].Value.ToString() || item_position != (indexC + 1).ToString())
                                                {
                                                    flagQuestionSave = true;
                                                    rdr2.Close();
                                                    connpar.Close();
                                                    return;
                                                }
                                                indexC++;
                                            }
                                        }
                                        finally
                                        {
                                            rdr2.Close();
                                        }
                                    }
                                    else
                                    {
                                        flagQuestionSave = true;
                                        connpar.Close();
                                        return;
                                    }
                                }
                                else if (this.typeDGV == 1)
                                {
                                    if (this.countItemDB == this.countDGV)
                                    {
                                        try
                                        {
                                            while (rdr2.Read())
                                            {
                                                item_name = rdr2[0].ToString();
                                                if (item_name != dataGridView.Rows[indexC].Cells[0].Value.ToString())
                                                {
                                                    flagQuestionSave = true;
                                                    rdr2.Close();
                                                    connpar.Close();
                                                    return;
                                                }
                                                indexC++;
                                            }
                                        }
                                        finally
                                        {
                                            rdr2.Close();
                                        }
                                    }
                                    else
                                    {
                                        flagQuestionSave = true;
                                        connpar.Close();
                                        return;
                                    }
                                }
                            }
                            connpar.Close();
                        }

                        if (type != this.typeDGV.ToString() || txtName.Text != name || txtShortName.Text != shortName || txtMin.Text != min || txtMax.Text != max)
                        {
                            flagQuestionSave = true;
                        }
                    }
                }
            }
            catch(Exception ex) { MessageBox.Show(ex + ""); }
        }

        #region выделение узлов
        internal void selectedNode(TreeNode nodeSelect, TreeView treeViewMW)
        {
            try
            {
                if (!flagIzmenenie)
                    proverkaNaIzmenenie();
                flagIzmenenie = false;
                //условие для возникновения сообщения при создании нового параметра и изменении выделенного
                if (flagQuestionSave)
                {
                    GC.Collect();
                    var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);//Cancel, MessageBoxIcon.Question);
                    switch (result)
                    {
                        case DialogResult.Yes:
                            flagQuestionSave = false;
                            this.nodeOldSelect = this.nodeSelect;
                            this.nodeSelect = nodeSelect;
                            //проверка адекватности сохраняемых данных 
                            if (!saveParam())
                            {
                                flagQuestionSave = true;
                                return;
                            }
                            this.nodeSelect = nodeSelect;
                            treeViewMW.SelectedNode = nodeSelect;
                            break;
                            //return;
                        case DialogResult.No:
                            flagQuestionSave = false;
                            if (newNode == this.nodeSelect)
                            {
                                deleteParam(this.nodeSelect);
                            }
                            break;
                        //case DialogResult.Cancel:
                        //    //flagCancel = false;
                        //    //treeViewMW.SelectedNode = this.nodeSelect;
                        //    //flagCancel = true;      
                        //    flagQuestionSave = false;
                        //    return;
                        //break;
                    }
                }


                this.nodeSelect = nodeSelect;
                treeViewMW.HideSelection = false;
                switch (nodeSelect.Level)
                {
                    //выделение параметра в глобальных параметрах
                    case 1:
                        {
                            if (nodeSelect.Parent.Index == 0)
                            {
                                otobrajenie2();
                                checking();
                                workingWihtTable();
                            }
                            else
                                otobrajenie01();
                            break;
                        }
                    //выделение параметра в классах и в том числе и локальных параметрах
                    case 2:
                        {
                            otobrajenie2();
                            checking();
                            workingWihtTable();
                            break;
                        }
                    default:
                        {
                            otobrajenie01();
                            break;
                        }
                }
                //treeViewMW.Focus();
                flagCheck = false;
            }
            catch (Exception ex) { MessageBox.Show(ex + ""); }
        }
        #endregion

        #region методы чеквью
        public int checkedAllLocalProverka(int handbookIndex)
        {
            foreach (TreeNode nd in treeViewCheck.Nodes[handbookIndex].Nodes)
            {
                if (nd.Checked != true)
                    return 0;
            }
            treeViewCheck.Nodes[handbookIndex].Checked = true;
            //flagClickJeneralCheck = true;
            return 1;
        }

        public int checkedAllGlobalProverka(int handbookIndex)
        {
            for (int i = 1; i <= handbookList.Count; i++)
            {
                if (treeViewCheck.Nodes[i].Checked != true)
                    return 0;
            }
            return 1;
        }

        public void checkedAll(int ind)
        {
            treeViewCheck.Nodes[ind].Checked = true;
            foreach (TreeNode nd in treeViewCheck.Nodes[ind].Nodes)
            {
                nd.Checked = true;
            }
        }

        public void unCheckedAll(int ind)
        {
            treeViewCheck.Nodes[ind].Checked = false;
            foreach (TreeNode nd in treeViewCheck.Nodes[ind].Nodes)
            {
                nd.Checked = false;
            }
        }

        private void createTreeViewCheck()
        {
            try
            {
                treeViewCheck.Nodes.Clear();
                treeViewCheck.Nodes.Add("Выделение всех параметров");
                for (int i = 0; i < handbookList.Count; i++)
                {
                    //treeViewCheck.Nodes.Add(handbookList[i]);
                    treeViewCheck.Nodes.Add(dslist[i].Tables["HB"].Rows[0]["HB_name"].ToString());
                    treeViewCheck.Nodes[i + 1].Tag = dslist[i].Tables["HB"].Rows[0]["HB_id"].ToString();
                    foreach (DataRow dr in dslist[i].Tables["LST_CLASS"].Rows)
                    {
                        treeViewCheck.Nodes[i + 1].Nodes.Add(dr["CLASS_NAME"].ToString());
                    }
                }

                treeViewCheck.Nodes[1].Expand();
                treeViewCheck.CheckBoxes = true;
            }
            catch { }
        }

        private void clearTreeViewCheck()
        {
            for (int i = 0; i < handbookList.Count + 1; i++)
            {
                foreach (TreeNode node in treeViewCheck.Nodes[i].Nodes)
                {
                    node.Checked = false;
                }
                treeViewCheck.Nodes[i].Checked = false;
            }
        }

        public void checking()
        {
            try
            {
                //открытие обновленной бд
                dsPar = sqlite.dataSetParamLoader(filename_inp_path);
                using (var connpar = new SQLiteConnection("Data source=" + filename_inp_path + ";Version=3; Read Only=True;", true))
                {
                    //connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;ReadOnly=True;");
                    string cmd = @"SELECT *FROM sl_table WHERE inpar_id=@inpar_id";
                    using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                    {
                        clearTreeViewCheck();
                        if (nodeSelect.Level == 2)
                        {
                            selectInpar_id = specificCell();
                            command.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                            connpar.Open();
                            using (SQLiteDataReader rdr = command.ExecuteReader())
                            {
                                try
                                {
                                    while (rdr.Read())
                                    {
                                        int numInList_hb;
                                        tvmwList_hb_id.TryGetValue(Int32.Parse(rdr[1].ToString()), out numInList_hb);
                                        object class_id = rdr[2];
                                        if (Int32.Parse(class_id.ToString()) != -1)
                                        {
                                            treeViewCheck.Nodes[numInList_hb].Nodes[Int32.Parse(class_id.ToString()) - 1].Checked = true;
                                        }
                                        else
                                        {
                                            checkedAll(numInList_hb);
                                        }
                                    }
                                }
                                finally
                                {
                                    rdr.Close();
                                    connpar.Close();
                                }
                            }
                        }
                        //в глобальных парам 
                        else
                        {
                            for (int j = 0; j < handbookList.Count + 1; j++)
                            {
                                checkedAll(j);
                            }
                            //добавить отображение главных чеков
                            selectInpar_id = specificCell();
                        }
                    }
                }
            }
            catch { }
        }

        private void treeViewCheck_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.nodeCheck = e.Node;

            if (flagCheck)
            {
                if (e.Node.Checked)
                {
                    //обход нажатия узла, теперь только чек
                    try
                    {
                        if (!nodeCheck.Parent.Checked)
                        {
                            foreach (TreeNode item in treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[nodeCheck.Index + 1].Nodes)
                            {
                                if (item.Text == nodeSelect.Text)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    catch
                    {
                        addAt();
                        e.Node.Checked = true;
                        return;
                    }
                    //перенес из под кэтча//
                    addAt();
                    e.Node.Checked = true;
                }
                else
                {
                    removeAt();
                    e.Node.Checked = false;
                }
            }
            flagCheck = false;
        }

        private void treeViewCheck_AfterCheck(object sender, TreeViewEventArgs e)
        {
            flagCheck = true;
        }
        #endregion

        #region нахождение определенного inpar_id для параметра(определенная ячейка таблицы инпар)
        private int specificCell()
        {
            int inpar_id = 0;
            try
            {
                foreach (DataRow item in dsPar.Tables["LST_INPAR"].Rows)
                {
                    if (item["NAME"].Equals(nodeSelect.Text))
                        inpar_id = Int32.Parse(item["INPAR_ID"].ToString());
                }
            }
            catch { }
            return inpar_id;
        }
        #endregion

        #region метод для удаления лишних узлов из тривьюМайн, при переходе параметров из классов в общие и наоборот
        private void removingNodes(int hb_id, int class_id, TreeNode nodeClone)
        {
            foreach (TreeNode item in treeViewMW.Nodes[hb_id].Nodes[class_id].Nodes)
            {
                if (item.Text.Equals(nodeClone.Text))
                {
                    treeViewMW.Nodes[hb_id].Nodes[class_id].Nodes[item.Index].Remove();
                    break;
                }
            }
        }

        private void removingNodes2(int hb_id, TreeNode nodeClone)
        {
            foreach (TreeNode item in treeViewMW.Nodes[hb_id].Nodes)
            {
                if (item.Text.Equals(nodeClone.Text))
                {
                    treeViewMW.Nodes[hb_id].Nodes[item.Index].Remove();
                    break;
                }
            }
        }
        #endregion

        #region добавление
        private void addAt()
        {
            try
            {
                //клоны для добавления в классы справочников
                nodeOldSelect = nodeSelect;
                TreeNode nodeClone = (TreeNode)nodeSelect.Clone();
                treeViewMW.SelectedNode = null;
                using (var connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;"))
                {
                    //определяем 
                    int inpar_id = specificCell();
                    string cmd = @"INSERT INTO sl_table (inpar_id,handbook_id,class_id) VALUES (@inpar_id,@handbook_id,@class_id)";
                    using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                    {
                        if (nodeCheck.Level == 0)
                        {
                            //добавление при нажатии на главный чек
                            if (nodeCheck.Index == 0)
                            {
                                connpar.Open();     
                                string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                                using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                                {
                                    command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                                    command2.ExecuteNonQuery();
                                }  
                                command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                command.Parameters.AddWithValue("@handbook_id", -1);
                                command.Parameters.AddWithValue("@class_id", "");
                                command.ExecuteNonQuery();
                                connpar.Close();

                                //удаление во всех остатльных справочниках
                                for (int i = 1; i < treeViewCheck.Nodes.Count; i++)
                                {
                                    for (int j = 0; j < treeViewCheck.Nodes[i].Nodes.Count + 1; j++)
                                    {
                                        removingNodes(i, j, nodeClone);
                                    }
                                }
                                TreeNode nodeSelectNew = (TreeNode)nodeClone.Clone();
                                treeViewMW.Nodes[0].Nodes.Add(nodeSelectNew);//.Insert(0, (TreeNode)nodeSelect.Clone());
                                treeViewMW.SelectedNode = nodeSelectNew;
                            }
                            //добавление при нажатии на чек справочника
                            else
                            {
                                Boolean flagLocCheck = proverkaVidelLocCheck(nodeCheck.Index);

                                if (flagLocCheck)
                                {
                                    connpar.Open();  
                                    //и удалить везде 
                                    string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                                    using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                                    {
                                        command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                                        command2.ExecuteNonQuery();
                                    }
                                    command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                    command.Parameters.AddWithValue("@handbook_id", -1);
                                    command.Parameters.AddWithValue("@class_id", "");
                                    command.ExecuteNonQuery();
                                    connpar.Close();

                                    //удаление во всех справочниках
                                    for (int i = 1; i < treeViewCheck.Nodes.Count; i++)
                                    {
                                        if (i == nodeCheck.Index)
                                        {
                                            for (int j = 0; j < treeViewCheck.Nodes[i].Nodes.Count + 1; j++)
                                            {
                                                removingNodes(i, j, nodeClone);
                                            }
                                        }
                                        else
                                            removingNodes(i, 0, nodeClone);
                                    }
                                    TreeNode nodeSelectNew = (TreeNode)nodeClone.Clone();
                                    treeViewMW.Nodes[0].Nodes.Add(nodeSelectNew);
                                    treeViewMW.SelectedNode = nodeSelectNew;
                                }
                                else
                                {
                                    connpar.Open();
                                    //и удалить везде      
                                    string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id";
                                    using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                                    {
                                        command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                                        command2.Parameters.AddWithValue("@handbook_id", nodeCheck.Tag);//.Index);
                                        command2.ExecuteNonQuery();
                                    }
                                    int nodeClonePPIndex = nodeSelect.Parent.Parent.Index;
                                    command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                    command.Parameters.AddWithValue("@handbook_id", nodeCheck.Tag);//.Index);
                                    command.Parameters.AddWithValue("@class_id", -1);
                                    command.ExecuteNonQuery();
                                    connpar.Close();

                                    for (int i = 1; i < treeViewMW.Nodes[nodeCheck.Index].Nodes.Count; i++)
                                    {
                                        removingNodes(nodeCheck.Index, i, nodeClone);
                                    }
                                    TreeNode nodeSelectNew = (TreeNode)nodeClone.Clone();
                                    treeViewMW.Nodes[nodeCheck.Index].Nodes[0].Nodes.Add(nodeSelectNew);
                                    if (nodeClonePPIndex == nodeCheck.Index)
                                    {
                                        treeViewMW.SelectedNode = nodeSelectNew;
                                    }
                                    else
                                    {
                                        treeViewMW.SelectedNode = nodeOldSelect;
                                    }

                                    checkedAll(nodeCheck.Index);
                                }
                            }
                        }
                        //добавление параметра при нажатии на чек класса
                        else
                        {
                            Boolean flagLocCheck = proverkaVidelLocCheck(nodeCheck.Parent.Index);
                            Boolean flagClassCheck = proverkaVidelClassCheck(nodeCheck.Parent.Index, nodeCheck.Index);

                            if (flagLocCheck && flagClassCheck)
                            {
                                connpar.Open();     
                                //и удалить везде 
                                string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                                using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                                {
                                    command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                                    command2.ExecuteNonQuery();
                                }
                                command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                command.Parameters.AddWithValue("@handbook_id", -1);
                                command.Parameters.AddWithValue("@class_id", "");
                                command.ExecuteNonQuery();
                                connpar.Close();

                                //удаление во всех справочниках
                                for (int i = 1; i < treeViewCheck.Nodes.Count; i++)
                                {
                                    if (i == nodeCheck.Parent.Index)
                                    {
                                        for (int j = 0; j < treeViewCheck.Nodes[i].Nodes.Count + 1; j++)
                                        {
                                            removingNodes(i, j, nodeClone);
                                        }
                                    }
                                    else
                                        removingNodes(i, 0, nodeClone);
                                }
                                TreeNode nodeSelectNew = (TreeNode)nodeClone.Clone();
                                treeViewMW.Nodes[0].Nodes.Add(nodeSelectNew);
                                treeViewMW.SelectedNode = nodeSelectNew;
                                //nodeSelectNew.TreeView.Focus();
                            }
                            else if (flagClassCheck)
                            {
                                connpar.Open();     
                                //и удалить везде      
                                string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id";
                                using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                                {
                                    command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                                    command2.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Tag);//.Index);
                                    command2.ExecuteNonQuery();
                                }
                                int nodeClonePPIndex = nodeSelect.Parent.Parent.Index;
                                command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                command.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Tag);//Index);
                                command.Parameters.AddWithValue("@class_id", -1);
                                command.ExecuteNonQuery();
                                connpar.Close();

                                for (int i = 1; i < treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes.Count; i++)
                                {
                                    removingNodes(nodeCheck.Parent.Index, i, nodeClone);
                                }
                                TreeNode nodeSelectNew = (TreeNode)nodeClone.Clone();
                                treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[0].Nodes.Add(nodeSelectNew);
                                //надо править выделение 
                                if (nodeClonePPIndex == nodeCheck.Parent.Index)
                                {
                                    treeViewMW.SelectedNode = nodeSelectNew;
                                }
                                else
                                {
                                    treeViewMW.SelectedNode = nodeOldSelect;
                                }
                                checkedAll(nodeCheck.Parent.Index);

                            }
                            else
                            {
                                //нужна проверка на добавлении последнее чека в справочнике в лок и глоб масштабе 
                                treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[nodeCheck.Index + 1].Nodes.Add((TreeNode)nodeClone.Clone());//.Insert(0, (TreeNode)nodeSelect.Clone());
                                command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                command.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Tag);//.Index);
                                command.Parameters.AddWithValue("@class_id", nodeCheck.Index + 1);
                                connpar.Open();
                                command.ExecuteNonQuery();
                                connpar.Close();

                                treeViewMW.SelectedNode = nodeOldSelect;
                            }
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        #region удаление
        private void removeAt()
        {
            try
            {
                nodeOldSelect = nodeSelect;
                //определяем yacheyku
                int inpar_id = specificCell();
                TreeNode nodeClone = (TreeNode)nodeSelect.Clone();
                treeViewMW.SelectedNode = null;
                using (var connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;"))
                {
                    if (nodeCheck.Level == 0)
                    {
                        connpar.Open();
                        if (nodeCheck.Index == 0)
                        {
                            string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                            using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                            {
                                command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                command.ExecuteNonQuery();
                            }
                            removingNodes2(nodeCheck.Index, nodeClone);
                            for (int i = 1; i < handbookList.Count; i++)
                            {
                                unCheckedAll(nodeCheck.Index);
                            }
                            otobrajenie01();
                            //treeViewMW.SelectedNode = null;
                        }
                        else
                        {
                            //проверка на выделение главного чека
                            if (treeViewCheck.Nodes[0].Checked)
                            {
                                string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                                using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                                {
                                    command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                    command.ExecuteNonQuery();
                                }

                                string cmd2 = @"INSERT INTO sl_table (inpar_id,handbook_id,class_id) VALUES (@inpar_id,@handbook_id,@class_id)";
                                using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                                {
                                    command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                                    command2.Parameters.AddWithValue("@class_id", -1);

                                    foreach (var hb_id in tvmwList_hb_id.Keys)
                                    {
                                        int numInList_hb;
                                        tvmwList_hb_id.TryGetValue(hb_id, out numInList_hb);
                                        if (numInList_hb != nodeCheck.Index)
                                        {
                                            treeViewMW.Nodes[numInList_hb].Nodes[0].Nodes.Add((TreeNode)nodeClone.Clone());
                                            command2.Parameters.AddWithValue("@handbook_id", hb_id);
                                            command2.ExecuteNonQuery();
                                        }
                                    }
                                    //for (int hb_id = 1; hb_id < treeViewMW.Nodes.Count; hb_id++)
                                    //{
                                    //    int numInList_hb;
                                    //    tvmwList_hb_id.TryGetValue(hb_id, out numInList_hb);
                                    //    if (hb_id != nodeCheck.Index)
                                    //    {
                                    //        treeViewMW.Nodes[hb_id].Nodes[0].Nodes.Add((TreeNode)nodeClone.Clone());
                                    //        command2.Parameters.AddWithValue("@hb_id", hb_id);
                                    //        command2.ExecuteNonQuery();
                                    //    }
                                    //}
                                }
                                removingNodes2(0, nodeClone);
                                treeViewCheck.Nodes[0].Checked = false;
                                otobrajenie01();
                                //treeViewMW.SelectedNode = null;
                            }
                            else
                            {
                                int indHb;
                                if (nodeSelect.Level == 1)
                                    indHb = nodeSelect.Parent.Index;
                                else
                                    indHb = nodeSelect.Parent.Parent.Index;

                                string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id";
                                using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                                {
                                    command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                    command.Parameters.AddWithValue("@handbook_id", nodeCheck.Tag);//.Index);
                                    command.ExecuteNonQuery();
                                }
                                removingNodes(nodeCheck.Index, 0, nodeClone);
                                if (indHb == nodeCheck.Index)
                                {
                                    otobrajenie01();
                                    //treeViewMW.SelectedNode = null;
                                }
                                else
                                {
                                    treeViewMW.SelectedNode = nodeOldSelect;
                                    //nodeOldSelect.TreeView.Focus();
                                }
                            }
                            unCheckedAll(nodeCheck.Index);
                        }
                        connpar.Close();
                    }
                    else
                    {
                        connpar.Open();
                        string cmd3 = @"SELECT *FROM sl_table WHERE inpar_id=@inpar_id";
                        using (SQLiteCommand command3 = new SQLiteCommand(cmd3, connpar))
                        {
                            command3.Parameters.AddWithValue("@inpar_id", inpar_id);
                            using (SQLiteDataReader rdr = command3.ExecuteReader())
                            {
                                try
                                {
                                    while (rdr.Read())
                                    {
                                        int hb_id = Int32.Parse(rdr[1].ToString());
                                        int class_id = Int32.Parse(rdr[2].ToString());
                                        int numInList_hb;
                                        tvmwList_hb_id.TryGetValue(hb_id, out numInList_hb);
                                        if (hb_id == -1)
                                        {
                                            string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                                            using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                                            {
                                                command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                                command.ExecuteNonQuery();
                                            }
                                            //добавить все классы кроме нажатого и все справочники кроме нажатого 
                                            string cmd2 = @"INSERT INTO sl_table (inpar_id,handbook_id,class_id) VALUES (@inpar_id,@handbook_id,@class_id)";
                                            using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                                            {
                                                command2.Parameters.AddWithValue("@inpar_id", inpar_id);

                                                for (int classIndex = 1; classIndex < treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes.Count; classIndex++)
                                                {
                                                    if (classIndex != nodeCheck.Index + 1)
                                                    {
                                                        treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[classIndex].Nodes.Add((TreeNode)nodeClone.Clone());
                                                        command2.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Tag);//.Index);
                                                        command2.Parameters.AddWithValue("@class_id", classIndex);
                                                        command2.ExecuteNonQuery();
                                                    }
                                                }

                                                command2.Parameters.AddWithValue("@class_id", -1);

                                                foreach (var hb_id2 in tvmwList_hb_id.Keys)
                                                {
                                                    int numInList_hb2;
                                                    tvmwList_hb_id.TryGetValue(hb_id2, out numInList_hb2);
                                                    if (numInList_hb2 != nodeCheck.Parent.Index)
                                                    {
                                                        treeViewMW.Nodes[numInList_hb2].Nodes[0].Nodes.Add((TreeNode)nodeClone.Clone());
                                                        command2.Parameters.AddWithValue("@handbook_id", hb_id2);
                                                        command2.ExecuteNonQuery();
                                                    }
                                                }
                                                //for (int hbIndex = 1; hbIndex < treeViewMW.Nodes.Count; hbIndex++)
                                                //{
                                                //    if (hbIndex != nodeCheck.Parent.Index)
                                                //    {
                                                //        treeViewMW.Nodes[hbIndex].Nodes[0].Nodes.Add((TreeNode)nodeClone.Clone());
                                                //        command2.Parameters.AddWithValue("@handbook_id", hbIndex);
                                                //        command2.ExecuteNonQuery();
                                                //    }
                                                //}
                                            }
                                            removingNodes2(0, nodeClone);

                                            treeViewCheck.Nodes[0].Checked = false;
                                            treeViewCheck.Nodes[nodeCheck.Parent.Index].Checked = false;
                                            otobrajenie01();
                                            //treeViewMW.SelectedNode = null;
                                            break;
                                        }
                                        //при удалении параметра из класса при выделенном главном чеке 
                                        else if (class_id == -1 && hb_id == Int32.Parse(nodeCheck.Parent.Tag.ToString()))//.Index)
                                        {

                                            string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id";
                                            using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                                            {
                                                command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                                command.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Tag);//.Index);
                                                command.ExecuteNonQuery();
                                            }
                                            string cmd2 = @"INSERT INTO sl_table (inpar_id,handbook_id,class_id) VALUES (@inpar_id,@handbook_id,@class_id)";
                                            using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                                            {
                                                command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                                                command2.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Tag);//.Index);

                                                for (int classIndex = 1; classIndex < treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes.Count; classIndex++)
                                                {
                                                    if (classIndex != nodeCheck.Index + 1)
                                                    {
                                                        treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[classIndex].Nodes.Add((TreeNode)nodeClone.Clone());
                                                        command2.Parameters.AddWithValue("@class_id", classIndex);
                                                        command2.ExecuteNonQuery();
                                                    }
                                                }
                                            }
                                            removingNodes(nodeCheck.Parent.Index, 0, nodeClone);

                                            treeViewCheck.Nodes[nodeCheck.Parent.Index].Checked = false;
                                            otobrajenie01();
                                            //treeViewMW.SelectedNode = null;
                                            break;
                                        }
                                        //обычное удалении из класса
                                        else if (hb_id == Int32.Parse(nodeCheck.Parent.Tag.ToString()))//.Index)
                                        {
                                            int indClass = nodeSelect.Parent.Index;

                                            string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id AND class_id=@class_id";
                                            using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                                            {
                                                command.Parameters.AddWithValue("@inpar_id", inpar_id);
                                                command.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Tag);//.Index);
                                                command.Parameters.AddWithValue("@class_id", nodeCheck.Index + 1);
                                                command.ExecuteNonQuery();
                                            }
                                            removingNodes(numInList_hb, nodeCheck.Index + 1, nodeClone);

                                            if (indClass == nodeCheck.Index + 1)
                                            {
                                                otobrajenie01();
                                                //treeViewMW.SelectedNode = null;
                                            }
                                            else
                                            {
                                                treeViewMW.SelectedNode = nodeOldSelect;
                                                //nodeOldSelect.TreeView.Focus();
                                            }
                                            break;
                                        }
                                    }
                                }
                                finally
                                {
                                    rdr.Close();
                                }
                            }
                        }
                        connpar.Close();
                    }
                }
            }
            catch { }
        }
        #endregion

        #region проверка для переноса параметров из классов в общие параметры и наоборот
        //для добавления последнее чека в справочниках
        public Boolean proverkaVidelLocCheck(int handbookIndex)
        {
            Boolean flagLocCheck = false;
            for (int i = 1; i < treeViewCheck.Nodes.Count; i++)
            {
                if (i != handbookIndex)
                {
                    if (treeViewCheck.Nodes[i].Checked)
                        flagLocCheck = true;
                    else
                    {
                        flagLocCheck = false;
                        break;
                    }
                }
            }
            return flagLocCheck;
        }

        //для добавления последнее чека в классах
        public Boolean proverkaVidelClassCheck(int handbookIndex, int classIndex)
        {
            Boolean flagClassCheck = false;
            for (int i = 0; i < treeViewCheck.Nodes[handbookIndex].Nodes.Count; i++)
            {
                if (i != classIndex)
                {
                    if (treeViewCheck.Nodes[handbookIndex].Nodes[i].Checked)
                        flagClassCheck = true;
                    else
                    {
                        flagClassCheck = false;
                        break;
                    }
                }
            }
            return flagClassCheck;
        }
        #endregion

        #region создание нового параметра
        public void newParam()
        {
            //добавление в справочники
            if (nodeSelect.Level == 0)
                //0,0 //глоб общ пар
                if (nodeSelect.Index == 0)
                    addNewParam(nodeSelect.Index, -2);
                //n,0 //лок общ парам
                else
                    addNewParam(Int32.Parse(nodeSelect.Tag.ToString()), -1);
            //добавление в классы справочников
            else if (nodeSelect.Level == 1)
                // n,0 //лок общ пар
                if (nodeSelect.Index == 0)
                    addNewParam(Int32.Parse(nodeSelect.Parent.Tag.ToString()), -1);
                //n,n //в класс
                else
                    addNewParam(Int32.Parse(nodeSelect.Parent.Tag.ToString()), nodeSelect.Index);
        }

        //добавление новых параметров, индексы - координаты выделенного справочника или класса, в который добавляем
        public void addNewParam(int index, int index_2)
        {
            try
            {
                int numInList_hb;
                tvmwList_hb_id.TryGetValue(index, out numInList_hb);
                using (var connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;"))
                {
                    connpar.Open();

                    //создаем ячейку в столбце сл_валид с новыми данными 
                    newNode = new TreeNode();
                    cleanWorkingPanel();

                    txtName.Text = "Новый параметр";
                    txtShortName.Text = "нп";

                    int numberNextInpar_Id = dsPar.Tables["LST_INPAR"].Rows.Count + 1;
                    string cmd = @"INSERT INTO LST_INPAR (INPAR_ID, TYPE, NAME, SHORT)";
                    cmd += "VALUES (" + numberNextInpar_Id + ", '" + 0 + "', '" + txtName.Text + "', '" + txtShortName.Text + "')";
                    using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                    {
                        command.ExecuteNonQuery();
                    }
                    string cmd2 = @"INSERT INTO sl_table (inpar_id, handbook_id, class_id) VALUES(@inpar_id, @handbook_id, @class_id) ";
                    using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                    {
                        command2.Parameters.AddWithValue("@inpar_id", numberNextInpar_Id);

                        if (index_2 < 0)
                        {
                            if (index_2 == -2)
                            {
                                newNode = treeViewMW.Nodes[numInList_hb].Nodes.Add("Новый параметр");
                                command2.Parameters.AddWithValue("@handbook_id", -1);
                                command2.Parameters.AddWithValue("@class_id", "");
                            }
                            else
                            {
                                newNode = treeViewMW.Nodes[numInList_hb].Nodes[0].Nodes.Add("Новый параметр");
                                command2.Parameters.AddWithValue("@handbook_id", index);
                                command2.Parameters.AddWithValue("@class_id", -1);
                            }
                        }
                        else
                        {
                            newNode = treeViewMW.Nodes[numInList_hb].Nodes[index_2].Nodes.Add("Новый параметр");
                            command2.Parameters.AddWithValue("@handbook_id", index);
                            command2.Parameters.AddWithValue("@class_id", index_2);
                        }
                        command2.ExecuteNonQuery();
                        connpar.Close();
                    }
                    treeViewMW.SelectedNode = newNode;
                    //selectedNode(newNode, treeViewMW);

                    flagQuestionSave = true;
                    treeViewCheck.Enabled = false;
                    //connpar.Close();
                }
            }
            catch { }
        }

        public void deleteParam(TreeNode nodeSelect)
        {
            try
            {
                int selectInpar_id = specificCell();

                //удали все лишнии узлы из тривью
                for (int i = 0; i < handbookList.Count; i++)
                {
                    for (int j = 0; j < treeViewMW.Nodes[i].Nodes.Count; j++)
                    {
                        removingNodes(i, j, nodeSelect);
                    }
                }
                using (var connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;"))
                {
                    connpar.Open();
                    string cmd = @"DELETE FROM LST_INPAR WHERE INPAR_ID=@inpar_id";
                    using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                    {
                        command.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                        command.ExecuteNonQuery();
                    }

                    string cmd2 = @"DELETE FROM sl_table WHERE INPAR_ID=@inpar_id";
                    using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                    {
                        command2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                        command2.ExecuteNonQuery();
                    }
                    connpar.Close();
                }
            }
            catch { }
        }
        #endregion

        #region сохранение изменений
        //метод для сохранения координат ранее выделенного узла
        public void coordinatsNewNodePar(TreeNode nodeNewSelect, out int indexPPNO, out int indexPNO, out int indexCNO)
        {
            indexPPNO = -1;
            indexPNO = -1;
            indexCNO = -1;
            if (nodeNewSelect.Parent.Parent != null)
            {
                indexPPNO = nodeNewSelect.Parent.Parent.Index;
                indexPNO = nodeNewSelect.Parent.Index;
                indexCNO = nodeNewSelect.Index;
            }
            else
            {
                indexPPNO = nodeNewSelect.Parent.Index;
                indexPNO = nodeNewSelect.Index;
            }
        }

        //сохранение параметров
        private bool saveParam()
        {
            try
            {
                ////редактируемый узел
                //TreeNode nodeNewSelect = new TreeNode();
                //nodeNewSelect = (TreeNode)nodeOldSelect.Clone();
                //int indexPPNO = -1;
                //int indexPNO = -1;
                //int indexCNO = -1;
                //coordinatsNewNodePar(nodeSelect, out indexPPNO, out indexPNO, out indexCNO);
                using (var connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;"))
                {
                    //selectInpar_id = specificCell();
                    string cmd = @"UPDATE LST_INPAR SET TYPE=@type, NAME=@name, SHORT=@short, MIN=@min, MAX=@max WHERE INPAR_ID=@inpar_id";
                    using (SQLiteCommand command = new SQLiteCommand(cmd, connpar))
                    {
                        if (txtName.Text != "" || txtShortName.Text != "")
                        {
                            if (radioButtonCb.Checked)
                            {
                                int type = cbEdit.Checked ? 1 : 0;
                                double min, max;
                                if (!cbEdit.Checked)
                                {
                                    dgvDbUpdate(selectInpar_id);
                                    command.Parameters.AddWithValue("@min", null);
                                    command.Parameters.AddWithValue("@max", null);
                                }
                                else
                                {
                                    sortDGV();
                                    if (minInf.Checked)
                                    {
                                        command.Parameters.AddWithValue("@min", "-9e9999");
                                    }
                                    else if (double.TryParse(txtMin.Text, out min))
                                    {
                                        command.Parameters.AddWithValue("@min", min);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Проверьте минимум.", "Неверно заполнены поля.", MessageBoxButtons.OK);
                                        return false;
                                    }

                                    if (maxInf.Checked)
                                    {
                                        command.Parameters.AddWithValue("@max", "9e9999");
                                    }
                                    else if (double.TryParse(txtMax.Text, out max))
                                    {
                                        command.Parameters.AddWithValue("@max", max);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Проверьте максимум.", "Неверно заполнены поля.", MessageBoxButtons.OK);
                                        return false;
                                    }
                                    dgvDbUpdate(selectInpar_id);
                                }
                                command.Parameters.AddWithValue("@type", type);
                            }
                            else
                            {
                                int type = tbInt.Checked ? 3 : 2;
                                int min, max;
                                float dmin, dmax;

                                if (tbInt.Checked && (Int32.TryParse(txtMin.Text, out min) || minInf.Checked) && (Int32.TryParse(txtMax.Text, out max) || maxInf.Checked))
                                {
                                    if (minInf.Checked)
                                    {
                                        command.Parameters.AddWithValue("@min", "-9e9999");
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@min", min);
                                    }

                                    if (maxInf.Checked)
                                    {
                                        command.Parameters.AddWithValue("@max", "9e9999");

                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@max", max);
                                    }
                                }
                                else if (!tbInt.Checked && (float.TryParse(txtMin.Text, out dmin) || minInf.Checked) && (float.TryParse(txtMax.Text, out dmax) || maxInf.Checked))
                                {
                                    if (minInf.Checked)
                                    {
                                        command.Parameters.AddWithValue("@min", "-9e9999");
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@min", dmin);
                                    }

                                    if (maxInf.Checked)
                                    {
                                        command.Parameters.AddWithValue("@max", "9e9999");
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@max", dmax);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Проверьте минимум и максимум.", "Неверно заполнены поля.", MessageBoxButtons.OK);
                                    return false;
                                }
                                dgvDel(selectInpar_id);
                                command.Parameters.AddWithValue("@type", type);
                            }
                            command.Parameters.AddWithValue("@name", txtName.Text);
                            command.Parameters.AddWithValue("@short", txtShortName.Text);
                            command.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                            connpar.Open();
                            command.ExecuteNonQuery();
                            connpar.Close();
                        }
                        else
                        {
                            MessageBox.Show("Есть незаполненные поля.");
                            return false;
                        }
                    }
                }
                if (nodeOldSelect.Text != txtName.Text)
                    updateTextInTreeView(nodeOldSelect);
                flagQuestionSave = false;
                flagIzmenenie = true;
                //treeViewMW.SelectedNode = null;
                MessageBox.Show("SAVE");
                treeViewCheck.Enabled = true;
                //if (indexCNO != -1)
                //    treeViewMW.SelectedNode = treeViewMW.Nodes[indexPPNO].Nodes[indexPNO].Nodes[indexCNO];
                //else
                //    treeViewMW.SelectedNode = treeViewMW.Nodes[indexPPNO].Nodes[indexPNO];
            }
            catch (Exception ex) { MessageBox.Show(ex + "");}
            return true;
        }

        private void dgvDel(int selectInpar_id)
        {
            try
            {
                using (var connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;"))
                {
                    connpar.Open();
                    string cmdDel = @"DELETE FROM LST_ITEM WHERE INPAR_ID = @inpar_id";
                    using (SQLiteCommand commandDel = new SQLiteCommand(cmdDel, connpar))
                    {
                        commandDel.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                        commandDel.ExecuteNonQuery();
                    }
                    string cmdDel2 = @"DELETE FROM sl_itemid WHERE INPAR_ID = @inpar_id";
                    using (SQLiteCommand commandDel2 = new SQLiteCommand(cmdDel2, connpar))
                    {
                        commandDel2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                        commandDel2.ExecuteNonQuery();
                    }
                    connpar.Close();
                }
                while (dataGridView.Rows.Count != 1)
                {
                    dataGridView.Rows.RemoveAt(0);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex + ""); }
        }

        private void dgvDbUpdate(int selectInpar_id)
        {
            try
            {
                int type;
                using (var connpar2 = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;Read only =true;", true))
                {
                    connpar2.Open();
                    //получение количества строк в базе и дгв для стравнения
                    string cmdSel = @"SELECT COUNT(*) FROM LST_ITEM WHERE INPAR_ID = @inpar_id";
                    using (SQLiteCommand commandSel = new SQLiteCommand(cmdSel, connpar2))
                    {
                        commandSel.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                        using (SQLiteDataReader rdrSel = commandSel.ExecuteReader())
                        {
                            rdrSel.Read();
                            this.countItemDB = Int32.Parse(rdrSel[0].ToString());
                            rdrSel.Close();
                        }
                    }
                    string cmdSel2 = @"SELECT TYPE FROM LST_INPAR WHERE INPAR_ID = @inpar_id";
                    using (SQLiteCommand commandSel2 = new SQLiteCommand(cmdSel2, connpar2))
                    {
                        commandSel2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                        using (SQLiteDataReader rdrSel2 = commandSel2.ExecuteReader())
                        {
                            rdrSel2.Read();
                            type = Int32.Parse(rdrSel2[0].ToString());
                            rdrSel2.Close();
                        }
                    }
                    connpar2.Close();
                }
                using (var connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;"))
                {
                    connpar.Open();
                    if ((type == 0 && !cbEdit.Checked) || (type == 1 && cbEdit.Checked))
                    {
                        if (countItemDB == 0)
                        {
                            string cmdIns = @"INSERT INTO LST_ITEM (INPAR_ID, ITEM_NAME, ITEM_ID, ITEM_POSITION) VALUES (@inpar_id,@item_name,@item_id,@item_position)";
                            using (SQLiteCommand commandIns = new SQLiteCommand(cmdIns, connpar))
                            {
                                commandIns.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                                if (!cbEdit.Checked)
                                {
                                    for (int i = 0; i < dataGridView.RowCount - 1; i++)
                                    {
                                        commandIns.Parameters.AddWithValue("@item_name", dataGridView.Rows[i].Cells[0].Value.ToString());
                                        commandIns.Parameters.AddWithValue("@item_id", i + 1);
                                        commandIns.Parameters.AddWithValue("@item_position", i + 1);
                                        commandIns.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < dataGridView.RowCount - 1; i++)
                                    {
                                        commandIns.Parameters.AddWithValue("@item_name", dataGridView.Rows[i].Cells[0].Value.ToString());
                                        commandIns.Parameters.AddWithValue("@item_id", null);
                                        commandIns.Parameters.AddWithValue("@item_position", null);
                                        commandIns.ExecuteNonQuery();
                                    }
                                }
                            }
                            if (dataGridView.RowCount != 1)
                            {
                                string cmdIns2 = @"INSERT INTO sl_itemid (INPAR_ID, LAST_ITEMID) VALUES (@inpar_id,@last_itemid)";
                                using (SQLiteCommand commandIns2 = new SQLiteCommand(cmdIns2, connpar))
                                {
                                    commandIns2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                                    commandIns2.Parameters.AddWithValue("@last_itemid", dataGridView.RowCount - 1);
                                    commandIns2.ExecuteNonQuery();
                                }
                            }
                        }
                        else
                        {
                            GC.Collect();
                            if (this.countItemDB < (dataGridView.RowCount - 1))
                            {
                                //сравнить данные и добавить недостающие строки
                                string cmdIns = @"INSERT INTO LST_ITEM (INPAR_ID, ITEM_ID) VALUES (@inpar_id,@item_id)";
                                using (SQLiteCommand commandIns = new SQLiteCommand(cmdIns, connpar))
                                {
                                    commandIns.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                                    for (int index = this.countItemDB; index < dataGridView.RowCount - 1; index++)
                                    {
                                        if (!cbEdit.Checked)
                                            commandIns.Parameters.AddWithValue("item_id", index);
                                        else
                                            commandIns.Parameters.AddWithValue("item_id", null);
                                        commandIns.ExecuteNonQuery();
                                    }
                                }
                            }
                            else if (this.countItemDB > (dataGridView.RowCount - 1))
                            {
                                int delCol = this.countItemDB - dataGridView.RowCount + 1;
                                //сравнить данные и удалить лишнюю строку
                                string cmdDel = @"DELETE FROM LST_ITEM WHERE rowid IN (SELECT rowid FROM LST_ITEM WHERE INPAR_ID = @inpar_id ORDER BY rowid DESC LIMIT @delCol)";
                                //string cmdDel = @"DELETE FROM LST_ITEM WHERE INPAR_ID = @inpar_id LIMIT @delCol";
                                using (SQLiteCommand commandDel = new SQLiteCommand(cmdDel, connpar))
                                {
                                    commandDel.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                                    commandDel.Parameters.AddWithValue("@delCol", delCol);
                                    //GC.Collect();
                                    commandDel.ExecuteNonQuery();
                                }
                            }
                            upDbWhenEqually(selectInpar_id);
                            if (dataGridView.RowCount != 1)
                            {
                                //обновить данные в таблице с сл_итемид
                                string cmdUp = @"UPDATE sl_itemid SET LAST_ITEMID=@last_itemid WHERE INPAR_ID=@inpar_id";
                                using (SQLiteCommand commandUp = new SQLiteCommand(cmdUp, connpar))
                                {
                                    commandUp.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                                    commandUp.Parameters.AddWithValue("@last_itemid", dataGridView.RowCount - 1);
                                    commandUp.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                string cmdDel2 = @"DELETE FROM sl_itemid WHERE INPAR_ID = @inpar_id";
                                using (SQLiteCommand commandDel2 = new SQLiteCommand(cmdDel2, connpar))
                                {
                                    commandDel2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                                    commandDel2.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    else
                    {
                        string cmdDel = @"DELETE FROM LST_ITEM WHERE INPAR_ID = @inpar_id";
                        using (SQLiteCommand commandDel = new SQLiteCommand(cmdDel, connpar))
                        {
                            commandDel.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                            commandDel.ExecuteNonQuery();
                        }
                        string cmdDel2 = @"DELETE FROM sl_itemid WHERE INPAR_ID = @inpar_id";
                        using (SQLiteCommand commandDel2 = new SQLiteCommand(cmdDel2, connpar))
                        {
                            commandDel2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                            commandDel2.ExecuteNonQuery();
                        }

                        string cmdIns = @"INSERT INTO LST_ITEM (INPAR_ID, ITEM_NAME, ITEM_ID, ITEM_POSITION) VALUES (@inpar_id,@item_name,@item_id,@item_position)";
                        using (SQLiteCommand commandIns = new SQLiteCommand(cmdIns, connpar))
                        {
                            commandIns.Parameters.AddWithValue("@inpar_id", selectInpar_id);

                            if (!cbEdit.Checked)
                            {
                                for (int i = 0; i < dataGridView.RowCount - 1; i++)
                                {
                                    commandIns.Parameters.AddWithValue("@item_name", dataGridView.Rows[i].Cells[0].Value.ToString());
                                    commandIns.Parameters.AddWithValue("@item_id", i + 1);
                                    commandIns.Parameters.AddWithValue("@item_position", i + 1);
                                    commandIns.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dataGridView.RowCount - 1; i++)
                                {
                                    commandIns.Parameters.AddWithValue("@item_name", dataGridView.Rows[i].Cells[0].Value.ToString());
                                    commandIns.Parameters.AddWithValue("@item_id", null);
                                    commandIns.Parameters.AddWithValue("@item_position", null);
                                    commandIns.ExecuteNonQuery();
                                }
                            }
                        }
                        string cmdIns2 = @"INSERT INTO sl_itemid (INPAR_ID, LAST_ITEMID) VALUES (@inpar_id,@last_itemid)";
                        using (SQLiteCommand commandIns2 = new SQLiteCommand(cmdIns2, connpar))
                        {
                            commandIns2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                            commandIns2.Parameters.AddWithValue("@last_itemid", dataGridView.RowCount - 1);
                            commandIns2.ExecuteNonQuery();
                        }
                    }       
                    connpar.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex + ""); }
        }

        private void upDbWhenEqually(int selectInpar_id)
        {                        
            int rowid;
            string name, position;
            try
            {
                using (var connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;"))
                {
                    connpar.Open();
                    //обновление lst_item из dgv
                    string cmdUp = @"UPDATE LST_ITEM SET ITEM_NAME=@item_name, ITEM_POSITION=@position WHERE rowid=@rowid";
                    using (SQLiteCommand commandUp = new SQLiteCommand(cmdUp, connpar))
                    {
                        //данные dgv в базе
                        string cmdSel2 = @"SELECT rowid, ITEM_NAME, ITEM_POSITION FROM LST_ITEM WHERE INPAR_ID=@inpar_id LIMIT @limit,1";
                        using (SQLiteCommand commandSel2 = new SQLiteCommand(cmdSel2, connpar))
                        {
                            commandSel2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                            //проверить данные 
                            for (int i = 0; i < dataGridView.RowCount - 1; i++)
                            {
                                commandSel2.Parameters.AddWithValue("@limit", i);

                                using (SQLiteDataReader rdrSel2 = commandSel2.ExecuteReader())
                                {
                                    rdrSel2.Read();
                                    rowid = Int32.Parse(rdrSel2[0].ToString());
                                    name = rdrSel2[1].ToString();
                                    position = rdrSel2[2].ToString();
                                    rdrSel2.Close();
                                }
                                if (position != "")
                                {
                                    if (name != dataGridView.Rows[i].Cells[0].Value.ToString())// && Int32.Parse(position) != i + 1)
                                    {
                                        commandUp.Parameters.AddWithValue("@position", i + 1);
                                        commandUp.Parameters.AddWithValue("@rowid", rowid);
                                        commandUp.Parameters.AddWithValue("@item_name", dataGridView.Rows[i].Cells[0].Value.ToString());
                                        commandUp.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    if (name != dataGridView.Rows[i].Cells[0].Value.ToString())
                                    {
                                        commandUp.Parameters.AddWithValue("@position", null);
                                        commandUp.Parameters.AddWithValue("@rowid", rowid);
                                        commandUp.Parameters.AddWithValue("@item_name", dataGridView.Rows[i].Cells[0].Value.ToString());
                                        commandUp.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                    connpar.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex + ""); }
        }

        private void updateTextInTreeView(TreeNode nodeOld)
        {
            try
            {
                using (var connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;Read Only=True", true))
                {
                    //обновление текста в тривью после изменения имени параметра
                    string cmd2 = @"SELECT *FROM sl_table WHERE inpar_id=@inpar_id";
                    using (SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar))
                    {
                        command2.Parameters.AddWithValue("@inpar_id", selectInpar_id);

                        treeViewMW.SelectedNode = null;
                        connpar.Open();

                        int numInList_hb;
                        TreeNode nodeFuture = new TreeNode();
                        nodeFuture.Text = txtName.Text;
                        using (SQLiteDataReader rdr = command2.ExecuteReader())
                        {
                            try
                            {
                                while (rdr.Read())
                                {
                                    //int hb_id = Int32.Parse(rdr[1].ToString());
                                    int class_id = Int32.Parse(rdr[2].ToString());
                                    tvmwList_hb_id.TryGetValue(Int32.Parse(rdr[1].ToString()), out numInList_hb);
                                    if (numInList_hb == -1)
                                    {
                                        foreach (TreeNode item in treeViewMW.Nodes[0].Nodes)
                                        {
                                            if (item.Text.Equals(nodeOld.Text))
                                            {
                                                treeViewMW.Nodes[0].Nodes.Insert(item.Index, (TreeNode)nodeFuture.Clone());
                                                item.Remove();
                                                break;
                                            }
                                        }
                                    }
                                    //при удалении параметра из класса при выделенном главном чеке 
                                    else if (class_id == -1)// && hb_id == nodeCheck.Parent.Index)
                                    {
                                        foreach (TreeNode item in treeViewMW.Nodes[numInList_hb].Nodes[0].Nodes)
                                        {
                                            if (item.Text.Equals(nodeOld.Text))
                                            {
                                                treeViewMW.Nodes[numInList_hb].Nodes[0].Nodes.Insert(item.Index, (TreeNode)nodeFuture.Clone());
                                                item.Remove();
                                                break;
                                            }
                                        }
                                    }
                                    //обычное удалении из класса
                                    else //if (hb_id == nodeCheck.Parent.Index)
                                    {
                                        foreach (TreeNode item in treeViewMW.Nodes[numInList_hb].Nodes[class_id].Nodes)
                                        {
                                            if (item.Text.Equals(nodeOld.Text))
                                            {
                                                treeViewMW.Nodes[numInList_hb].Nodes[class_id].Nodes.Insert(item.Index, (TreeNode)nodeFuture.Clone());
                                                item.Remove();
                                                //treeViewMW.Nodes[hb_id].Nodes[class_id].Nodes.Add(nodeFuture.Text);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                rdr.Close();
                                connpar.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex + ""); }
        }
        #endregion

        #region кнопки
        private void buttonAddParam_Click(object sender, EventArgs e)
        {
            newParam();
        }

        private void buttonSaveParam_Click(object sender, EventArgs e)
        {
            flagIzmenenie = saveParam();
        }

        private void buttonCancelParam_Click(object sender, EventArgs e)
        {
            flagQuestionSave = false;
            flagIzmenenie = true;
            treeViewMW.SelectedNode = null;
            treeViewMW.SelectedNode = nodeSelect;
            treeViewMW.Focus();
        }

        private void cleanWorkingPanel()
        {
            radioButtonCb.Checked = true;
            panelForData.Visible = true;
            txtName.Text = null;
            txtShortName.Text = null;
            txtMin.Text = null;
            txtMax.Text = null;
            cbEdit.Checked = false;
            dataGridView.Rows.Clear();
        }

        //private void btnAddRow_Click(object sender, EventArgs e)
        //{
        //    dataGridView.Rows.Add();
        //}
        //private void btnRemoveRow_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        foreach (DataGridViewCell dr in dataGridView.SelectedCells)
        //        {
        //            dataGridView.Rows.Remove(dr.OwningRow);
        //        }
        //    }
        //    catch { }
        //}
        //private void btnSort_Click(object sender, EventArgs e)
        //{
        //    sortDGV();
        //}

        //перемещение стрелками
        private void transposition(int offset)
        {
            DataGridViewRow row = dataGridView.CurrentRow;
            if (row != null)
            {
                if (row.Index == 0 && offset == -1 || ((row.Index == dataGridView.NewRowIndex - 1)
                    && offset == 1 || row.Index == dataGridView.NewRowIndex))//(row.Index == 0 || row.Index == dataGridView.NewRowIndex - 1 || row.Index == dataGridView.NewRowIndex)
                    return;

                // Получаем текущий индекс строки
                int currentIndex = row.Index;
                // Удаляем ее из коллекции
                dataGridView.Rows.Remove(row);
                // А теперь добавляем со смещением
                dataGridView.Rows.Insert(currentIndex + offset, row);
                try
                {
                    //выделяем перемещенную строку как активную
                    dataGridView.CurrentCell = dataGridView[0, row.Index];
                }
                catch { }
            }
        }
        private void btnUp_Click(object sender, EventArgs e)
        {
            int offset = -1;
            transposition(offset);
        }
        private void btnDown_Click(object sender, EventArgs e)
        {
            int offset = 1;
            transposition(offset);
        }
        #endregion

        #region панелька, описание всех внутренних ..боксов
        //вывод значений параметров на панель 
        public void workingWihtTable()
        {
            try
            {
                cleanWorkingPanel();
                using (var connpar = new SQLiteConnection("Data source=" + filename_inp_path + ";Version=3; Read Only=True;", true))
                {
                    connpar.Open();
                    string cmd = @"SELECT * FROM LST_INPAR WHERE LST_INPAR.INPAR_ID =@inpar_id";// +treeViewMW.SelectedNode.Name;
                    SQLiteCommand com = new SQLiteCommand(cmd, connpar);
                    com.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                    using (SQLiteDataReader rdr = com.ExecuteReader())
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                int type = int.Parse(rdr["TYPE"].ToString());
                                if (type < 2)
                                {
                                    radioButtonCb.Checked = true;
                                    txtName.Text = rdr["NAME"].ToString();
                                    txtShortName.Text = rdr["SHORT"].ToString();
                                    if (type == 0)
                                    {
                                        cbEdit.Checked = false;

                                        minInf.Checked = false;
                                        txtMin.Enabled = false;
                                        txtMin.Text = "";
                                        minInf.Enabled = false;

                                        maxInf.Checked = false;
                                        txtMax.Enabled = false;
                                        txtMax.Text = "";
                                        maxInf.Enabled = false;
                                    }
                                    else
                                    {
                                        cbEdit.Checked = true;

                                        txtMin.Enabled = true;
                                        minInf.Enabled = true;
                                        txtMax.Enabled = true;
                                        maxInf.Enabled = true;

                                        txtMin.Text = rdr["MIN"].ToString();
                                        if (txtMin.Text == "-∞")
                                        {
                                            minInf.Checked = true;
                                            txtMin.Enabled = false;
                                        }
                                        txtMax.Text = rdr["MAX"].ToString();
                                        if (txtMax.Text == "∞")
                                        {
                                            maxInf.Checked = true;
                                            txtMax.Enabled = false;
                                        }
                                    }

                                    //получение количества строк в базе и дгв для стравнения
                                    string cmdCount = @"SELECT COUNT(*) FROM LST_ITEM WHERE INPAR_ID = @inpar_id";
                                    using (SQLiteCommand commandCount = new SQLiteCommand(cmdCount, connpar))
                                    {
                                        commandCount.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                                        using (SQLiteDataReader rdrCount = commandCount.ExecuteReader())
                                        {
                                            rdrCount.Read();
                                            this.countItemDB = Int32.Parse(rdrCount[0].ToString());
                                            rdrCount.Close();
                                        }
                                    }
                                    if (countItemDB != 0)
                                        dataGridView.Rows.Add(this.countItemDB);

                                    //string cmdpar = @"SELECT LST_ITEM.ITEM_NAME FROM LST_ITEM WHERE LST_ITEM.INPAR_ID =@item_id"; //+ treeViewMW.SelectedNode.Name;
                                    string cmdpar = @"SELECT *FROM LST_ITEM WHERE LST_ITEM.INPAR_ID =@item_id"; //+ treeViewMW.SelectedNode.Name;
                                    using (SQLiteCommand compar = new SQLiteCommand(cmdpar, connpar))
                                    {
                                        compar.Parameters.AddWithValue("@item_id", selectInpar_id);
                                        using (SQLiteDataReader rdrpar = compar.ExecuteReader())
                                        {
                                            try
                                            {
                                                int index = 1;
                                                while (rdrpar.Read())
                                                {
                                                    if (rdrpar["ITEM_ID"].ToString() != "" && rdrpar["ITEM_POSITION"].ToString() != "")
                                                    {
                                                        index = Int32.Parse(rdrpar["ITEM_POSITION"].ToString());
                                                    }
                                                    dataGridView.Rows[index - 1].Cells[0].Value = rdrpar["ITEM_NAME"].ToString();
                                                    index++;
                                                }
                                            }
                                            finally
                                            {
                                                rdrpar.Close();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    radioButtonTb.Checked = true;
                                    txtName.Text = rdr["NAME"].ToString();
                                    txtShortName.Text = rdr["SHORT"].ToString();

                                    txtMin.Enabled = true;
                                    minInf.Enabled = true;
                                    txtMax.Enabled = true;
                                    maxInf.Enabled = true;
                                    txtMin.Text = rdr["MIN"].ToString();
                                    if (txtMin.Text == "-∞")
                                    {
                                        minInf.Checked = true;
                                        txtMin.Enabled = false;
                                    }
                                    txtMax.Text = rdr["MAX"].ToString();
                                    if (txtMax.Text == "∞")
                                    {
                                        maxInf.Checked = true;
                                        txtMax.Enabled = false;
                                    }
                                    if (type == 2)
                                        tbInt.Checked = false;
                                    else
                                        tbInt.Checked = true;
                                }
                            }
                        }
                        finally
                        {
                            rdr.Close();
                        }
                    }
                    connpar.Close();
                }
            }
            catch { }
        }

        private void radioButtonCb_CheckedChanged(object sender, EventArgs e)
        {
            //общее
            panelForData.Visible = true;
            if (!cbEdit.Checked)
            {
                txtMin.Enabled = false;
                minInf.Enabled = false;
                txtMax.Enabled = false;
                maxInf.Enabled = false;

                btnUp.Enabled = true;
                btnDown.Enabled = true;
                btnSort.Enabled = false;
            }
            cbPanel.Visible = true;

            tbInt.Visible = false;
        }

        private void radioButtonTb_CheckedChanged(object sender, EventArgs e)
        {
            //общее
            panelForData.Visible = true;
            txtMin.Enabled = true;
            minInf.Enabled = true;
            txtMax.Enabled = true;
            maxInf.Enabled = true;
            tbInt.Visible = true;
            cbPanel.Visible = false;
        }

        private void editEnabled()
        {
            if (cbEdit.Checked)
            {
                txtMin.Enabled = true;
                minInf.Enabled = true;
                txtMax.Enabled = true;
                maxInf.Enabled = true;
                btnUp.Enabled = false;
                btnDown.Enabled = false;
                btnSort.Enabled = true;
            }
            else
            {
                minInf.Enabled = false;
                minInf.Checked = false;
                txtMin.Enabled = false;
                txtMin.Text = "";

                maxInf.Enabled = false;
                maxInf.Checked = false;
                txtMax.Enabled = false;
                txtMax.Text = "";
                btnUp.Enabled = true;
                btnDown.Enabled = true;
                btnSort.Enabled = false;
            }
        }

        private void cbEdit_CheckedChanged(object sender, EventArgs e)
        {
            editEnabled();
        }

        private void txtMinInf_CheckedChanged(object sender, EventArgs e)
        {
            if (minInf.Checked)
            {
                txtMin.Enabled = false;
                txtMin.Text = "-∞";
            }
            else
            {
                txtMin.Enabled = true;
                txtMin.Text = "";
            }
        }

        private void txtMaxInf_CheckedChanged(object sender, EventArgs e)
        {
            if (maxInf.Checked)
            {
                txtMax.Enabled = false;
                txtMax.Text = "∞";
            }
            else
            {
                txtMax.Enabled = true;
                txtMax.Text = "";
            }
        }
        #endregion

        #region dgvSort
        //Boolean asc;
        private void sortDGV()
        {
            try
            {
                //удаление пустых строк
                for (int i = 0; i < dataGridView.RowCount - 1; i++)
                {
                    if (dataGridView.Rows[i].Cells[0].Value == null)
                    {
                        dataGridView.Rows.RemoveAt(i);
                        i--;
                    }
                }
                //bubble sort
                for (int j = 0; j < dataGridView.RowCount - 1; j++)
                    for (int i = 0; i < dataGridView.RowCount - 2; i++)
                    {
                        var a = float.Parse(dataGridView.Rows[i].Cells[0].Value.ToString());
                        var b = float.Parse(dataGridView.Rows[i + 1].Cells[0].Value.ToString());
                        if (a > b)
                        {
                            object temp = dataGridView.Rows[i].Cells[0].Value;
                            dataGridView.Rows[i].Cells[0].Value = dataGridView.Rows[i + 1].Cells[0].Value;
                            dataGridView.Rows[i + 1].Cells[0].Value = temp;
                        }
                    }
            }
            catch { }
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (cbEdit.Checked && radioButtonCb.Checked)
                sortDGV();
        }    
        #endregion
    }

    #region доп класс для сортировки 
    public class Sort : IComparer
    {
        //Направление сортировки
        int asc;
        public Sort(bool asc)
        {
            //True - прямая; False - обратная
            this.asc = asc ? 1 : -1;
        }
        #region IComparer Members

        public int Compare(object x, object y)
        {
            //(?<Word>[\s\S-[0-9]]*) - Поиск всего кроме цифр и (?<Digit>[0-9]*) - цифры
            string pattern = @"((?<Word>[\s\S-[0-9]]*)(?<Digit>[0-9]*))";
            //Находим соответствия
            MatchCollection nc1 = Regex.Matches(((DataGridViewRow)x).Cells[0].Value.ToString(), pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection nc2 = Regex.Matches(((DataGridViewRow)y).Cells[0].Value.ToString(), pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            for (int z = 0; z < nc1.Count; z++)
            {
                // (-1) - pivot > y
                // (0)  - pivot = y
                // (1)  - pivot < y
                if (z >= nc2.Count)
                { return -1 * asc; }

                //Сравниваем символьную составляющую
                int res = nc1[z].Groups["Word"].Value.CompareTo(nc2[z].Groups["Word"].Value);
                if (res != 0)
                { return asc * res; }

                int num1;
                int num2;
                bool b1 = int.TryParse(nc1[z].Groups["Digit"].Value, out num1);
                bool b2 = int.TryParse(nc2[z].Groups["Digit"].Value, out num2);

                //Числа, в конце строки, не найдены у обоих
                if (!b1 && !b2)
                { return 0; }

                //У pivot или y в конце нет числа
                if (!b1 || !b2)
                { return -1 * asc; }

                //Если есть, то сравниваем их
                res = num1.CompareTo(num2);
                if (res != 0)
                { return asc * res; }
            }

            return 0;
        }

        #endregion
    }
    #endregion
}