using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SEModCreateTool
{
    /// <summary>
    /// EasySurvivalCreateBase
    /// </summary>
    internal class EasySurvivalCreateBase
    {
        /// <summary>
        /// データ作成
        /// </summary>
        public virtual void Create(string srcPath, string dstPath)
        {
            //virtual
        }

        /// <summary>
        /// 出力フォルダを返す
        /// </summary>
        /// <returns></returns>
        protected string GetDestinationFile(string dstPath1,string  dstPath2, string dstFilename)
        {
            //出力フォルダ
            string dstPath = System.IO.Path.Combine(dstPath1, dstPath2);
            if (!System.IO.Directory.Exists(dstPath))
            {
                System.IO.Directory.CreateDirectory(dstPath);
            }
            //出力ファイル
            string dstFile = System.IO.Path.Combine(dstPath, dstFilename);

            return dstFile;
        }

        /// <summary>
        /// ソースXMLを返す
        /// </summary>
        /// <returns></returns>
        protected XmlDocument GetSorceXmlDocument(string srcFile)
        {
            XmlDocument srcXmlDoc = new XmlDocument();
            srcXmlDoc.Load(srcFile);

            return srcXmlDoc;
        }


        /// <summary>
        /// 保存用XMLを返す
        /// </summary>
        /// <returns></returns>
        protected XmlDocument GetDestinationXmlDocument()
        {
            //保存XML
            XmlDocument dstXmlDoc = new XmlDocument();

            // XML宣言の生成
            {
                XmlDeclaration declaration = dstXmlDoc.CreateXmlDeclaration(@"1.0", @"", null);
                dstXmlDoc.AppendChild(declaration);
            }

            return dstXmlDoc;
        }

        /// <summary>
        /// Definitionsノードを取得及び出力XMLに追加
        /// </summary>
        /// <returns></returns>
        protected XmlNode? GetDefinitionsXmlNode(XmlDocument srcXmlDoc, XmlDocument dstXmlDoc)
        {
            XmlNode? nodeDefinition = null;
            XmlNodeList? nodeList = srcXmlDoc.SelectNodes("/Definitions");
            if (nodeList == null)
            {
                return null;
            }

            foreach (XmlNode node in nodeList)
            {
                nodeDefinition = dstXmlDoc.ImportNode(node, false);
                dstXmlDoc.AppendChild(nodeDefinition);
                break;
            }
            return nodeDefinition;
        }

        /// <summary>
        /// targetノードを取得及び出力XMLに追加
        /// </summary>
        /// <returns></returns>
        protected XmlNode? GetTargetXmlNode(XmlDocument srcXmlDoc, XmlDocument dstXmlDoc, string targetNode, XmlNode appendNode)
        {
            XmlNode? nodeTarget = null;
            XmlNodeList? nodeList = srcXmlDoc.SelectNodes(targetNode);
            if (nodeList == null)
            {
                return null;
            }

            foreach (XmlNode node in nodeList)
            {
                nodeTarget = dstXmlDoc.ImportNode(node, false);
                appendNode.AppendChild(nodeTarget);
                break;
            }
            return nodeTarget;
        }


        /// <summary>
        /// valueを変更
        /// </summary>
        protected void ChangeValue(XmlDocument tgtXmlDoc, string xPath, string value)
        {
            XmlNodeList? nodeList = tgtXmlDoc.SelectNodes(xPath);
            if (nodeList == null || nodeList.Count <= 0)
            {
                return;
            }

            foreach (XmlNode node in nodeList)
            {
                node.InnerText = value;
            }
        }


        /// <summary>
        /// valueを変更(値直接指定)
        /// </summary>
        protected void ChangeValue(XmlNode tgtNode, string xPath, string value)
        {
            XmlNodeList? nodeList = tgtNode.SelectNodes(xPath);
            if (nodeList == null || nodeList.Count <= 0)
            {
                return;
            }

            foreach (XmlNode node in nodeList)
            {
                node.InnerText = value;
            }
        }

        /// valueを変更(係数指定)
        /// </summary>
        protected void ChangeValue(XmlNode tgtNode, string xPath, decimal factor)
        {
            XmlNodeList? nodeList = tgtNode.SelectNodes(xPath);
            if (nodeList == null || nodeList.Count <= 0)
            {
                return;
            }


            foreach (XmlNode node in nodeList)
            {
                string valueText = node.InnerText;
                if (string.IsNullOrWhiteSpace(valueText))
                {
                    continue;
                }

                decimal value;
                if (decimal.TryParse(valueText, out value))
                {
                    value = value * factor;
                    node.InnerText = ((double)value).ToString();
                }

            }
        }


    }
}
