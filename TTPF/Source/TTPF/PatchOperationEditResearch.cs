using System;
using System.Collections;
using System.Linq;
using System.Xml;
using Verse;
using TTPF;

namespace VESSP
{

    /*
    * A custom patch operation to simplify sequence patch operations when defensively adding fields
    * Code by Lanilor (https://github.com/Lanilor)
    * This code is provided "as-is" without any warrenty whatsoever. Use it on your own risk.
    */
    public class PatchOperationEditResearch : PatchOperationPathed
    {
        public string doesRequire;
        public XmlContainer researchViewX;
        public XmlContainer researchViewY;
        public XmlContainer tab;

        protected override bool ApplyWorker(XmlDocument xml)
        {

            if (!string.IsNullOrWhiteSpace(doesRequire))
            {
                string doesRequire = this.doesRequire;
                char[] chArray = new char[1] { ',' };
                foreach (string str in doesRequire.Split(chArray))
                {
                    if (!ModsConfig.IsActive(str.Trim()))
                        return true;
                }
            }

            string researchViewX = this.researchViewX?.node.InnerText;
            string researchViewY = this.researchViewY?.node.InnerText;
            string tab = this.tab?.node.InnerText;
            bool result = false;

            foreach (XmlNode parentNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray<XmlNode>())
            {
                result = true;

                ReplaceNode(parentNode, "researchViewX", researchViewX);
                ReplaceNode(parentNode, "researchViewY", researchViewY);
                ReplaceNode(parentNode, "tab", tab);
            }

            return result;
        }

        private void ReplaceNode(XmlNode parentNode, string nodeName, string value)
        {
            if(String.IsNullOrWhiteSpace(value))
                return; 

            XmlNode node = parentNode.SelectSingleNode(nodeName);
            if (node != null)
            {
                node.InnerText = value;
            }
            else
            {
                XmlNode newnode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, nodeName, "");
                newnode.InnerText = value;
                parentNode.AppendChild(newnode);
            }            
        }

    }



}