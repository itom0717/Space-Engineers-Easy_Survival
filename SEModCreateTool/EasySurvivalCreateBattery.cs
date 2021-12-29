using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SEModCreateTool
{
    /// <summary>
    /// EasySurvivalCreateBattery
    /// </summary>
    internal class EasySurvivalCreateBattery : EasySurvivalCreateBase
    {
        /// <summary>
        /// データ作成
        /// </summary>
        public override void Create(string srcPath, string dstPath)
        {
            //読込ファイル
            string srcFile = System.IO.Path.Combine(srcPath, @"CubeBlocks", @"CubeBlocks_Energy.sbc");

            var factorList = new List<decimal>();
            factorList.Add(10.0M);
            factorList.Add(100.0M);
            factorList.Add(1000.0M);

            foreach (decimal factor in factorList)
            {
                string dstFilename = @$"Battery_{factor.ToString("0")}.sbc";
#if DEBUG
                string dstPath1 = @"";
#else
                string dstPath1 = @$"Easy_Survival Battery x {factor.ToString("0")}\Data";
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

            //保存XML
            XmlDocument dstXmlDoc = this.GetDestinationXmlDocument();

            //Definitionsを調べてdstに追加
            XmlNode? nodeDefinition = this.GetDefinitionsXmlNode(srcXmlDoc, dstXmlDoc);
            if (nodeDefinition == null)
            {
                return;
            }

            //CubeBlocks
            XmlNode? nodeCubeBlocks = this.GetTargetXmlNode(srcXmlDoc, dstXmlDoc, @"/Definitions/CubeBlocks", nodeDefinition);
            if (nodeCubeBlocks == null)
            {
                return;
            }

            //対象SubtypeIdをリストアップ
            var tgIds = new SortedDictionary<string, bool>();
            tgIds.Add("OxygenGenerator\t", true);
            tgIds.Add("OxygenGenerator\tOxygenGeneratorSmall", true);


            //CubeBlocks
            {
                XmlNodeList? nodeList = srcXmlDoc.SelectNodes("/Definitions/CubeBlocks/Definition");
                if (nodeList == null)
                {
                    return;
                }
                foreach (XmlNode node in nodeList)
                {
                    //TypeIdを取得
                    var typeId = node.SelectSingleNode("Id/TypeId")?.InnerText.ToString();
                    if (string.IsNullOrWhiteSpace(typeId))
                    {
                        typeId = "";
                    }
                    if (string.IsNullOrWhiteSpace(typeId))
                    {
                        //対象外のtypeId
                        continue;
                    }
                    //SubtypeIdを取得
                    var subtypeId = node.SelectSingleNode("Id/SubtypeId")?.InnerText.ToString();
                    if (string.IsNullOrWhiteSpace(subtypeId))
                    {
                        subtypeId = "";
                    }


                    //対象外のtypeId+subtypeIdか判定
                    if (!tgIds.ContainsKey($"{typeId}\t{subtypeId}"))
                    {
                        //対象外のtypeId+subtypeId
                        continue;
                    }

                    //要素を取り込み
                    XmlNode nodeCubeBlock = dstXmlDoc.ImportNode(node, true);



                    XmlNodeList? nodeListIceToGasRatio = nodeCubeBlock.SelectNodes("ProducedGases/GasInfo/IceToGasRatio");
                    if (nodeListIceToGasRatio == null || nodeListIceToGasRatio.Count <= 0)
                    {
                        continue;
                    }

                    //出力週を調整
                    foreach (XmlNode gasRateNode in nodeListIceToGasRatio)
                    {
                        string valueText = gasRateNode.InnerText;
                        if (string.IsNullOrWhiteSpace(valueText))
                        {
                            continue;
                        }

                        decimal gasRate;
                        if (decimal.TryParse(valueText, out gasRate))
                        {
                            gasRate = gasRate * factor;
                            gasRateNode.InnerText = ((double)gasRate).ToString();
                        }
                    }

                    //追加
                    nodeCubeBlocks.AppendChild(nodeCubeBlock);

                }
            }

            //XML保存
            dstXmlDoc.Save(dstFile);
        }




    }
}
