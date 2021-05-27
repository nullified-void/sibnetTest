using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sibnetTestApp
{
    public class CsvImport
    {
        public static DataTable NewDataTable(string fileName, string delimiters, bool firstRowContainsFieldNames = true, bool skipTwoRows = false)
        {
            DataTable result = new DataTable();

            using (TextFieldParser tfp = new TextFieldParser(fileName, Encoding.GetEncoding(1251)))
            {
                tfp.SetDelimiters(delimiters);
                if (!tfp.EndOfData)
                {
                    if (skipTwoRows)
                    {
                        tfp.ReadFields();
                        tfp.ReadFields();
                    }
                    
                    string[] fields = tfp.ReadFields();

                    for (int i = 0; i < fields.Count(); i++)
                    {
                        if (firstRowContainsFieldNames)
                            result.Columns.Add(fields[i]);
                        else
                            result.Columns.Add("Col" + i);
                    }

                    if (!firstRowContainsFieldNames)
                        result.Rows.Add(fields);
                }
                while (!tfp.EndOfData)
                    result.Rows.Add(tfp.ReadFields());
                
                
            }

            return result;
        }
    }
}
