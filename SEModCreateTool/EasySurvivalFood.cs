using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SEModCreateTool
{
    /// <summary>
    /// EasySurvivalFood
    /// </summary>
    internal class EasySurvivalFood : EasySurvivalCreateBase
    {


        /// <summary>
        /// データ作成
        /// </summary>
        public override void Create(string srcPath, string dstPath)
        {
            //読込ファイル
            string srcFile = System.IO.Path.Combine(srcPath, @"Blueprints_Food.sbc");

            var factorList = new List<decimal>();
            factorList.Add(10.0M);
            factorList.Add(100.0M);
            factorList.Add(1000.0M);

            foreach (decimal factor in factorList)
            {
                string dstFilename1 = @$"Blueprints_Food_{factor.ToString("0")}.sbc";
#if DEBUG
                string dstPath1 = @"";
#else
                string dstPath1 = @$"Easy_Survival Food x {factor.ToString("0")}\Data";
#endif
                this.CreateSbc(srcFile, dstPath, dstPath1, dstFilename1, factor);
            }
        }

        /// <summary>
        ///データ加工
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        public void CreateSbc(string srcFile,
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

            //Blueprints
            XmlNode? nodeBlueprints = this.GetTargetXmlNode(srcXmlDoc, dstXmlDoc, @"/Definitions/Blueprints", nodeDefinition);
            if (nodeBlueprints == null)
            {
                return;
            }

            //対象SubtypeIdをリストアップ
            var tgtSubtypeId = new SortedDictionary<string, bool>();
            tgtSubtypeId.Add("Position0010_CookMammalMeat", true);
            tgtSubtypeId.Add("Position0020_CookSpiderMeat", true);
            tgtSubtypeId.Add("Position0030_MealPack_KelpCrisp", true);
            tgtSubtypeId.Add("Position0040_MealPack_FruitBar", true);
            tgtSubtypeId.Add("Position0050_MealPack_GardenSlaw", true);
            tgtSubtypeId.Add("Position0060_MealPack_RedPellets", true);
            tgtSubtypeId.Add("Position0070_MealPack_Chili", true);
            tgtSubtypeId.Add("Position0080_MealPack_Flatbread", true);
            tgtSubtypeId.Add("Position0090_MealPack_Ramen", true);
            tgtSubtypeId.Add("Position0100_MealPack_FruitPastry", true);
            tgtSubtypeId.Add("Position0110_MealPack_VeggieBurger", true);
            tgtSubtypeId.Add("Position0120_MealPack_Curry", true);
            tgtSubtypeId.Add("Position0130_MealPack_GreenPellets", true);
            tgtSubtypeId.Add("Position0140_MealPack_Dumplings", true);
            tgtSubtypeId.Add("Position0150_MealPack_Spaghetti", true);
            tgtSubtypeId.Add("Position0160_MealPack_Lasagna", true);
            tgtSubtypeId.Add("Position0170_MealPack_Burrito", true);
            tgtSubtypeId.Add("Position0180_MealPack_FrontierStew", true);
            tgtSubtypeId.Add("Position0190_MealPack_SearedSabiroid", true);
            tgtSubtypeId.Add("Position0200_MealPack_SteakDinner", true);

            tgtSubtypeId.Add("Position0010_Seeds_Fruit", true);
            tgtSubtypeId.Add("Position0020_Seeds_Grain", true);
            tgtSubtypeId.Add("Position0030_Seeds_Vegetables", true);
            tgtSubtypeId.Add("Position0040_Spores_Mushrooms", true);

            //Blueprint
            {
                XmlNodeList? nodeList = srcXmlDoc.SelectNodes("/Definitions/Blueprints/Blueprint");
                if (nodeList == null)
                {
                    return;
                }
                foreach (XmlNode node in nodeList)
                {
                    //SubtypeIdを取得
                    var subtypeId = node.SelectSingleNode("Id/SubtypeId")?.InnerText.ToString();
                    if (string.IsNullOrWhiteSpace(subtypeId))
                    {
                        subtypeId = "";
                    }

                    //対象外のsubtypeIdか判定
                    if (!tgtSubtypeId.ContainsKey(subtypeId))
                    {
                        //対象外のsubtypeId
                        continue;
                    }

                    //要素を取り込み
                    XmlNode nodeBlueprint = dstXmlDoc.ImportNode(node, true);
                    nodeBlueprints.AppendChild(nodeBlueprint);

                    //データ加工
                    this.ChangeItemAmount(subtypeId, nodeBlueprint, factor, tgtSubtypeId[subtypeId]);
                }
            }

            //XML保存
            dstXmlDoc.Save(dstFile);
        }


        /// <summary>
        /// Item Amountをn倍
        /// </summary>
        /// <param name="blueprint"></param>
        /// <param name="factor"></param>
        private void ChangeItemAmount(string subtypeId,
                                      XmlNode nodeBlueprint,
                                      decimal factor,
                                      bool isQuantityOnly)
        {

            decimal newFactor = factor;



            ////必要数を1にする
            //{
            //    bool isPrerequisitesOk = false;

            //    XmlNodeList? items = nodeBlueprint.SelectNodes("Prerequisites/Item");
            //    if (items == null)
            //    {
            //        return;
            //    }
            //    if (items.Count == 0)
            //    {
            //        items = nodeBlueprint.SelectNodes("Prerequisites");
            //        if (items == null)
            //        {
            //            return;
            //        }
            //    }
            //    foreach (XmlNode item in items)
            //    {
            //        if (item == null)
            //        {
            //            continue;
            //        }
            //        var attrs = item.Attributes;
            //        if (attrs == null)
            //        {
            //            continue;
            //        }
            //        var attr = attrs["Amount"];
            //        if (attr == null)
            //        {
            //            continue;
            //        }

            //        string? amountText = attr.Value;
            //        if (string.IsNullOrWhiteSpace(amountText))
            //        {
            //            continue;
            //        }

            //        decimal amount;
            //        if (decimal.TryParse(amountText, out amount))
            //        {
            //            if (amount == 1.0M)
            //            {
            //                //すでに1のためそのまま
            //                isPrerequisitesOk = true;
            //            }
            //            else
            //            {
            //                //1にする
            //                isPrerequisitesOk = true;

            //                attr.Value = "1";

            //                //出力の倍率を計算する
            //                newFactor = factor * (1.0M / amount);

            //            }
            //        }
            //        break;
            //    }
            //    if (!isPrerequisitesOk)
            //    {
            //        //エラー
            //        return;
            //    }
            //}




            //出力週を調整
            {
                XmlNodeList? items = nodeBlueprint.SelectNodes("Results/Item");
                if (items == null)
                {
                    return;
                }
                if (items.Count == 0)
                {
                    items = nodeBlueprint.SelectNodes("Result");
                    if (items == null)
                    {
                        return;
                    }
                }
                foreach (XmlNode item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    var attrs = item.Attributes;
                    if (attrs == null)
                    {
                        continue;
                    }
                    var attr = attrs["Amount"];
                    if (attr == null)
                    {
                        continue;
                    }

                    string? amountText = attr.Value;
                    if (string.IsNullOrWhiteSpace(amountText))
                    {
                        continue;
                    }

                    decimal amount;
                    if (decimal.TryParse(amountText, out amount))
                    {
                        amount = amount * newFactor;
                        attr.Value = Math.Round(amount).ToString();
                    }
                }
            }
        }

    }
}
