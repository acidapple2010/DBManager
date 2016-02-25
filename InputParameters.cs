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
        sqliteclass sqlite = new sqliteclass();
        public string filename_inp_path = @"../../../inpar_Kulygin_2.sqlite";
        public DataSet dsPar { get; set; }
        //public DataSet dsPar2 { get; set; }
        public SQLiteConnection connpar { get; set; }
        public SQLiteCommand command { get; set; }
        string cmd { get; set; }
        //string strElem = null;
        //для панели, где отображаются значения параметров
        int selectInpar_id { get; set; }

        //список справочников
        public List<string> handbookList;
        //список баз данных
        public List<DataSet> dslist;
        //public List<ListLocal> listGlobalJeneralParam;
        //public List<ListLocal> listLocalJeneralParam;        
        TreeView treeViewMW = new TreeView();
        TreeNode nodeSelect = new TreeNode();
        TreeNode nodeCheck = new TreeNode();
        //TreeNode checkSelect { get; set; }
        //новый параметр
        TreeNode newNode;
        //TreeNode nodeJeneralPar = new TreeNode();
        //public int kolichObParam { get; set; }
        //флаг нажатия на чекбокс(событие) 
        Boolean flagCheck;
        //для повторного выделения
        //Boolean flagClickJeneralCheck { get; set; }
        //int flagGlob { get; set; }
        //int flagLoc { get; set; }
        //для открытия диалогового окна 
        Boolean flagQuestionSave = false;

        const string message = "Сохранить новый параметр перед выходом?";
        const string caption = "Вопрос на миллион";
        //public delegate void pererisovka();
        //public event pererisovka signal;

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
        }
        #endregion

        #region методы мввью
        internal void createTreeViewMW(List<string> handbookList, TreeView treeViewMW)
        {
            //открытие всех баз данных и добавление в список
            dsPar = sqlite.dataSetParamLoader(filename_inp_path);
            connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;");
            dslist = new List<DataSet>();
            for (int i = 0; i < handbookList.Count; i++)
            {
                dslist.Add(sqlite.dataSetLoader(handbookList[i]));
            }
            //принимаем все значения из мэйнвиндоу 
            this.handbookList = handbookList;
            this.treeViewMW = treeViewMW;
            //listGlobalJeneralParam = new List<ListLocal>();
            //listLocalJeneralParam = new List<ListLocal>();
            //constrJeneralParam2();
            //constrJeneralParam();
            createrTVMW();
            createTreeViewCheck();
            //proverkaDlyaObnov();
            otobrajenie01();
        }

        //public void proverkaDlyaObnov() 
        //{
        //    // выделение элемента после обновления 
        //    if (nodeSelect != null)
        //    {
        //        //проверка для перерисовки у справочников с лок парам
        //        if (nodeSelect.Parent.Parent != null)
        //        {      
        //            foreach (TreeNode item in treeViewMW.Nodes[nodeSelect.Parent.Parent.Index].Nodes[nodeSelect.Parent.Index].Nodes)
        //            {
        //                if (item.Text == nodeSelect.Text)
        //                {
        //                    selectionAfterUpdating(nodeSelect.Parent.Parent.Index);
        //                    return;
        //                }
        //            }
        //            if (flagClickJeneralCheck == true)
        //            {
        //                selectionAfterUpdating(nodeSelect.Parent.Parent.Index);
        //                flagClickJeneralCheck = false;
        //                return;
        //            }
        //        }
        //    }
        //}

        ////создаем списки из справочников с общими параметрами 
        //public void constrJeneralParam()
        //{
        //    int inpar_id = 1;
        //    foreach (DataRow dr in dsPar.Tables["LST_INPAR"].Rows)
        //    {
        //        string inpar_name = dr["NAME"].ToString();

        //        int kolich_sprav = handbookList.Count;
        //        XDocument xd = XDocument.Parse(dr["sl_valid"].ToString());
        //        foreach (XElement xel in xd.Descendants("hb"))
        //        {
        //            int number_sp = Int32.Parse(xel.Attribute("id").Value);
        //            int kolichClass = dslist[number_sp - 1].Tables["LST_CLASS"].Rows.Count;
        //            //количество класс_ид в таблице сл_валид и определенной хд
        //            int kol_id = (from s in xel.Descendants("class") select s).Count();
        //            if (kol_id == kolichClass)
        //            {
        //                kolich_sprav--;
        //                if (kolich_sprav == 0)
        //                {
        //                    //перед добавлением в список глобальных, удаляем сначала все из локального
        //                    listLocalJeneralParam.RemoveAll(item => item.inpar_id == inpar_id);

        //                    ListLocal listlocal = new ListLocal(inpar_id, inpar_name);
        //                    listGlobalJeneralParam.Add(listlocal);
        //                    break;
        //                }
        //                else
        //                {
        //                    //создаем переменную с номером справочника и параметром, который есть во всех классах и записываем его в список
        //                    ListLocal listlocal = new ListLocal(inpar_id, number_sp, inpar_name);
        //                    listLocalJeneralParam.Add(listlocal);
        //                }
        //            }
        //        }
        //        inpar_id++;
        //    }
        //}

        //public void constrJeneralParam2()
        //{
        //    foreach (DataRow dr in dsPar.Tables["sl_table"].Rows)
        //    {
        //        int inpar_id = Int32.Parse(dr["inpar_id"].ToString());
        //        int handbook_id = Int32.Parse(dr["handbook_id"].ToString());

        //        if (Int32.Parse(dr["handbook_id"].ToString()) == -1)
        //            listGlobalJeneralParam.Add(new ListLocal(inpar_id, dr["handbook_id"].ToString()));

        //        else if (Int32.Parse(dr["class_id"].ToString()) == -1)
        //            listLocalJeneralParam.Add(new ListLocal(inpar_id, handbook_id, dr["class_id"].ToString()));
        //    }
        //}

        //построение тривью

        public void createrTVMW()
        {
            treeViewMW.Nodes.Clear();
            //TreeNode globGPar = new TreeNode();
            //globGPar.Text = "Общие параметры";
            treeViewMW.Nodes.Add("Общие параметры");

            //foreach (var item in listGlobalJeneralParam)
            //{
            //    treeViewMW.Nodes[0].Nodes.Add(item.inpar_name.ToString());
            //}

            for (int i = 0; i < handbookList.Count; i++)
            {
                treeViewMW.Nodes.Add(handbookList[i]);
                //TreeNode locGPar = new TreeNode();
                //locGPar.Text = "Общие параметры";
                treeViewMW.Nodes[i + 1].Nodes.Add("Общие параметры");

                foreach (DataRow dr in dslist[i].Tables["LST_CLASS"].Rows)
                {
                    treeViewMW.Nodes[i + 1].Nodes.Add(dr["CLASS_ID"].ToString(), dr["CLASS_NAME"].ToString());
                }
            }
            constrParam2();
            treeViewMW.Nodes[1].Expand();
            //foreach (DataRow drGroup in dsPar.Tables["LST_INPAR"].Rows)
            //{
            //    ////проверка на совпадение из списка общих параметров глобальных или локальных  
            //    ListLocal lst = new ListLocal(Int32.Parse(drGroup["INPAR_ID"].ToString()), 0, drGroup["NAME"].ToString());
            //    Boolean flag1 = true;
            //    Boolean flag2 = true;
            //    foreach (var item in listGlobalJeneralParam)
            //    {
            //        if (item.inpar_id == lst.inpar_id && flag1)
            //        {
            //            flag1 = false;
            //        }
            //    }
            //    foreach (var item2 in listLocalJeneralParam)
            //    {
            //        if ((hbIndex + 1) == item2.number_sp && item2.inpar_id == lst.inpar_id)
            //        {
            //            flag2 = false;
            //            treeViewMW.Nodes[hbIndex + 1].Nodes[0].Nodes.Add(item2.inpar_name.ToString());
            //        }
            //    }
            //    if (flag1 && flag2)
            //        constrParam(hbIndex, drGroup);
            //}
        }

        //заполняет определенный справочник своими параметрами
        public void constrParam2()
        {
            foreach (DataRow dr in dsPar.Tables["sl_table"].Rows)
            {
                //переменные из таблицы sl_table, будем использовать для таблицы lst_inpar, чтобы взять данные
                int class_id = Int32.Parse(dr["class_id"].ToString());
                int inpar_id = Int32.Parse(dr["inpar_id"].ToString());
                int handbook_id = Int32.Parse(dr["handbook_id"].ToString());
                //MessageBox.Show(dsPar.Tables["LST_INPAR"].Rows[inpar_id - 1]["NAME"].ToString() + " " + inpar_id + " " + handbook_id + " " + class_id);

                if (handbook_id == -1)
                {
                    treeViewMW.Nodes[0].Nodes.Add(dsPar.Tables["LST_INPAR"].Rows[inpar_id - 1]["NAME"].ToString());
                    //listGlobalJeneralParam.Add(new ListLocal(inpar_id, dr["handbook_id"].ToString()));
                }

                else if (class_id == -1)
                {
                    treeViewMW.Nodes[handbook_id].Nodes[0].Nodes.Add(dsPar.Tables["LST_INPAR"].Rows[inpar_id - 1]["NAME"].ToString());
                    //listLocalJeneralParam.Add(new ListLocal(inpar_id, handbook_id, dr["class_id"].ToString()));
                }

                else
                    treeViewMW.Nodes[handbook_id].Nodes[class_id].Nodes.Add(dsPar.Tables["LST_INPAR"].Rows[inpar_id - 1]["NAME"].ToString());
            }
        }
        #endregion

        int countItemDB {get; set;}

        int countDGV { get; set; }
        int typeDGV { get; set; }

        private void proverkaNaIzmenenie()
        {
            int inpar_id = specificCell();
            if (inpar_id != 0)
            {
                string cmd = @"SELECT *FROM LST_INPAR WHERE INPAR_ID = @inpar_id";
                SQLiteCommand command = new SQLiteCommand(cmd, connpar);
                command.Parameters.AddWithValue("@inpar_id", inpar_id);
                connpar.Open();
                SQLiteDataReader rdr = command.ExecuteReader();
                rdr.Read();
                string type = rdr[1].ToString();
                string name = rdr[2].ToString();
                string shortName = rdr[3].ToString();
                string min = rdr[4].ToString();
                string max = rdr[5].ToString();
                rdr.Close();
                if (radioButtonCb.Checked)
                {
                    //получение количества строк в базе и дгв для стравнения
                    string cmd3 = @"SELECT COUNT(*) FROM LST_ITEM WHERE INPAR_ID = @inpar_id";
                    SQLiteCommand command3 = new SQLiteCommand(cmd3, connpar);
                    command3.Parameters.AddWithValue("@inpar_id", inpar_id);
                    SQLiteDataReader rdr3 = command3.ExecuteReader();
                    rdr3.Read();
                    this.countItemDB = Int32.Parse(rdr3[0].ToString());
                    rdr3.Close();

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
                SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                string item_name;
                string item_position;

                SQLiteDataReader rdr2 = command2.ExecuteReader();
                int indexC = 0;
                while (rdr2.Read())
                {
                    item_name = rdr2[0].ToString();
                    item_position = rdr2[1].ToString();
                    if (this.typeDGV == 0)
                    {
                        if (item_name != dataGridView.Rows[Int32.Parse(item_position)-1].Cells[0].Value.ToString() || this.countItemDB != this.countDGV)//|| item_position != (indexC + 1).ToString())
                        {
                            flagQuestionSave = true;
                            rdr2.Close();
                            connpar.Close();
                            return;
                        }
                    }
                    else
                    {
                        if (item_name != dataGridView.Rows[indexC].Cells[0].Value.ToString() || this.countItemDB != this.countDGV)
                        {
                            flagQuestionSave = true;
                            rdr2.Close();
                            connpar.Close();
                            return;
                        }
                        indexC++;
                    }
                }
                rdr2.Close();
                connpar.Close();

                if (type != this.typeDGV.ToString() || txtName.Text != name || txtShortName.Text != shortName || txtMin.Text != min || txtMax.Text != max)
                {
                    flagQuestionSave = true;
                }
            }
        }

        //обход повторного появления меседжа с вопросом
        Boolean flagCancel = true;
        #region выделение узлов
        internal void selectedNode(TreeNode nodeSelect, TreeView treeViewMW)
        {
            if(saveFlag != 1)
                proverkaNaIzmenenie(); 

            //условие для возникновения сообщения при создании нового параметра и изменении выделенного
            if (flagQuestionSave && flagCancel)
            {
                var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Yes:
                        flagQuestionSave = false;
                        //проверка адекватности сохраняемых данных 
                        if (saveParam()== 0)
                        {
                            flagQuestionSave = true;
                            return;
                        }
                        break;
                    case DialogResult.No:
                        flagQuestionSave = false;
                        if (newNode == this.nodeSelect)
                        {
                            deleteParam(this.nodeSelect);
                        }    
                        break;
                    case DialogResult.Cancel:    
                        //flagCancel = false;
                        //treeViewMW.SelectedNode = this.nodeSelect;
                        //flagCancel = true;      
                        flagQuestionSave = false;   
                        return;
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
            flagCheck = false;
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
            treeViewCheck.Nodes.Clear();
            treeViewCheck.Nodes.Add("Выделение всех параметров");
            for (int i = 0; i < handbookList.Count; i++)
            {
                treeViewCheck.Nodes.Add(handbookList[i]);
                foreach (DataRow dr in dslist[i].Tables["LST_CLASS"].Rows)
                {
                    treeViewCheck.Nodes[i + 1].Nodes.Add(dr["CLASS_NAME"].ToString());
                }
            }

            treeViewCheck.Nodes[1].Expand();
            treeViewCheck.CheckBoxes = true;
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
            //открытие обновленной бд
            dsPar = sqlite.dataSetParamLoader(filename_inp_path);
            connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;");
            string cmd = @"SELECT *FROM sl_table WHERE inpar_id=@inpar_id";
            SQLiteCommand command = new SQLiteCommand(cmd, connpar);

            clearTreeViewCheck();

            if (nodeSelect.Level == 2)
            {
                selectInpar_id = specificCell();

                command.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                connpar.Open();
                SQLiteDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    object handbook_id = rdr[1];
                    object class_id = rdr[2];

                    if (Int32.Parse(class_id.ToString()) != -1)
                    {
                        treeViewCheck.Nodes[Int32.Parse(handbook_id.ToString())].Nodes[Int32.Parse(class_id.ToString()) - 1].Checked = true;
                    }
                    else
                    {
                        checkedAll(Int32.Parse(handbook_id.ToString()));
                    }
                }
                rdr.Close();
                connpar.Close();

                ////выделение чеков не находящтхся в глобальных и локальных общ парам 
                //if (nodeSelect.Parent.Index != 0)
                //{
                //    if (nodeSelect.Name != "")
                //    {
                //        //определенная ячейка таблицы инпар 
                //        int ind = Int32.Parse(nodeSelect.Name);
                //        DataRow dr = dsPar.Tables["LST_INPAR"].Rows[ind - 1];

                //        XDocument d = XDocument.Parse(dr["sl_valid"].ToString());
                //        foreach (XElement el in d.Descendants("hb"))
                //        {
                //            foreach (XElement el2 in el.Descendants("class"))
                //            {
                //                int fir = Int32.Parse(el.Attribute("id").Value);
                //                int sec = Int32.Parse(el2.Attribute("id").Value);
                //                treeViewCheck.Nodes[fir].Nodes[sec - 1].Checked = true;
                //            }
                //        }
                //        foreach (var item in listLocalJeneralParam)
                //        {
                //            if (item.inpar_name == nodeSelect.Text)
                //                treeViewCheck.Nodes[item.number_sp].Checked = true;
                //        }
                //        selectInpar_id = ind;
                //    }
                //}

                ////в локальных парам
                //else
                //{
                //    foreach (var item in listLocalJeneralParam)
                //    {
                //        if (item.inpar_name == nodeSelect.Text)
                //        {
                //            //задаем выделенному параметру имя из листа, тк в дереве оно значится без имени
                //            nodeSelect.Name = item.inpar_id.ToString();
                //            //выделяем все чеки у этого справочника
                //            checkedAll(item.number_sp);
                //            //определенная ячейка таблицы инпар 
                //            DataRow dr = dsPar.Tables["LST_INPAR"].Rows[item.inpar_id - 1];
                //            //выделяем оставшиеся чеки в других справочниках (не находящиеся в общих параметрах)
                //            XDocument d = XDocument.Parse(dr["sl_valid"].ToString());
                //            foreach (XElement el in d.Descendants("hb"))
                //            {
                //                if (Int32.Parse(el.Attribute("id").Value) != item.number_sp)
                //                {
                //                    foreach (XElement el2 in el.Descendants("class"))
                //                    {
                //                        int fir = Int32.Parse(el.Attribute("id").Value);
                //                        int sec = Int32.Parse(el2.Attribute("id").Value);
                //                        treeViewCheck.Nodes[fir].Nodes[sec - 1].Checked = true;
                //                    }
                //                }
                //            }
                //            selectInpar_id = item.inpar_id;
                //        }
                //    }
                //}
            }
            //в глобальных парам 
            else
            {
                for (int j = 0; j < handbookList.Count + 1; j++)
                {
                    checkedAll(j);
                }
                //добавить отображение главных чеков
                //connpar.Open();
                selectInpar_id = specificCell();
                //command.Parameters.AddWithValue("@inpar_id", inpar_id.ToString());
                //MessageBox.Show(selectInpar_id+"");
                //SQLiteDataReader rdr = command.ExecuteReader();
                //while (rdr.Read())
                //{
                //    object handbook_id = rdr[1];
                //    object class_id = rdr[2];

                //    if (Int32.Parse(class_id.ToString()) != -1)
                //    {
                //        treeViewCheck.Nodes[Int32.Parse(handbook_id.ToString())].Nodes[Int32.Parse(class_id.ToString()) - 1].Checked = true;
                //    }
                //    else
                //    {
                //        checkedAll(Int32.Parse(handbook_id.ToString()));
                //    }
                //}
                //rdr.Close();

                //connpar.Close();

                //foreach (var item in listGlobalJeneralParam)
                //{
                //    if (item.inpar_name == nodeSelect.Text)
                //    {
                //        selectInpar_id = item.inpar_id;
                //        //задаем выделенному параметру имя из листа, тк в дереве оно значится без имени
                //        nodeSelect.Name = item.inpar_id.ToString();
                //    }
                //}
            }
        }

        private void treeViewCheck_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //treeViewCheck.SelectedNode = e.Node;
            this.nodeCheck = e.Node;

            if (flagCheck)
            {
                //flagLoc = 0;
                //flagGlob = 0;

                if (e.Node.Checked)
                {
                    //if (treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[nodeCheck.Index + 1].Nodes.Count == 0)
                    //{
                    //    addAt();
                    //    e.Node.Checked = true;
                    //    //проверка на выделение чеков, для повторного выделения
                    //    if (nodeSelect.Parent.Parent != null)
                    //        checkedAllLocalProverka(nodeSelect.Parent.Parent.Index);
                    //    signal();
                    //    return;
                    //}
                    //else
                    //{
                    //    foreach (TreeNode item in treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[nodeCheck.Index + 1].Nodes)
                    //    {
                    //        if (item.Text == nodeSelect.Text)
                    //            return;
                    //    }
                    //}

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
                        //проверка на выделение чеков, для повторного выделения
                        //if (nodeCheck.Parent != null)
                        //{
                        //    flagLoc = checkedAllLocalProverka(nodeCheck.Parent.Index);
                        //    if (flagLoc == 1)
                        //    {
                        //        flagGlob = checkedAllGlobalProverka(nodeCheck.Parent.Index);
                        //    }
                        //}
                        //else 
                        //{
                        //    flagLoc = 1;
                        //    if (nodeCheck.Index == 0)
                        //        flagGlob = 1;
                        //    else
                        //        flagGlob = checkedAllGlobalProverka(nodeCheck.Index);
                        //}
                        //signal();
                        return;
                    }
                    //перенес из под кэтча//
                    addAt();
                    e.Node.Checked = true;
                    //проверка на выделение чеков, для повторного выделения
                    //if (nodeCheck.Parent != null)
                    //{
                    //    flagLoc = checkedAllLocalProverka(nodeCheck.Parent.Index);
                    //    if (flagLoc == 1)
                    //    {
                    //        flagGlob = checkedAllGlobalProverka(nodeCheck.Parent.Index);
                    //    }
                    //}
                    //else
                    //{
                    //    flagLoc = 1;
                    //    if (nodeCheck.Index == 0)
                    //        flagGlob = 1;
                    //    else
                    //        flagGlob = checkedAllGlobalProverka(nodeCheck.Index);
                    //}
                }
                else
                {
                    removeAt();
                    e.Node.Checked = false;
                }

                //signal();
                //открытие обновленной бд
                //this.dsPar = sqlite.dataSetParamLoader(filename_inp_path);
                //this.connpar = new SQLiteConnection("data source=" + filename_inp_path + ";version=3;failifmissing=true;");
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

            foreach (DataRow item in dsPar.Tables["LST_INPAR"].Rows)
            {
                if (item["NAME"].Equals(nodeSelect.Text))
                    inpar_id = Int32.Parse(item["INPAR_ID"].ToString());

            }
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
            //определяем 
            int inpar_id = specificCell();
            string cmd = @"INSERT INTO sl_table (inpar_id,handbook_id,class_id) VALUES (@inpar_id,@handbook_id,@class_id)";
            SQLiteCommand command = new SQLiteCommand(cmd, connpar);

            //клоны для добавления в классы справочников
            TreeNode nodeClone = (TreeNode)nodeSelect.Clone();
            if (nodeCheck.Level == 0)
            {
                //добавление при нажатии на главный чек
                if (nodeCheck.Index == 0)
                {
                    connpar.Open();
                    command.Parameters.AddWithValue("@inpar_id", inpar_id);
                    command.Parameters.AddWithValue("@handbook_id", -1);
                    command.Parameters.AddWithValue("@class_id", "");

                    string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                    SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                    command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                    command2.ExecuteNonQuery();
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

                    /* для выделения цветом(взять в фокус)
                     * nodeSelectNew.TreeView.Focus();
                     */
                }
                //добавление при нажатии на чек справочника
                else
                {
                    Boolean flagLocCheck = proverkaVidelLocCheck(nodeCheck.Index);

                    if (flagLocCheck)
                    {
                        connpar.Open();
                        command.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command.Parameters.AddWithValue("@handbook_id", -1);
                        command.Parameters.AddWithValue("@class_id", "");
                        //и удалить везде 
                        string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                        SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                        command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command2.ExecuteNonQuery();
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

                        int nodeClonePPIndex = nodeSelect.Parent.Parent.Index;
                        command.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command.Parameters.AddWithValue("@handbook_id", nodeCheck.Index);
                        command.Parameters.AddWithValue("@class_id", -1);
                        //и удалить везде      
                        string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id";
                        SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                        command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command2.Parameters.AddWithValue("@handbook_id", nodeCheck.Index);
                        command2.ExecuteNonQuery();
                        command.ExecuteNonQuery();
                        connpar.Close();

                        for (int i = 1; i < treeViewMW.Nodes[nodeCheck.Index].Nodes.Count; i++)
                        {
                            removingNodes(nodeCheck.Index, i, nodeClone);
                        }
                        TreeNode nodeSelectNew = (TreeNode)nodeClone.Clone();
                        treeViewMW.Nodes[nodeCheck.Index].Nodes[0].Nodes.Add(nodeSelectNew);
                        if (nodeClonePPIndex == nodeCheck.Index)
                            treeViewMW.SelectedNode = nodeSelectNew;

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
                    command.Parameters.AddWithValue("@inpar_id", inpar_id);
                    command.Parameters.AddWithValue("@handbook_id", -1);
                    command.Parameters.AddWithValue("@class_id", "");
                    //и удалить везде 
                    string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                    SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                    command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                    command2.ExecuteNonQuery();
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

                }
                else if (flagClassCheck)
                {
                    connpar.Open();
                    int nodeClonePPIndex = nodeSelect.Parent.Parent.Index;
                    command.Parameters.AddWithValue("@inpar_id", inpar_id);
                    command.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Index);
                    command.Parameters.AddWithValue("@class_id", -1);
                    //и удалить везде      
                    string cmd2 = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id";
                    SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                    command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                    command2.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Index);
                    command2.ExecuteNonQuery();
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
                        treeViewMW.SelectedNode = nodeSelectNew;

                    checkedAll(nodeCheck.Parent.Index);

                }
                else
                {
                    //нужна проверка на добавлении последнее чека в справочнике в лок и глоб масштабе 
                    treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[nodeCheck.Index + 1].Nodes.Add((TreeNode)nodeClone.Clone());//.Insert(0, (TreeNode)nodeSelect.Clone());
                    command.Parameters.AddWithValue("@inpar_id", inpar_id);
                    command.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Index);
                    command.Parameters.AddWithValue("@class_id", nodeCheck.Index + 1);
                    connpar.Open();
                    command.ExecuteNonQuery();
                    connpar.Close();
                }
            }

            //command.ExecuteNonQuery();
            //connpar.Close();
        }
        #endregion

        #region удаление
        private void removeAt()
        {
            //определяем yacheyku
            int inpar_id = specificCell();
            TreeNode nodeClone = new TreeNode();
            nodeClone = (TreeNode)nodeSelect.Clone();

            if (nodeCheck.Level == 0)
            {
                connpar.Open();
                if (nodeCheck.Index == 0)
                {
                    string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                    SQLiteCommand command = new SQLiteCommand(cmd, connpar);
                    command.Parameters.AddWithValue("@inpar_id", inpar_id);
                    command.ExecuteNonQuery();
                    removingNodes2(nodeCheck.Index, nodeClone);
                    for (int i = 1; i < handbookList.Count; i++)
                    {
                        unCheckedAll(nodeCheck.Index);
                    }
                    otobrajenie01();
                    treeViewMW.SelectedNode = null;
                }
                else
                {
                    //проверка на выделение главного чека
                    if (treeViewCheck.Nodes[0].Checked)
                    {
                        string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                        SQLiteCommand command = new SQLiteCommand(cmd, connpar);
                        command.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command.ExecuteNonQuery();

                        string cmd2 = @"INSERT INTO sl_table (inpar_id,handbook_id,class_id) VALUES (@inpar_id,@handbook_id,@class_id)";
                        SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                        command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command2.Parameters.AddWithValue("@class_id", -1);
                        for (int hbIndex = 1; hbIndex < treeViewMW.Nodes.Count; hbIndex++)
                        {
                            if (hbIndex != nodeCheck.Index)
                            {
                                treeViewMW.Nodes[hbIndex].Nodes[0].Nodes.Add((TreeNode)nodeClone.Clone());
                                command2.Parameters.AddWithValue("@handbook_id", hbIndex);
                                command2.ExecuteNonQuery();
                            }
                        }
                        removingNodes2(0, nodeClone);
                        treeViewCheck.Nodes[0].Checked = false;
                        otobrajenie01();
                        treeViewMW.SelectedNode = null;
                    }
                    else
                    {
                        int indHb;
                        if (nodeSelect.Level == 1)
                            indHb = nodeSelect.Parent.Index;
                        else
                            indHb = nodeSelect.Parent.Parent.Index;

                        string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id";
                        SQLiteCommand command = new SQLiteCommand(cmd, connpar);
                        command.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command.Parameters.AddWithValue("@handbook_id", nodeCheck.Index);
                        command.ExecuteNonQuery();
                        removingNodes(nodeCheck.Index, 0, nodeClone);
                        if (indHb == nodeCheck.Index)
                        {
                            otobrajenie01();
                            treeViewMW.SelectedNode = null;
                        }
                    }
                    unCheckedAll(nodeCheck.Index);
                }
                connpar.Close();
            }
            else
            {
                string cmd3 = @"SELECT *FROM sl_table WHERE inpar_id=@inpar_id";
                SQLiteCommand command3 = new SQLiteCommand(cmd3, connpar);
                command3.Parameters.AddWithValue("@inpar_id", inpar_id);
                connpar.Open();
                SQLiteDataReader rdr = command3.ExecuteReader();
                while (rdr.Read())
                {
                    int handbook_id = Int32.Parse(rdr[1].ToString());
                    int class_id = Int32.Parse(rdr[2].ToString());

                    if (handbook_id == -1)
                    {
                        string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id";
                        SQLiteCommand command = new SQLiteCommand(cmd, connpar);
                        command.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command.ExecuteNonQuery();

                        //добавить все классы кроме нажатого и все справочники кроме нажатого 
                        string cmd2 = @"INSERT INTO sl_table (inpar_id,handbook_id,class_id) VALUES (@inpar_id,@handbook_id,@class_id)";
                        SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                        command2.Parameters.AddWithValue("@inpar_id", inpar_id);

                        for (int classIndex = 1; classIndex < treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes.Count; classIndex++)
                        {
                            if (classIndex != nodeCheck.Index + 1)
                            {
                                treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[classIndex].Nodes.Add((TreeNode)nodeClone.Clone());
                                command2.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Index);
                                command2.Parameters.AddWithValue("@class_id", classIndex);
                                command2.ExecuteNonQuery();
                            }
                        }

                        command2.Parameters.AddWithValue("@class_id", -1);
                        for (int hbIndex = 1; hbIndex < treeViewMW.Nodes.Count; hbIndex++)
                        {
                            if (hbIndex != nodeCheck.Parent.Index)
                            {
                                treeViewMW.Nodes[hbIndex].Nodes[0].Nodes.Add((TreeNode)nodeClone.Clone());
                                command2.Parameters.AddWithValue("@handbook_id", hbIndex);
                                command2.ExecuteNonQuery();
                            }
                        }
                        removingNodes2(0, nodeClone);

                        treeViewCheck.Nodes[0].Checked = false;
                        treeViewCheck.Nodes[nodeCheck.Parent.Index].Checked = false;
                        otobrajenie01();
                        treeViewMW.SelectedNode = null;
                        break;
                    }
                    //при удалении параметра из класса при выделенном главном чеке 
                    else if (class_id == -1 && handbook_id == nodeCheck.Parent.Index)
                    {

                        string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id";
                        SQLiteCommand command = new SQLiteCommand(cmd, connpar);
                        command.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Index);
                        command.ExecuteNonQuery();

                        string cmd2 = @"INSERT INTO sl_table (inpar_id,handbook_id,class_id) VALUES (@inpar_id,@handbook_id,@class_id)";
                        SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                        command2.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command2.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Index);

                        for (int classIndex = 1; classIndex < treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes.Count; classIndex++)
                        {
                            if (classIndex != nodeCheck.Index + 1)
                            {
                                treeViewMW.Nodes[nodeCheck.Parent.Index].Nodes[classIndex].Nodes.Add((TreeNode)nodeClone.Clone());
                                command2.Parameters.AddWithValue("@class_id", classIndex);
                                command2.ExecuteNonQuery();
                            }
                        }
                        removingNodes(nodeCheck.Parent.Index, 0, nodeClone);

                        treeViewCheck.Nodes[nodeCheck.Parent.Index].Checked = false;
                        otobrajenie01();
                        treeViewMW.SelectedNode = null;
                        break;
                    }
                    //обычное удалении из класса
                    else if (handbook_id == nodeCheck.Parent.Index)
                    {
                        int indClass = nodeSelect.Parent.Index;

                        string cmd = @"DELETE FROM sl_table WHERE inpar_id = @inpar_id AND handbook_id = @handbook_id AND class_id=@class_id";
                        SQLiteCommand command = new SQLiteCommand(cmd, connpar);
                        command.Parameters.AddWithValue("@inpar_id", inpar_id);
                        command.Parameters.AddWithValue("@handbook_id", nodeCheck.Parent.Index);
                        command.Parameters.AddWithValue("@class_id", nodeCheck.Index + 1);
                        command.ExecuteNonQuery();
                        removingNodes(handbook_id, nodeCheck.Index + 1, nodeClone);

                        if (indClass == nodeCheck.Index + 1)
                        {
                            otobrajenie01();
                            treeViewMW.SelectedNode = null;
                        }
                        break;
                    }
                }
                rdr.Close();
                connpar.Close();
            }
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
                    addNewParam(nodeSelect.Index, -1);
            //добавление в классы справочников
            else if (nodeSelect.Level == 1)
                // n,0 //лок общ пар
                if (nodeSelect.Index == 0)
                    addNewParam(nodeSelect.Parent.Index, -1);
                //n,n //в класс
                else
                    addNewParam(nodeSelect.Parent.Index, nodeSelect.Index);
        }

        //добавление новых параметров, индексы - координаты выделенного справочника или класса, в который добавляем
        public void addNewParam(int index, int index_2)
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
            SQLiteCommand command = new SQLiteCommand(cmd, connpar);
            //connpar.Open();
            command.ExecuteNonQuery();
            //connpar.Close(); 

            string cmd2 = @"INSERT INTO sl_table (inpar_id, handbook_id, class_id) VALUES(@inpar_id, @handbook_id, @class_id) ";
            SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
            command2.Parameters.AddWithValue("@inpar_id", numberNextInpar_Id);

            if (index_2 < 0)
            {
                if (index_2 == -2)
                {
                    newNode = treeViewMW.Nodes[index].Nodes.Add("Новый параметр");
                    //cmd += "VALUES (" + numberNextInpar_Id + ", '" + constl + "', '" + "" + "')";
                    command2.Parameters.AddWithValue("@handbook_id", -1);
                    command2.Parameters.AddWithValue("@class_id", "");
                    //treeViewCheck.Nodes[index].Checked = true;
                }
                else
                {
                    newNode = treeViewMW.Nodes[index].Nodes[0].Nodes.Add("Новый параметр");
                    command2.Parameters.AddWithValue("@handbook_id", index);
                    command2.Parameters.AddWithValue("@class_id", -1);
                    //cmd += "VALUES (" + numberNextInpar_Id + ", '" + index + "', '" + constl + "')";
                    //treeViewCheck.Nodes[index].Nodes[0].Checked = true;
                }
            }
            else
            {
                newNode = treeViewMW.Nodes[index].Nodes[index_2].Nodes.Add("Новый параметр");
                command2.Parameters.AddWithValue("@handbook_id", index);
                command2.Parameters.AddWithValue("@class_id", index_2);

                //cmd += "VALUES (" + numberNextInpar_Id + ", '" + index_2 + "', '" + index + "')";
                //treeViewCheck.Nodes[index_2].Nodes[index].Checked = true;
            }
            //connpar.Open();
            command2.ExecuteNonQuery();
            connpar.Close();

            treeViewMW.SelectedNode = newNode;
            //selectedNode(newNode, treeViewMW);

            flagQuestionSave = true;
            treeViewCheck.Enabled = false;
            //connpar.Close();
        }

        public void deleteParam(TreeNode nodeSelect)
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

            connpar.Open();
            string cmd = @"DELETE FROM LST_INPAR WHERE INPAR_ID=@inpar_id";
            SQLiteCommand command = new SQLiteCommand(cmd, connpar);
            command.Parameters.AddWithValue("@inpar_id", selectInpar_id);
            command.ExecuteNonQuery();
            //connpar.Close(); 

            string cmd2 = @"DELETE FROM sl_table WHERE INPAR_ID=@inpar_id";
            SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
            command2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
            command2.ExecuteNonQuery();
            connpar.Close();
        }
        #endregion

        #region сохранение изменений
        //метод для сохранения координат ранее выделенного узла
        public void coordinatsNewNodePar(TreeNode nodeOld, out int indexPPNO, out int indexPNO, out int indexCNO)
        {
            indexPPNO = -1;
            indexPNO = -1;
            indexCNO = -1;
            if (nodeOld.Parent.Parent != null)
            {
                indexPPNO = nodeOld.Parent.Parent.Index;
                indexPNO = nodeOld.Parent.Index;
                indexCNO = nodeOld.Index;
            }
            else
            {
                indexPPNO = nodeOld.Parent.Index;
                indexPNO = nodeOld.Index;
            }
        }

        private void dgvDbUpdate() 
        {
            //получение количества строк в базе и дгв для стравнения
            string cmdSel = @"SELECT COUNT(*) FROM LST_ITEM WHERE INPAR_ID = @inpar_id";
            SQLiteCommand commandSel = new SQLiteCommand(cmdSel, connpar);
            commandSel.Parameters.AddWithValue("@inpar_id", selectInpar_id);
            connpar.Open();
            SQLiteDataReader rdrSel = commandSel.ExecuteReader();
            rdrSel.Read();
            this.countItemDB = Int32.Parse(rdrSel[0].ToString());
            rdrSel.Close();

            if (countItemDB == 0)
            {
                //into                 
                string cmd2 = @"INSERT INTO LST_ITEM (INPAR_ID, ITEM_NAME, ITEM_ID, ITEM_POSITION) VALUES (@inpar_id,@item_name,@item_id,@item_position)";
                SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
                command2.Parameters.AddWithValue("@inpar_id", selectInpar_id);

                if (!cbEdit.Checked)
                {
                    for (int i = 0; i < dataGridView.RowCount - 1; i++)
                    {
                        command2.Parameters.AddWithValue("@item_name", dataGridView.Rows[i].Cells[0].Value.ToString());
                        command2.Parameters.AddWithValue("@item_id", i + 1);
                        command2.Parameters.AddWithValue("@item_position", i + 1);
                        command2.ExecuteNonQuery();
                    }
                }
                else
                {
                    for (int i = 0; i < dataGridView.RowCount - 1; i++)
                    {
                        command2.Parameters.AddWithValue("@item_name", dataGridView.Rows[i].Cells[0].Value.ToString());
                        command2.Parameters.AddWithValue("@item_id", null);
                        command2.Parameters.AddWithValue("@item_position", null);
                        command2.ExecuteNonQuery();
                    }
                }
                string cmd3 = @"INSERT INTO sl_itemid (INPAR_ID, LAST_ITEMID) VALUES (@inpar_id,@last_itemid)";
                SQLiteCommand command3 = new SQLiteCommand(cmd3, connpar);
                command3.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                command3.Parameters.AddWithValue("@last_itemid", dataGridView.RowCount - 1);
                command3.ExecuteNonQuery();
            }
            else
            {
                //обновление lst_item из dgv
                string cmdUp = @"UPDATE LST_ITEM SET ITEM_POSITION=@position WHERE INPAR_ID=@inpar_id AND ITEM_NAME=@name";
                SQLiteCommand commandUp = new SQLiteCommand(cmdUp, connpar);
                commandUp.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                string cmdUp2 = @"UPDATE LST_ITEM SET ITEM_NAME=@name WHERE INPAR_ID=@inpar_id AND ITEM_POSITION=@position";
                SQLiteCommand commandUp2 = new SQLiteCommand(cmdUp2, connpar);
                commandUp2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                //данные dgv в базе
                string cmdSel2 = @"SELECT ITEM_NAME, ITEM_ID, ITEM_POSITION FROM LST_ITEM WHERE INPAR_ID=@inpar_id";// LIMIT @limit,1";
                SQLiteCommand commandSel2 = new SQLiteCommand(cmdSel2, connpar);
                commandSel2.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                Boolean flagHasDB;
                if (this.countItemDB == dataGridView.RowCount - 1)
                {
                    //проверить данные 
                    for (int i = 0; i < dataGridView.RowCount-1; i++)
                    {
                        //command3.Parameters.AddWithValue("@limit", i);
                        SQLiteDataReader rdrSel2 = commandSel2.ExecuteReader();
                        flagHasDB = false;
                        while (rdrSel2.Read())
                        {
                            string name = rdrSel2[0].ToString();
                            string position = rdrSel2[2].ToString();

                            if (name == dataGridView.Rows[i].Cells[0].Value.ToString())
                                if (Int32.Parse(position) == i + 1)
                                {
                                    flagHasDB = true;
                                    break;
                                }
                                //else
                                //{
                                //    commandUp.Parameters.AddWithValue("@name", dataGridView.Rows[i].Cells[0].Value.ToString());
                                //    commandUp.Parameters.AddWithValue("@position", i + 1);
                                //    commandUp.ExecuteNonQuery();
                                //    flagHasDB = true;
                                //    break;
                                //}
                            ////if (item_id != "")
                            //{
                            //    //if (Int32.Parse(item_id) == i + 1)
                            //    {
                            //        //command2.Parameters.AddWithValue("@item_id", item_id);
                            //        command2.Parameters.AddWithValue("@name", dataGridView.Rows[i].Cells[0].FormattedValue);
                            //        command2.Parameters.AddWithValue("@position", i + 1);
                            //        command2.ExecuteNonQuery();
                            //    }
                            //}
                            ////else
                            //{
                            //    //command2.Parameters.AddWithValue("@item_id", "");
                            //    command2.Parameters.AddWithValue("@name", dataGridView.Rows[i].Cells[0].FormattedValue);
                            //    command2.ExecuteNonQuery();
                            //}
                        }
                        rdrSel2.Close();
                        if (!flagHasDB)
                        {
                            commandUp2.Parameters.AddWithValue("@name", dataGridView.Rows[i].Cells[0].Value.ToString());
                            commandUp2.Parameters.AddWithValue("@position", i + 1);
                            commandUp2.ExecuteNonQuery();
                            ////надо добавить в базу
                            //string cmdIns = @"INSERT INTO LST_ITEM (INPAR_ID, ITEM_NAME, ITEM_POSITION) VALUES (@inpar_id,@item_name,@item_position)";
                            //SQLiteCommand commandIns = new SQLiteCommand(cmdIns, connpar);
                            //commandIns.Parameters.AddWithValue("@inpar_id", selectInpar_id);
                            //commandIns.Parameters.AddWithValue("@item_name", dataGridView.Rows[i].Cells[0].Value.ToString());
                            //commandIns.Parameters.AddWithValue("@item_position", i + 1);
                        }
                    }
                }
                else if (this.countItemDB <= (dataGridView.RowCount - 1))
                {
                    //сравнить данные и добавить недостающую строку

                }
                else
                {
                    //сравнить данные и удалить лишнюю строку

                }
            }
            connpar.Close();
        }

        //сохранение параметров
        private int saveParam()
        {
            //редактируемый узел
            TreeNode nodeOld = (TreeNode)nodeSelect.Clone();
            int indexPPNO = -1;
            int indexPNO = -1;
            int indexCNO = -1;
            coordinatsNewNodePar(nodeSelect, out indexPPNO, out indexPNO, out indexCNO);

            selectInpar_id = specificCell();
            string cmd = @"UPDATE LST_INPAR SET TYPE=@type, NAME=@name, SHORT=@short, MIN=@min, MAX=@max WHERE INPAR_ID=@inpar_id";
            SQLiteCommand command = new SQLiteCommand(cmd, connpar);

            if (radioButtonCb.Checked)
            {
                if (txtName.Text != "" && txtShortName.Text != "")
                {

                    int type = cbEdit.Checked ? 1 : 0;
                    double min, max;
                    if (!cbEdit.Checked)
                    {
                        cmd = @"UPDATE LST_INPAR SET TYPE=@type, NAME=@name, SHORT=@short WHERE INPAR_ID=@inpar_id";
                        command = new SQLiteCommand(cmd, connpar);

                    }
                    else
                    {
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
                            MessageBox.Show("Проверьте минимум.", "Неверно заполнены поля", MessageBoxButtons.OK);
                            return 0;
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
                            MessageBox.Show("Проверьте максимум.", "Неверно заполнены поля", MessageBoxButtons.OK);
                            return 0;
                        }
                    }

                    dgvDbUpdate();
#if DEBUG
                    Console.WriteLine("Debug");
#endif
                    //// Получаем id парамтера из LST_INPAR
                    //string cmd3 = @"SELECT last_insert_rowid()";
                    //SQLiteCommand command3 = new SQLiteCommand(cmd3, connpar);
                    //connpar.Open();
                    //SQLiteDataReader rdr2 = command3.ExecuteReader();
                    //string id;
                    //rdr2.Read();
                    //id = rdr2[0].ToString();
                    //connpar.Close();
                    /*
                     string cmd3 = @"SELECT ITEM_ID FROM LST_ITEM WHERE INPAR_ID=@inpar_id ORDER BY ITEM_ID DESC LIMIT 1";
                     SQLiteCommand command3 = new SQLiteCommand(cmd3, connpar);
                     connpar.Open();
                     SQLiteDataReader rdr2 = command3.ExecuteReader();
                     rdr2.Read();
                     //последний итем в данном параметре
                     int id = Int32.Parse(rdr2[0].ToString());
                     connpar.Close();
                     if (!cbEdit.Checked)
                         for (int indexClassId = 0; indexClassId < dataGridView.RowCount - 1; indexClassId++)
                         {
                             string cmd4 = @"INSERT INTO LST_ITEM (INPAR_ID, ITEM_NAME, ITEM_ID) ";
                             cmd4 += "VALUES (" + id + ", '" + dataGridView.Rows[indexClassId].Cells[1].Value + "', " + (indexClassId + 1).ToString() + ")";
                             SQLiteCommand command4 = new SQLiteCommand(cmd4, connpar);
                             command4.ExecuteNonQuery();
                         }
                     else
                         for (int indexClassId = 0; indexClassId < dataGridView.RowCount - 1; indexClassId++)
                         {
                             string cmd4 = @"INSERT INTO LST_ITEM (INPAR_ID, ITEM_NAME) ";
                             cmd4 += "VALUES (" + id + ", '" + dataGridView.Rows[indexClassId].Cells[1].Value + "')";
                             SQLiteCommand command4 = new SQLiteCommand(cmd4, connpar);
                             command4.ExecuteNonQuery();
                         }
                     */
                    command.Parameters.AddWithValue("@type", type);
                }
                else
                {
                    MessageBox.Show("Есть незаполненные поля");
                    return 0;
                }
            }
            else if (radioButtonTb.Checked)
            {
                int type = tbInt.Checked ? 3 : 2;
                int min, max;
                float dmin, dmax;

                if (tbInt.Checked && (Int32.TryParse(txtMin.Text, out min) || minInf.Checked) && (Int32.TryParse(txtMax.Text, out max) || maxInf.Checked))
                {
                    //cmd = @"INSERT INTO LST_INPAR (INPAR_ID, TYPE, NAME, SHORT, sl_valid, MIN, MAX) ";
                    //cmd += "VALUES (" + numberNextInpar_Id + ", '" + type + "', '" + txtName.Text + "', '" + txtShortName.Text + "', '" + d.ToString() + "', '";

                    if (minInf.Checked)
                    {
                        //cmd += "-9e9999', '";
                        command.Parameters.AddWithValue("@min", "-9e9999");
                    }
                    else
                    {
                        //cmd += min + "', '";
                        command.Parameters.AddWithValue("@min", min);
                    }

                    if (maxInf.Checked)
                    {
                        //cmd += "9e9999')";
                        command.Parameters.AddWithValue("@max", "9e9999");

                    }
                    else
                    {
                        //cmd += max + "')";
                        command.Parameters.AddWithValue("@max", max);
                    }
                }
                else if (!tbInt.Checked && (float.TryParse(txtMin.Text, out dmin) || minInf.Checked) && (float.TryParse(txtMax.Text, out dmax) || maxInf.Checked))
                {
                    //cmd = @"INSERT INTO LST_INPAR (INPAR_ID, TYPE, NAME, SHORT, sl_valid, MIN, MAX) ";
                    //cmd += "VALUES (" + numberNextInpar_Id + ", '" + type + "', '" + txtName.Text + "', '" + txtShortName.Text + "', '" + d.ToString() + "', '";

                    if (minInf.Checked)
                    {
                        //cmd += "-9e9999', '";
                        command.Parameters.AddWithValue("@min", "-9e9999");
                    }
                    else
                    {
                        //cmd += dmin + "', '";
                        command.Parameters.AddWithValue("@min", dmin);
                    }

                    if (maxInf.Checked)
                    {
                        //cmd += "9e9999')";
                        command.Parameters.AddWithValue("@max", "9e9999");
                    }
                    else
                    {
                        //cmd += dmax + "')";
                        command.Parameters.AddWithValue("@max", dmax);
                    }

                }
                else
                {
                    MessageBox.Show("Проверьте минимум и максимум.", "Неверно заполнены поля", MessageBoxButtons.OK);
                    return 0;
                }
                command.Parameters.AddWithValue("@type", type);
            }
            command.Parameters.AddWithValue("@inpar_id", selectInpar_id);
            command.Parameters.AddWithValue("@name", txtName.Text);
            command.Parameters.AddWithValue("@short", txtShortName.Text);

            connpar.Open();
            command.ExecuteNonQuery();
            connpar.Close();

            updateTextInTreeView(nodeOld);

            MessageBox.Show("SAVE");
            flagQuestionSave = false;
            treeViewCheck.Enabled = true;
            saveFlag = 1;
            if (indexCNO != -1)
                treeViewMW.SelectedNode = treeViewMW.Nodes[indexPPNO].Nodes[indexPNO].Nodes[indexCNO];
            else
                treeViewMW.SelectedNode = treeViewMW.Nodes[indexPPNO].Nodes[indexPNO];
            return 1;
        }

        private void updateTextInTreeView(TreeNode nodeOld)
        {
            //обновление текста в тривью после изменения имени параметра
            string cmd2 = @"SELECT *FROM sl_table WHERE inpar_id=@inpar_id";
            SQLiteCommand command2 = new SQLiteCommand(cmd2, connpar);
            command2.Parameters.AddWithValue("@inpar_id", selectInpar_id);

            treeViewMW.SelectedNode = null;
            connpar.Open();

            TreeNode nodeFuture = new TreeNode();
            nodeFuture.Text = txtName.Text;
            SQLiteDataReader rdr = command2.ExecuteReader();
            while (rdr.Read())
            {
                int handbook_id = Int32.Parse(rdr[1].ToString());
                int class_id = Int32.Parse(rdr[2].ToString());
                if (handbook_id == -1)
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
                else if (class_id == -1)// && handbook_id == nodeCheck.Parent.Index)
                {
                    foreach (TreeNode item in treeViewMW.Nodes[handbook_id].Nodes[0].Nodes)
                    {
                        if (item.Text.Equals(nodeOld.Text))
                        {
                            treeViewMW.Nodes[handbook_id].Nodes[0].Nodes.Insert(item.Index, (TreeNode)nodeFuture.Clone());
                            item.Remove();
                            break;
                        }
                    }
                }
                //обычное удалении из класса
                else //if (handbook_id == nodeCheck.Parent.Index)
                {
                    foreach (TreeNode item in treeViewMW.Nodes[handbook_id].Nodes[class_id].Nodes)
                    {
                        if (item.Text.Equals(nodeOld.Text))
                        {
                            treeViewMW.Nodes[handbook_id].Nodes[class_id].Nodes.Insert(item.Index, (TreeNode)nodeFuture.Clone());
                            item.Remove();
                            //treeViewMW.Nodes[handbook_id].Nodes[class_id].Nodes.Add(nodeFuture.Text);
                            break;
                        }
                    }
                }
            }
            rdr.Close();
            connpar.Close();
        }
        #endregion

        #region кнопки
        private void buttonAddParam_Click(object sender, EventArgs e)
        {
            newParam();
        }

        public int saveFlag { get; set; }
        private void buttonSaveParam_Click(object sender, EventArgs e)
        {
            saveFlag = saveParam();
        }

        private void buttonCancelParam_Click(object sender, EventArgs e)
        {
            flagQuestionSave = false;

            txtName.Clear();
            txtShortName.Clear();
            txtMin.Clear();
            minInf.Checked = false;
            txtMax.Clear();
            maxInf.Checked = false;
            tbInt.Checked = false;
            cbEdit.Checked = false;
            dataGridView.Rows.Clear();
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
        #endregion

        #region панелька, описание всех внутренних ..боксов
        //вывод значений параметров на панель 
        public void workingWihtTable()
        {
            cleanWorkingPanel();
            connpar.Open();
            string cmd = @"SELECT * FROM LST_INPAR WHERE LST_INPAR.INPAR_ID =@inpar_id";// +treeViewMW.SelectedNode.Name;
            SQLiteCommand com = new SQLiteCommand(cmd, connpar);
            com.Parameters.AddWithValue("@inpar_id", selectInpar_id);
            SQLiteDataReader rdr = com.ExecuteReader();

            //получение количества строк в базе и дгв для стравнения
            string cmdCount = @"SELECT COUNT(*) FROM LST_ITEM WHERE INPAR_ID = @inpar_id";
            SQLiteCommand commandCount = new SQLiteCommand(cmdCount, connpar);
            commandCount.Parameters.AddWithValue("@inpar_id", selectInpar_id);
            SQLiteDataReader rdrCount = commandCount.ExecuteReader();
            rdrCount.Read();
            this.countItemDB = Int32.Parse(rdrCount[0].ToString());
            rdrCount.Close();
            dataGridView.Rows.Add(this.countItemDB); 

            //string cmdpar = @"SELECT LST_ITEM.ITEM_NAME FROM LST_ITEM WHERE LST_ITEM.INPAR_ID =@item_id"; //+ treeViewMW.SelectedNode.Name;
            string cmdpar = @"SELECT *FROM LST_ITEM WHERE LST_ITEM.INPAR_ID =@item_id"; //+ treeViewMW.SelectedNode.Name;
            SQLiteCommand compar = new SQLiteCommand(cmdpar, connpar);
            compar.Parameters.AddWithValue("@item_id", selectInpar_id);
            SQLiteDataReader rdrpar = compar.ExecuteReader();

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

                        int index = 1;
                        while (rdrpar.Read())
                        {
                            if (rdrpar["ITEM_ID"].ToString() != "")
                            {
                                index = Int32.Parse(rdrpar["ITEM_POSITION"].ToString());
                            }
                            dataGridView.Rows[index - 1].Cells[0].Value = rdrpar["ITEM_NAME"].ToString();
                            index++;
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
                rdrpar.Close();
            }
            connpar.Close();
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

        #region dgv
        private void btnAddRow_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Add();
        }

        private void btnRemoveRow_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewCell dr in dataGridView.SelectedCells)
                {
                    dataGridView.Rows.Remove(dr.OwningRow);
                }
            }
            catch { }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            dataGridView.BeginEdit(false);
        }

        private void cbTable_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //if (dataGridView.CurrentCell.Value == null)
            //{
            //    dataGridView.CurrentRow.Cells[0].Value = e.RowIndex + 1;
            //}
            //else
            //    MessageBox.Show("" + dataGridView.CurrentRow.Cells[0].Value);
        }

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

        private void btnSort_Click(object sender, EventArgs e)
        {   
            //asc = !asc;
            //dataGridView.Sort(new Sort(asc));
            dataGridView.Sort(dataGridView.Columns[0],ListSortDirection.Ascending);// DataGrtidView.Columns[0], ListSortDirection.Ascending);
        }
        #endregion

        private void dataGridView_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {

            if (e.Column.Index == 0)
            {
                try
                {
                    e.SortResult = float.Parse(e.CellValue1.ToString()).CompareTo(float.Parse(e.CellValue2.ToString()));
                    e.Handled = true;
                }
                catch (FormatException fe)
                {
                    MessageBox.Show(fe + "");
                }
            }

        }
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
                // (-1) - x > y
                // (0)  - x = y
                // (1)  - x < y
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

                //У x или y в конце нет числа
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