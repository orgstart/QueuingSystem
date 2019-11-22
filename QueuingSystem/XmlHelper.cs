using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace QueuingSystem
{
    /// <summary>
    /// xml操作
    /// </summary>
    public class XmlHelper
    {
        public XmlHelper()
        { }
        private static string convert_stream_xml(Stream stream)
        {
            string outcome = "";
            XmlReader xr = XmlReader.Create(stream);

            return outcome;
        }



        public static Dictionary<string, string> Read_Quest_ResultXml(string strXml)
        {
            Dictionary<string, string> outcome = new Dictionary<string, string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns_head = doc.SelectSingleNode("/root/head").ChildNodes;
                foreach (XmlNode xn in xns_head)
                {
                    outcome.Add(xn.Name, decodeUTF8(xn.InnerText));
                }
                XmlNodeList xns_body = doc.SelectSingleNode("/root/body").ChildNodes;
                foreach (XmlNode xn in xns_body)
                {
                    outcome.Add(xn.Name, decodeUTF8(xn.InnerText));
                }
                return outcome;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解析调用公安网接口查询驾驶证号的xml字符串
        /// </summary>
        /// <param name="strXml"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public static DataTable Read_JSZHInfo(string strXml, string nodeName, out string message)
        {
            message = "";
            try
            {
                Dictionary<string, string> tempDic = new Dictionary<string, string>();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns_head = doc.SelectSingleNode("/root/head").ChildNodes;
                foreach (XmlNode xn in xns_head)
                {
                    tempDic.Add(xn.Name, decodeUTF8(xn.InnerText));
                }
                if (tempDic["code"] == "1" && Convert.ToInt32(tempDic["rownum"].ToString()) > 0) //驾驶证号信息获取条数大于0
                {
                    DataTable outcome = new DataTable();
                    XmlNodeList nodes = doc.SelectNodes("/root/body/" + nodeName);
                    int i = 0;
                    foreach (XmlNode xn in nodes)  //drv 可能有多条记录
                    {
                        List<object> obj = new List<object>();
                        XmlNodeList chs = xn.ChildNodes;
                        foreach (XmlNode ch in chs)
                        {
                            if (i == 0)
                            {
                                outcome.Columns.Add(new DataColumn() { ColumnName = ch.Name.ToUpper() }); //表添加列
                            }
                            obj.Add(decodeUTF8(ch.InnerText));
                        }
                        outcome.Rows.Add(obj.ToArray());
                        i++;
                    }
                    return outcome;
                }
                else
                {
                    message = tempDic["message"].ToString();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 解析车辆信息/驾驶证信息等
        /// </summary>
        /// <param name="strXml"></param>
        /// <param name="nodeName">节点标签</param>
        /// <returns></returns>
        public static Dictionary<string, string> Read_QueryXmlDoc_ByNode(string strXml, string nodeName)
        {
            Dictionary<string, string> outcome = new Dictionary<string, string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns_head = doc.SelectSingleNode("/root/head").ChildNodes;
                foreach (XmlNode xn in xns_head)
                {
                    outcome.Add(xn.Name, decodeUTF8(xn.InnerText));
                }
                if (outcome["code"] == "1" && outcome["rownum"] != "0")
                {
                    XmlNodeList xns_body = doc.SelectSingleNode("/root/body/" + nodeName + "").ChildNodes;
                    foreach (XmlNode xn in xns_body)
                    {
                        outcome.Add(xn.Name, decodeUTF8(xn.InnerText));
                    }
                }
                return outcome;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取机动车对应需拍摄照片和人工检验项目信息
        /// </summary>
        /// <param name="strXml"></param>
        /// <returns></returns>
        public static Dictionary<string, string> CheckInfo_Read_Quest_ResultXml(string strXml)
        {
            Dictionary<string, string> outcome = new Dictionary<string, string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns_head = doc.SelectSingleNode("/root/head").ChildNodes;
                foreach (XmlNode xn in xns_head)
                {
                    outcome.Add(xn.Name, decodeUTF8(xn.InnerText));
                }
                if (outcome["code"] == "1")
                {
                    if (outcome["rownum"] == "1")
                    {
                        XmlNodeList xns_body = doc.SelectSingleNode("/root/body/vehispara").ChildNodes;
                        foreach (XmlNode xn in xns_body)
                        {
                            outcome.Add(xn.Name, decodeUTF8(xn.InnerText));
                        }
                    }
                }
                return outcome;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 读取向公安网写入xml的返回结果的head下的所有节点 
        /// </summary>
        /// <param name="strXml"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Read_ResultXml_AllNodes(string strXml)
        {
            try
            {
                Dictionary<string, string> outcome = new Dictionary<string, string>();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns = doc.SelectSingleNode("/root/head").ChildNodes;
                foreach (XmlNode xn in xns)
                {
                    outcome.Add(xn.Name, decodeUTF8(xn.InnerText));
                }
                return outcome;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 读取写入类返回结果文档ResultXML的code和message内容
        /// </summary>
        /// <param name="strXml"></param>
        /// <returns></returns>
        public static string[] Read_ResultXml(string strXml)
        {
            try
            {
                string[] outcome = new string[2];
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns = doc.SelectSingleNode("/root/head").ChildNodes;
                outcome[0] = xns.Item(0).InnerText;
                if (outcome[0].Contains('%'))
                {
                    outcome[0] = HttpUtility.UrlDecode(outcome[0], Encoding.UTF8);
                }
                if (!string.IsNullOrEmpty(xns.Item(1).InnerText))
                {
                    string mes = HttpUtility.UrlDecode(xns.Item(1).InnerText, Encoding.UTF8);
                    outcome[1] = mes;
                }
                return outcome;
            }

            catch
            {
                return null;
            }
        }


        /// <summary>
        /// 将中文字符转换为UTF8格式
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static string encodeUTF8(string xmlDoc)
        {
            try
            {
                string str = HttpUtility.UrlEncode(xmlDoc, Encoding.UTF8);
                return str;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 将UTF8格式字符串转换为中文字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string decodeUTF8(string str)
        {
            try
            {
                string xml = HttpUtility.UrlDecode(str, Encoding.UTF8);
                return xml;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 查询指定的属性的值
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="node"></param>
        /// <param name="attribute_dis"></param>
        /// <param name="value_dis"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string Read_1(string strPath, string node, string attribute_dis, string value_dis, string attribute)
        {
            string outcome = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(strPath);
                XmlNodeList xns = doc.SelectNodes(node);
                if (string.IsNullOrEmpty(attribute_dis))
                    foreach (XmlNode xn in xns)
                    {
                        if (string.IsNullOrEmpty(attribute))
                        {
                            XmlAttributeCollection attrs = xn.Attributes;
                            foreach (XmlAttribute at in attrs)
                            {

                            }
                        }
                        else
                        {

                        }
                    }
            }
            catch { }
            return outcome;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="strXml">xml格式的字符串</param>
        /// <param name="node">节点</param>
        /// <param name="attribute_dis">属性 筛选用 为空时遍历所有的节点 否则筛选出符合属性值条件的节点</param>
        /// <param name="value_dis">值 筛选用</param>
        /// <param name="attribute">属性名，非空时返回该属性值，否则返回当前节点的所有属性值</param>
        /// <returns>string</returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Read(path, "/Node", "")
         * XmlHelper.Read(path, "/Node/Element[@Attribute='Name']", "Attribute")
         ************************************************/
        public static Dictionary<string, string> Read(string strXml, string node, string attribute_dis, string value_dis, string attribute)
        {
            Dictionary<string, string> dic_attr_value = new Dictionary<string, string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns = doc.SelectNodes(node);
                if (string.IsNullOrEmpty(attribute_dis))
                    foreach (XmlNode xn in xns)
                    {
                        if (string.IsNullOrEmpty(attribute))
                        {
                            XmlAttributeCollection attrs = xn.Attributes;
                            foreach (XmlAttribute at in attrs)
                            {
                                dic_attr_value.Add(at.Name, at.Value);
                            }
                        }
                        else
                        {
                            dic_attr_value.Add(attribute, xn.Attributes[attribute].Value);
                        }
                    }
                else
                    foreach (XmlNode xn in xns)
                    {
                        if (((XmlElement)xn).GetAttribute(attribute_dis) == value_dis)
                        {
                            if (string.IsNullOrEmpty(attribute))
                            {
                                XmlAttributeCollection attrs = xn.Attributes;
                                foreach (XmlAttribute at in attrs)
                                {
                                    dic_attr_value.Add(at.Name, at.Value);
                                }
                            }
                            else
                            {
                                dic_attr_value.Add(attribute, xn.Attributes[attribute].Value);
                            }
                        }
                    }

            }
            catch { }
            return dic_attr_value;
        }


        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute_dis">属性 筛选用 为空时遍历所有的节点 否则筛选出符合属性值的节点</param>
        /// <param name="value_dis">值 筛选用</param>
        /// <param name="element">元素名，非空时插入新元素，否则在该元素中插入属性</param>
        /// <param name="attribute">属性名，非空时插入该元素属性值，否则插入元素值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Insert(path, "/Node", "Element", "", "Value")
         * XmlHelper.Insert(path, "/Node", "Element", "Attribute", "Value")
         * XmlHelper.Insert(path, "/Node", "", "Attribute", "Value")
         ************************************************/
        public static string Insert(string strXml, string node, string attribute_dis, string value_dis, string element, string attribute, string value)
        {
            string outcome = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns = doc.SelectNodes(node);
                if (attribute_dis.Equals(""))
                    foreach (XmlNode xn in xns)
                    {
                        if (element.Equals(""))
                        {
                            if (!attribute.Equals(""))
                            {
                                XmlElement xe = (XmlElement)xn;
                                xe.SetAttribute(attribute, value);
                            }
                        }
                        else
                        {
                            XmlElement xe = doc.CreateElement(element);
                            if (attribute.Equals(""))
                                xe.InnerText = value;
                            else
                                xe.SetAttribute(attribute, value);
                            xn.AppendChild(xe);
                        }
                    }
                else
                    foreach (XmlNode xn in xns)
                    {
                        XmlElement xe = (XmlElement)xn;
                        if (xe.GetAttribute(attribute_dis) == value_dis)
                        {
                            if (element.Equals(""))
                            {
                                if (!attribute.Equals(""))
                                {
                                    xe.SetAttribute(attribute, value);
                                }
                            }
                            else
                            {
                                XmlElement xe1 = doc.CreateElement(element);
                                if (attribute.Equals(""))
                                    xe1.InnerText = value;
                                else
                                    xe1.SetAttribute(attribute, value);
                                xn.AppendChild(xe1);
                            }
                        }
                    }
                outcome = XMLDocumentToString(ref doc);
                return outcome;
            }
            catch { return outcome; }
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute_dis">属性 筛选 为空时修改所有的节点属性 否则修改指定的属性值的节点属性 </param>
        /// <param name="value_dis">值 筛选用</param>
        /// <param name="attribute">属性名，非空时修改该节点属性值，否则修改节点值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Insert(path, "/Node", "", "Value")
         * XmlHelper.Insert(path, "/Node", "Attribute", "Value")
         ************************************************/
        public static string Update(string strXml, string node, string attribute_dis, string value_dis, string attribute, string value)
        {
            string outcome = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns = doc.SelectNodes(node);
                if (attribute_dis.Equals(""))
                    //每一个节点的属性都要设置
                    if (attribute.Equals(""))
                        foreach (XmlNode xn in xns)
                        {
                            ((XmlElement)xn).InnerText = value;
                        }
                    else
                        foreach (XmlNode xn in xns)
                        {
                            ((XmlElement)xn).SetAttribute(attribute, value);
                        }
                else
                    //设置指定节点的属性
                    if (attribute.Equals(""))
                    foreach (XmlNode xn in xns)
                    {
                        XmlElement xe = (XmlElement)xn;
                        if (xe.GetAttribute(attribute_dis) == value_dis)
                            xe.InnerText = value;
                    }
                else
                    foreach (XmlNode xn in xns)
                    {
                        XmlElement xe = (XmlElement)xn;
                        if (xe.GetAttribute(attribute_dis) == value_dis)
                            xe.SetAttribute(attribute, value);
                    }
                outcome = XMLDocumentToString(ref doc);
                return outcome;
            }
            catch (Exception ex) { return outcome; }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute">属性名，为空时删除节点值，否则根据value做处理</param>
        /// <param name="value">值，为空时删除属性 否则根据属性值删除指定节点</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Delete(path, "/Node", "")
         * XmlHelper.Delete(path, "/Node", "Attribute")
         ************************************************/
        public static string Delete(string strXml, string node, string attribute, string value)
        {
            string outcome = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns = doc.SelectNodes(node);
                if (attribute.Equals(""))
                    //删除节点
                    foreach (XmlNode xn in xns)
                    {
                        xn.ParentNode.RemoveChild(xn);
                    }
                else
                    if (value.Equals(""))
                    //删除属性
                    foreach (XmlNode xn in xns)
                    {
                        ((XmlElement)xn).RemoveAttribute(attribute);
                    }
                else
                    //删除指定节点
                    foreach (XmlNode xn in xns)
                    {
                        if (((XmlElement)xn).GetAttribute(attribute) == value)
                            xn.ParentNode.RemoveChild(xn);
                    }
                outcome = XMLDocumentToString(ref doc);
                return outcome;
            }
            catch { return outcome; }
        }

        /// <summary>
        /// 将Xml文件转换成string
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public string XMLDocumentToString(ref XmlDocument doc)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            doc.Save(writer);

            StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8);
            stream.Position = 0;
            string xmlString = sr.ReadToEnd();
            sr.Close();
            stream.Close();

            return xmlString;
        }
        /// <summary>
        /// 将对象转换成xml格式的字符串
        /// </summary>
        /// <param name="data"></param>
        /* 示例
       传入  Xml_Data xml_data = new Xml_Data()
            {
                xml_node = "LINES",
                xml_children = new Xml_Data[] {
                   new Xml_Data(){
                     xml_node="LINE", 
                     xml_attr_val=new Xml_Attr_Value[]{ new Xml_Attr_Value(){xml_Attribute="no", xml_value="3"}},
                     xml_children=new Xml_Data[]{
                        new Xml_Data(){ xml_node="STAGE", xml_attr_val=new Xml_Attr_Value[]{ new Xml_Attr_Value(){ xml_Attribute="pwd",xml_value="bw131515"},new Xml_Attr_Value(){ xml_Attribute="user", xml_value="bw"}}},
                        new Xml_Data(){ xml_node="STAGE",xml_attr_val=new Xml_Attr_Value[]{ new Xml_Attr_Value(){ xml_Attribute="pwd",xml_value="bw131515"},new Xml_Attr_Value(){ xml_Attribute="user", xml_value="bw"}}},
                        new Xml_Data(){ xml_node="STAGE",xml_attr_val=new Xml_Attr_Value[]{ new Xml_Attr_Value(){ xml_Attribute="pwd",xml_value="bw131515"},new Xml_Attr_Value(){ xml_Attribute="user", xml_value="bw"}}},
                        new Xml_Data(){ xml_node="STAGE",xml_attr_val=new Xml_Attr_Value[]{ new Xml_Attr_Value(){ xml_Attribute="pwd",xml_value="bw131515"},new Xml_Attr_Value(){ xml_Attribute="user", xml_value="bw"}}}
                     }
                   }
                }
                //xml_attr_val = new Xml_Attr_Value[] { 
                //    new Xml_Attr_Value() { xml_Attribute = "", xml_value = "封笑笑" }, 
                //    new Xml_Attr_Value() { xml_Attribute = "OtherName", xml_value = "封大侠" } },
                //xml_children = new Xml_Data[] { new Xml_Data() {


                //} }
            };
         输出
         <?xml version="1.0" encoding="GB2312"?>
         <LINES>
             <LINE no="3">
                  <STAGE pwd="bw131515" user="bw" />
                  <STAGE pwd="bw131515" user="bw" />
                  <STAGE pwd="bw131515" user="bw" />
                  <STAGE pwd="bw131515" user="bw" />
             </LINE>
         </LINES>
         */
        public static void StringToXml(Xml_Data data)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode xn = doc.CreateXmlDeclaration("1.0", "GB2312", null);
            doc.AppendChild(xn);
            XmlElement root = doc.CreateElement(data.xml_node);
            if (data.xml_attr_val != null)
            {
                foreach (Xml_Attr_Value one in data.xml_attr_val)
                {
                    root.SetAttribute(one.xml_Attribute, one.xml_value);
                }
            }
            doc.AppendChild(root);
            foreach_xml_data_children(data, root, doc);
            string p = doc.InnerXml;
            doc.Save(@"D:\p.xml");
        }
        private static void foreach_xml_data_children(Xml_Data data, XmlElement root, XmlDocument doc)
        {
            if (data.xml_children != null)
            {
                foreach (Xml_Data child in data.xml_children)
                {
                    XmlElement xe = doc.CreateElement(child.xml_node);
                    if (child.xml_attr_val.Count() > 0)
                    {
                        foreach (Xml_Attr_Value one in child.xml_attr_val)
                        {
                            xe.SetAttribute(one.xml_Attribute, one.xml_value);
                        }
                    }
                    root.AppendChild(xe);
                    foreach_xml_data_children(child, xe, doc);
                }
            }
        }
    }


    public class Xml_Data
    {
        public string xml_node { get; set; }
        public Xml_Attr_Value[] xml_attr_val { get; set; }
        public Xml_Data[] xml_children
        {
            get;
            set;
        }
    }
    public class Xml_Attr_Value
    {
        public string xml_Attribute { get; set; }
        public string xml_value { get; set; }
    }
}