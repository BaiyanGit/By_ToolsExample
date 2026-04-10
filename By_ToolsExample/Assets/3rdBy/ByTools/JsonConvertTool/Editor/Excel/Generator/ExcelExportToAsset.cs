namespace _3rdBy.ByTools.TableConvertJson.Editor.Excel.Generator
{
    using System;
    using System.IO;
    using System.Text;
    using ConvertHelper;
    using ExcelDataReader;
    using UnityEditor;
    using UnityEngine;

    public class ExcelExportToAsset
    {
        public void Generate(string filePath, SaveJsonPathType pathType)
        {
            try
            {
                using var file      = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var excelData = ExcelReaderFactory.CreateOpenXmlReader(file);
                var       dataSet   = excelData.AsDataSet();
                var       sheet     = dataSet.Tables[0];

                var data = new ExcelMiddleData();
                data.Init(sheet, filePath);

                string tempName = Path.GetFileNameWithoutExtension(filePath);
                tempName = tempName.Replace("t_", "");
                string json = data.ToJson();
                // Debug.Log(json);

                string jsonDir;
                if (pathType == SaveJsonPathType.Both)
                {
                    // Resources
                    jsonDir = TableHelper.GetGenerateAssetPathPath(SaveJsonPathType.Resources, ConvertFileType.Excel);
                    string targetFilePath = jsonDir + tempName + ".json";
                    SaveFile(json, targetFilePath);

                    // StreamingAssets
                    jsonDir        = TableHelper.GetGenerateAssetPathPath(SaveJsonPathType.StreamingAssets, ConvertFileType.Excel);
                    targetFilePath = jsonDir + tempName + ".json";
                    SaveFile(json, targetFilePath);
                }
                else
                {
                    jsonDir = TableHelper.GetGenerateAssetPathPath(pathType, ConvertFileType.Excel);
                    string targetFilePath = $"{jsonDir}{tempName}.json";
                    SaveFile(json, targetFilePath);
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

            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(str);
                }
            }

            Debug.Log("资产创建成功: " + filePath);
            AssetDatabase.Refresh();
        }
    }
}