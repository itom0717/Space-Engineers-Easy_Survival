using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SEModCreateTool
{


    /// <summary>
    /// EasySurvival
    /// </summary>
    internal class EasySurvival
    {
        /// <summary>
        /// Create
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        public void Create(string srcPath, string dstPath1)
        {

            string srcFile = System.IO.Path.Combine(srcPath, "Blueprints.sbc");

            {
                const decimal factor = 10.0M;
                string dstFilename = @$"Blueprints_{factor.ToString("0")}.sbc";
#if DEBUG
                const string dstPath2 = @"";
#else
                const string dstPath2 = @"Easy_Survival x 10\Data";
//                const string dstFilename = @"Blueprints.sbc";
#endif
                this.CreateBlueprintsSbc(srcFile, dstPath1, dstPath2, dstFilename, factor, true, false);
            }
            {
                const Decimal factor = 100.0M;
                string dstFilename = @$"Blueprints_{factor.ToString("0")}.sbc";
#if DEBUG
                const string dstPath2 = @"";
#else
                const string dstPath2 = @"Easy_Survival x 100\Data";
//                const string dstFilename = @"Blueprints.sbc";
#endif
                this.CreateBlueprintsSbc(srcFile, dstPath1, dstPath2, dstFilename, factor, true, false);
            }

            {
                const Decimal factor = 1000.0M;
                string dstFilename = @$"Blueprints_{factor.ToString("0")}.sbc";
#if DEBUG
                const string dstPath2 = @"";
#else
                const string dstPath2 = @"Easy_Survival x 1000\Data";
//                const string dstFilename = @"Blueprints.sbc";
#endif
                this.CreateBlueprintsSbc(srcFile, dstPath1, dstPath2, dstFilename, factor, true, false);
            }



            {
                const Decimal factor = 1000.0M;
                string dstFilename = @$"Blueprints_{factor.ToString("0")}_Only_Quantity.sbc";
#if DEBUG
                const string dstPath2 = @"";
#else
                const string dstPath2 = @"Easy_Survival_Only_Quantity\Data";
//                const string dstFilename = @"Blueprints.sbc";
#endif
                this.CreateBlueprintsSbc(srcFile, dstPath1, dstPath2, dstFilename, factor, true, true);
            }

        }

        /// <summary>
        ///データ加工
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        public void CreateBlueprintsSbc(string srcFile,
                                        string dstPath1,
                                        string dstPath2,
                                        string dstFilename,
                                        decimal factor,
                                        bool isResultsOnly,
                                        bool isQuantityOnly)
        {
            //出力フォルダ
            string dstPath = System.IO.Path.Combine(dstPath1, dstPath2);
            if (!System.IO.Directory.Exists(dstPath))
            {
                System.IO.Directory.CreateDirectory(dstPath);
            }
            //出力ファイル
            string dstFile = System.IO.Path.Combine(dstPath, dstFilename);

            //Org Xml
            XmlDocument srcXmlDoc = new XmlDocument();
            srcXmlDoc.Load(srcFile);

            //保存XML
            XmlDocument dstXmlDoc = new XmlDocument();

            // XML宣言の生成
            {
                XmlDeclaration declaration = dstXmlDoc.CreateXmlDeclaration(@"1.0", @"", null);
                dstXmlDoc.AppendChild(declaration);
            }

            //Definitions
            XmlNode? nodeDefinition = null;
            {
                XmlNodeList? nodeList = srcXmlDoc.SelectNodes("/Definitions");
                if (nodeList == null)
                {
                    return;
                }
                foreach (XmlNode node in nodeList)
                {
                    nodeDefinition = dstXmlDoc.ImportNode(node, false);
                    dstXmlDoc.AppendChild(nodeDefinition);
                    break;
                }
            }
            if (nodeDefinition == null)
            {
                return;
            }

            //Blueprints
            XmlNode? nodeBlueprints = null;
            {
                XmlNodeList? nodeList = srcXmlDoc.SelectNodes("/Definitions/Blueprints");
                if (nodeList == null)
                {
                    return;
                }
                foreach (XmlNode node in nodeList)
                {
                    nodeBlueprints = dstXmlDoc.ImportNode(node, false);
                    nodeDefinition.AppendChild(nodeBlueprints);
                    break;
                }
            }
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
