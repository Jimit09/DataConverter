using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DataConverter
{
    public class CsvReader
    {
        /// <summary>
        /// These are special characters in CSV files. If a column contains any
        /// of these characters, the entire column is wrapped in double quotes.
        /// </summary>
        private const char Delimiter = ',';
        private const char Quote = '"';

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Path">Path of Csv File to Read</param>
        /// <param name="columnheaders"></param>
        /// <returns></returns>
        public DataTable ReadCsvToDataTable(string path, DataColumn[] columnheaders, string tableName = "")
        {
            StreamReader streamReader = new StreamReader(path);
            List<string> listOfLines = new List<string>();
            string fileContents = string.Empty;
            while (!streamReader.EndOfStream)
            {
                fileContents = streamReader.ReadToEnd();
            }
            listOfLines = fileContents.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            DataTable dt = null;
            if (listOfLines.Count > 0)
            {
                dt = new DataTable();
                dt.Columns.AddRange(columnheaders);
            }
            for (int i = 0; i < listOfLines.Count; i++)
            {
                string line = listOfLines[i];
                bool isComment = false;
                if (line.Length >= 2)
                {
                    //If the 1st 2 char of line contains the '*'(comment character) then line needs to be ignored.
                    isComment = line.Substring(0, 1).All(r => r.Equals('*'));
                }
                if (!isComment)
                {
                    List<string> row = ParseRow(line, columnheaders.Count());
                    DataRow dataRow = dt.NewRow();
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        //This if's are bcoz blank string cannot be set to Datacolumn of non-string type
                        if (dt.Columns[j].DataType == typeof(bool))
                        {
                            if (row[j] == string.Empty)
                            {
                                dataRow[j] = false;
                            }
                            else
                            {
                                dataRow[j] = row[j];
                            }
                        }
                        else if (dt.Columns[j].DataType == typeof(Int32))
                        {
                            if (row[j] == string.Empty)
                            {
                                dataRow[j] = 0;
                            }
                            else
                            {
                                dataRow[j] = row[j];
                            }
                        }
                        else
                        {
                            dataRow[j] = row[j];
                        }
                    }
                    dt.Rows.Add(dataRow);
                }
            }
            streamReader.Close();
            return dt;
        }

        public List<string> ParseRow(string line, int noOfColumns)
        {
            char[] array = line.ToCharArray();
            bool inQuotes = false;
            List<string> row = new List<string>();
            StringBuilder columnData = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == Quote && columnData.Length == 0 && !inQuotes)//When column Data starts with Quotes setting InQuotes to true
                {
                    inQuotes = true;
                }
                else if (array[i] == Delimiter)//When ,
                {
                    if (inQuotes)//If in Quotes then just add , as part of data
                    {
                        columnData.Append(array[i]);
                    }
                    else//If not then it is the end of column then add to Row
                    {
                        row.Add(columnData.ToString().Trim());
                        columnData.Clear();
                    }
                }
                else if (array[i] == Quote)//If data is Quote   
                {
                    if (inQuotes)//If in Quotes 
                    {
                        int nextPos = i + 1;
                        if (nextPos < array.Length)
                        {
                            //If the next is element is also quote means next delimiter is data so add to columnData
                            if (array[nextPos] == Quote)
                            {
                                columnData.Append(array[nextPos]);
                                i++;
                            }
                            else //It is the end of Quotes and so ends the column
                            {
                                row.Add(columnData.ToString());
                                columnData.Clear();
                                i++; //This is to skip delimiter(,) which will be for the end of column 
                                inQuotes = false;
                            }
                        }
                        else //It is the end of Quotes and also the last column & the column  ends with Quote.
                        {
                            row.Add(columnData.ToString());
                            columnData.Clear();
                            i++; //This is to skip delimiter(,) which will be for the end of column 
                            inQuotes = false;
                        }
                    }
                }
                else // Add data
                {
                    if (inQuotes)
                    {
                        columnData.Append(array[i]);
                    }
                    else if (!char.IsWhiteSpace(array[i]) || columnData.Length != 0)//Ignore starting Whitespaces if not in quotes.
                    {
                        columnData.Append(array[i]);
                    }
                    if (i == (array.Length - 1))
                    {
                        row.Add(columnData.ToString().Trim());
                        columnData.Clear();
                    }
                }
            }
            //If last Column of a row which is blank,then it will not be in the string passed and so needs to be added in the row List
            //If last Column of a row which is not blank,then it will be in the part of the string passed and so no need to be added in the row List
            if (row.Count < noOfColumns)
            {
                row.Add("");
            }
            return row;
        }
    }
}