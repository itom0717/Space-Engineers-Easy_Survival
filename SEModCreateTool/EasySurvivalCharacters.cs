using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SEModCreateTool
{
    /// <summary>
    /// EasySurvivalCharacters
    /// </summary>
    internal class EasySurvivalCharacters : EasySurvivalCreateBase
    {
        /// <summary>
        /// データ作成
        /// </summary>
        public override void Create(string srcPath, string dstPath)
        {
            //読込ファイル
            string srcFile = System.IO.Path.Combine(srcPath, @"Characters.sbc");
            var factorList = new List<decimal>();
            factorList.Add(10.0M);
            factorList.Add(100.0M);
            factorList.Add(1000.0M);



            foreach (decimal factor in factorList)
            {
                string dstFilename = @$"EasyCharacters_{factor.ToString("0")}.sbc";
#if DEBUG
                string dstPath1 = @"";
#else
                    string dstPath1 = @$"Easy_Survival Character x {factor.ToString("0")}\Data";
#endif
                this.CreateSbc(srcFile, dstPath, dstPath1, dstFilename, factor);
            }



        }

        /// <summary>
        ///データ加工
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        private void CreateSbc(string srcFile,
                               string dstPath1,
                               string dstPath2,
                               string dstFilename,
                               decimal factor)
        {
            //出力ファイル
            string dstFile = this.GetDestinationFile(dstPath1, dstPath2, dstFilename);

            //ソースXML
            XmlDocument srcXmlDoc = this.GetSorceXmlDocument(srcFile);

            //InventoryVolume
            {
                string xPath = @"/Definitions/Characters/Character/Inventory/InventoryVolume";
                this.ChangeValue(srcXmlDoc, xPath, factor);
            }

            //Hydrogen suit MaxCapacity
            //Oxygen suit MaxCapacity
            {
                string xPath = @"/Definitions/Characters/Character/SuitResourceStorage/Resource/MaxCapacity";
                string value = @"500";
                this.ChangeValue(srcXmlDoc, xPath, value);
            }

            //MaxPowerConsumption
            {
                string xPath = @"/Definitions/Characters/Character/Jetpack/ThrustProperties/MaxPowerConsumption";
                string value = @"0.00001";
                this.ChangeValue(srcXmlDoc, xPath, value);
            }

            //ConsumptionFactorPerG
            {
                string xPath = @"/Definitions/Characters/Character/Jetpack/ThrustProperties/ConsumptionFactorPerG";
                string value = @"25";
                this.ChangeValue(srcXmlDoc, xPath, value);
            }

            //SuitConsumptionInTemperatureExtreme
            {
                string xPath = @"/Definitions/Characters/Character/SuitConsumptionInTemperatureExtreme";
                string value = @"0.1";
                this.ChangeValue(srcXmlDoc, xPath, value);
            }

            //XML保存
            srcXmlDoc.Save(dstFile);
        }


    }
}
