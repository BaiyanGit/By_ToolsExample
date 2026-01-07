namespace _3rdBy.ByTools.ExcelDataTool.Editor.ExcelConvert.Convert
{
    using System;
    using System.Data;
    using System.IO;
    using System.Text;
    using ExcelConvet;
    using ExcelDataReader;
    using UnityEditor;
    using UnityEngine;

    public class ExcelExportToAsset
    {
        public void Generate(string filePath, ExcelConvertPathType pathType)
        {
            try
            {
                using (FileStream file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (IExcelDataReader excelData = ExcelReaderFactory.CreateOpenXmlReader(file))
                    {
                        DataSet   dataSet = excelData.AsDataSet();
                        DataTable sheet   = dataSet.Tables[0];

                        ExcelMiddleData data = new ExcelMiddleData();
                        data.Init(sheet, filePath);

                        string tempName = Path.GetFileNameWithoutExtension(filePath);
                        tempName = tempName.Replace("t_", "");

                        //string json = LitJson.JsonMapper.ToJson(data.excelMiddleDatas);
                        string json = data.ToJson();

                        string jsonDir;
                        if (pathType == ExcelConvertPathType.Both)
                        {
                            //Resources
                            jsonDir = ExcelConvertPathSetting.GetExcelGenerateAssetFilePath(ExcelConvertPathType
                                .Resources);
                            if (!Directory.Exists(jsonDir))
                            {
                                Directory.CreateDirectory(jsonDir);
                            }

                            string targetFilePath = jsonDir + tempName + ".json";
                            SaveFile(json, targetFilePath);

                            //StreamingAssets
                            jsonDir = ExcelConvertPathSetting.GetExcelGenerateAssetFilePath(ExcelConvertPathType
                                .StreamingAssets);
                            if (!Directory.Exists(jsonDir))
                            {
                                Directory.CreateDirectory(jsonDir);
                            }

                            targetFilePath = jsonDir + tempName + ".json";

                            SaveFile(json, targetFilePath);
                        }
                        else
                        {
                            jsonDir = ExcelConvertPathSetting.GetExcelGenerateAssetFilePath(pathType);
                            if (!Directory.Exists(jsonDir))
                            {
                                Directory.CreateDirectory(jsonDir);
                            }

                            string targetFilePath = jsonDir + tempName + ".json";

                            SaveFile(json, targetFilePath);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void SaveFile(string str, string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);

            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(str);
                }
            }

            Debug.Log("asset create: " + filePath);
            AssetDatabase.Refresh();
        }
    }
}