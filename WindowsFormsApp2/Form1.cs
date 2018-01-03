﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        string str = ConfigurationManager.ConnectionStrings["cis"].ConnectionString;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string tablename = "select TABLE_NAME from all_tables where owner='LENOVO_CIS'";
            string sql = "select column_name,data_type,data_precision,data_scale from user_tab_columns t where t.TABLE_NAME='{0}'";

            DataTable dtTableName = GetData(tablename);
            for (int i = 0; i < dtTableName.Rows.Count; i++)
            {
                DataTable dtFiled = GetData(string.Format(sql, dtTableName.Rows[i][0].ToString()));

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(GetNameSpace());
                stringBuilder.AppendLine(GetClass("Advices.Entity", GetTitleCase(dtTableName.Rows[i][0].ToString().Replace("HD_", "").Replace("_", " ")).Replace(" ", "")));
                for (int j = 0; j < dtFiled.Rows.Count; j++)
                {
                    stringBuilder.AppendLine(GetBody(dtFiled.Rows[j][0].ToString(),
                        ConvertSqlToNetType(dtFiled.Rows[j][1].ToString(), NullToInt(dtFiled.Rows[j][3]))
                                             , GetTitleCase(dtFiled.Rows[j][0].ToString().Replace("_", " ")).Replace(" ", "")));
                }
                stringBuilder.AppendLine(GetFoot());

                string path = AppDomain.CurrentDomain.BaseDirectory + "/class/";
                string fileName = path + GetTitleCase(dtTableName.Rows[i][0].ToString().Replace("HD_","").Replace("_", " ")).Replace(" ", "") + ".cs";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.AppendAllText(fileName, stringBuilder.ToString());
            }

        }

        public int NullToInt(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            if (string.IsNullOrWhiteSpace(obj.ToString()))
            {
                return 0;
            }
            return Convert.ToInt32(obj.ToString());
        }

        public string GetTitleCase(string word)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(word.ToLower());
        }

        public string ConvertSqlToNetType(string sqlType, int SqlLength)
        {
            switch (sqlType)
            {
                case "NUMBER":
                    if (SqlLength > 0) { return "decimal?"; }
                    else
                    {
                        return "int?";
                    };
                case "VARCHAR2": return "string";
                case "DATE": return "DateTime";
                default:
                    return "string";
            }
        }

        public DataTable GetData(string sql)
        {
            using (OracleConnection conn = new OracleConnection(str))
            {
                conn.Open();
                OracleDataAdapter sda = new OracleDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                conn.Close();
                return ds.Tables[0];
            }
        }

        public string GetNameSpace()
        {
            StringBuilder strClass = new StringBuilder();
            strClass.AppendLine("using Lenovo.HIS.Common;");
            strClass.AppendLine("using System;");
            strClass.AppendLine("using System.Collections.Generic;");
            strClass.AppendLine("using System.Linq;");
            strClass.AppendLine("using System.Runtime.Serialization;");
            strClass.AppendLine("using System.Text;");
            strClass.AppendLine("      ");
            return strClass.ToString();
        }

        public string GetClass(string namespac, string className)
        {
            StringBuilder strClass = new StringBuilder();
            strClass.AppendLine("namespace Lenovo.HIS." + namespac);
            strClass.AppendLine("{");
            strClass.AppendLine("    public class " + className);
            strClass.AppendLine("    {");
            return strClass.ToString();
        }

        public string GetBody(string DataField, string type, string filedName)
        {
            StringBuilder strb = new StringBuilder();
            strb.AppendLine("        [DataMember]");
            strb.AppendLine("        [DataField(\""+ DataField + "\")]");
            strb.AppendLine("        public " + type + " " + filedName + " { get; set; }");
            strb.AppendLine("        ");
            return strb.ToString();
        }

        public string GetFoot()
        {
            StringBuilder strb = new StringBuilder();
            strb.AppendLine("    }");
            strb.AppendLine("}");
            return strb.ToString();
        }
    }
}
