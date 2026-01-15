namespace _3rdBy.ByTools.TableConvertJson.ExcelDataTool.Editor.ExcelConvert.Convert
{
    using System.IO;
    using System.Text;
    using ConvertHelper;
    using ExcelDataReader;
    using UnityEditor;
    using UnityEngine;

    public class ExcelExportToClass
    {
        private string _templateFilePath = string.Empty;

        /// <summary>
        /// 获取模板文件路径
        /// </summary>
        private void GetTemplateFilePath()
        {
            _templateFilePath = TableHelper.FindExcelDataClassTemplatePath("ExcelDataClassTemplate");
        }

        public void Generate(string filePath)
        {
            // try
            {
                using var file      = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var excelData = ExcelReaderFactory.CreateOpenXmlReader(file);
                var       dataSet   = excelData.AsDataSet();
                var       sheet     = dataSet.Tables[0];

                string tempName           = Path.GetFileNameWithoutExtension(filePath);
                string dataItemClassName  = tempName;
                string dataTableClassName = tempName + "Table";

                var data = new ExcelMiddleData();
                data.Init(sheet, filePath);

                var sbProps = new StringBuilder();
                for (int i = 0; i < data.realColumns.Count; i++)
                {
                    var type      = data.types[i];
                    var prop      = data.props[i];
                    var noteArray = data.notes[i];

                    if (prop == "Id") continue;

                    sbProps.Append("\t/// <summary>\n");
                    for (int j = 0; j < noteArray.Length; j++)
                    {
                        var note = noteArray[j];
                        sbProps.Append("\t/// " + note + "\n");
                    }

                    sbProps.Append("\t/// </summary>\n");
                    sbProps.Append($"\tpublic {type} {prop} {{ get; set; }}\n");
                    sbProps.AppendLine();
                }

                if (_templateFilePath == string.Empty)
                {
                    GetTemplateFilePath();
                }

                string tempStrFile = AssetDatabase.LoadAssetAtPath<TextAsset>(_templateFilePath).text;
                tempStrFile = tempStrFile.Replace("{0}", dataItemClassName);
                tempStrFile = tempStrFile.Replace("{1}", dataTableClassName);
                tempStrFile = tempStrFile.Replace("{2}", sbProps.ToString());
                string csDir          = TableHelper.GetGenerateScriptPath(ConvertFileType.Excel);
                string targetFilePath = csDir + dataItemClassName + ".cs";

                SaveFile(tempStrFile, targetFilePath);
            }
            // catch (Exception e)
            // {
            //     Debug.LogError(e.ToString());
            // }
        }

        private static void SaveFile(string str, string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);

            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(str);
                }
            }

            Debug.Log("类创建: " + filePath);
            AssetDatabase.Refresh();
        }
    }
}