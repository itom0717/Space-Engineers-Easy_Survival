using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SEModCreateTool
{
    /// <summary>
    /// EasySurvivalCreateBlueprints
    /// </summary>
    internal class EasySurvivalCreateBlueprints : EasySurvivalCreateBase
    {
        /// <summary>
        /// データ作成
        /// </summary>
        public override void Create(string srcPath, string dstPath)
        {
            //読込ファイル
            string srcFile = System.IO.Path.Combine(srcPath, @"Blueprints.sbc");

            var factorList = new List<decimal>();
            factorList.Add(10.0M);
            factorList.Add(100.0M);
            factorList.Add(1000.0M);

            foreach (decimal factor in factorList)
            {
                string dstFilename1 = @$"Blueprints_{factor.ToString("0")}.sbc";
                string dstFilename2 = @$"Blueprints_{factor.ToString("0")}_Only_Quantity.sbc";
#if DEBUG
                string dstPath1 = @"";
                string dstPath2 = @"";
#else
                string dstPath1 = @$"Easy_Survival x {factor.ToString("0")}\Data";
                string dstPath2 = @$"Easy_Survival x {factor.ToString("0")} Only Quantity\Data";
#endif
                this.CreateSbc(srcFile, dstPath, dstPath1, dstFilename1, factor, true, false);
                this.CreateSbc(srcFile, dstPath, dstPath2, dstFilename2, factor, true, true);
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
                              decimal factor,
                              bool isResultsOnly,
                              bool isQuantityOnly)
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
            tgtSubtypeId.Add("StoneOreToIngotBasic", isQuantityOnly);
            tgtSubtypeId.Add("StoneOreToIngot", isQuantityOnly);
            tgtSubtypeId.Add("IronOreToIngot", true);
            tgtSubtypeId.Add("NickelOreToIngot", true);
            tgtSubtypeId.Add("CobaltOreToIngot", true);
            tgtSubtypeId.Add("MagnesiumOreToIngot", true);
            tgtSubtypeId.Add("SiliconOreToIngot", true);
            tgtSubtypeId.Add("SilverOreToIngot", true);
            tgtSubtypeId.Add("PlatinumOreToIngot", true);
            tgtSubtypeId.Add("UraniumOreToIngot", true);

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
                    this.ChangeItemAmount(subtypeId, nodeBlueprint, factor, isResultsOnly, tgtSubtypeId[subtypeId]);
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
                                      bool isResultsOnly,
                                      bool isQuantityOnly)
        {

            decimal newFactor = factor;


            if (!isResultsOnly)
            {
                //必要数を1にする
                {
                    bool isPrerequisitesOk = false;

                    XmlNodeList? items = nodeBlueprint.SelectNodes("Prerequisites/Item");
                    if (items == null)
                    {
                        return;
                    }
                    if (items.Count == 0)
                    {
                        items = nodeBlueprint.SelectNodes("Prerequisites");
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
                            if (amount == 1.0M)
                            {
                                //すでに1のためそのまま
                                isPrerequisitesOk = true;
                            }
                            else
                            {
                                //1にする
                                isPrerequisitesOk = true;

                                attr.Value = "1";

                                //出力の倍率を計算する
                                newFactor = factor * (1.0M / amount);

                            }
                        }
                        break;
                    }
                    if (!isPrerequisitesOk)
                    {
                        //エラー
                        return;
                    }
                }
            }



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
                        attr.Value = ((double)amount).ToString();
                    }
                }
            }

            if (isQuantityOnly)
            {
                //個数のみはここで抜ける
                return;
            }

            //subtypeId
            if (subtypeId == "StoneOreToIngot")
            {
                XmlNodeList? results = nodeBlueprint.SelectNodes("Results");
                if (results == null)
                {
                    return;
                }
                var result = results[0];
                if (result == null)
                {
                    return;
                }
                var doc = nodeBlueprint.OwnerDocument;
                if (doc == null)
                {
                    return;
                }

                //Ice
                {
                    decimal amount = 10.0M;
                    amount = amount * factor;

                    XmlElement item = doc.CreateElement("Item");
                    item.SetAttribute("Amount", ((double)amount).ToString());
                    item.SetAttribute("TypeId", "Ore");
                    item.SetAttribute("SubtypeId", "Ice");
                    result.AppendChild(item);
                }

                //Cobalt
                {
                    decimal amount = 0.8M;
                    amount = amount * factor;

                    XmlElement item = doc.CreateElement("Item");
                    item.SetAttribute("Amount", ((double)amount).ToString());
                    item.SetAttribute("TypeId", "Ingot");
                    item.SetAttribute("SubtypeId", "Cobalt");
                    result.AppendChild(item);
                }

                //Magnesium
                {
                    decimal amount = 0.5M;
                    amount = amount * factor;

                    XmlElement item = doc.CreateElement("Item");
                    item.SetAttribute("Amount", ((double)amount).ToString());
                    item.SetAttribute("TypeId", "Ingot");
                    item.SetAttribute("SubtypeId", "Magnesium");
                    result.AppendChild(item);
                }

                //Silver
                {
                    decimal amount = 5.0M;
                    amount = amount * factor;

                    XmlElement item = doc.CreateElement("Item");
                    item.SetAttribute("Amount", ((double)amount).ToString());
                    item.SetAttribute("TypeId", "Ingot");
                    item.SetAttribute("SubtypeId", "Silver");
                    result.AppendChild(item);
                }

                //Gold
                {
                    decimal amount = 0.3M;
                    amount = amount * factor;

                    XmlElement item = doc.CreateElement("Item");
                    item.SetAttribute("Amount", ((double)amount).ToString());
                    item.SetAttribute("TypeId", "Ingot");
                    item.SetAttribute("SubtypeId", "Gold");
                    result.AppendChild(item);
                }

                //Platinum
                {
                    decimal amount = 0.1M;
                    amount = amount * factor;

                    XmlElement item = doc.CreateElement("Item");
                    item.SetAttribute("Amount", ((double)amount).ToString());
                    item.SetAttribute("TypeId", "Ingot");
                    item.SetAttribute("SubtypeId", "Platinum");
                    result.AppendChild(item);
                }

                //Uranium
                {
                    decimal amount = 0.5M;
                    amount = amount * factor;

                    XmlElement item = doc.CreateElement("Item");
                    item.SetAttribute("Amount", ((double)amount).ToString());
                    item.SetAttribute("TypeId", "Ingot");
                    item.SetAttribute("SubtypeId", "Uranium");
                    result.AppendChild(item);
                }
            }
        }


    }
}
