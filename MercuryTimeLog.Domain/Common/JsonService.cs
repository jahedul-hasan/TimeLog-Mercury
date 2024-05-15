using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text;
using System.Xml;

namespace MercuryTimeLog.Domain.Common;

public static class JsonService 
{
    private const string EmptyJson = "{}";
    public static string UpdateJson(JObject objJson, string selectedValue, dynamic updatedValue = null)
    {
        try
        {
            JToken jToken = objJson.SelectToken(selectedValue);
            jToken.Replace(updatedValue);
            return objJson.ToString();
        }
        catch (Exception exp)
        {
            throw exp;
        }
    }
    public static string GetJsonValue(JObject objJson, string selectedValue)
    {
        try
        {
            JToken jToken = objJson.SelectToken(selectedValue);
            return jToken.ToString();
        }
        catch (Exception exp)
        {
            throw exp;
        }
    }
    public static string removeJson(JObject objJson, string selectedToken, List<string> propList = null)
    {
        try
        {
            JObject header = (JObject)objJson.SelectToken(selectedToken);
            if (propList != null)
            {
                foreach (var item in propList)
                {
                    header.Property(item).Remove();
                }
            }
            else
            {
                header.Remove();
            }
            objJson.ToString();
        }
        catch (Exception exp)
        {
            return objJson.ToString();
        }
        return objJson.ToString();
    }
    public static void UpdateAppSetting(string key, object value)
    {
        if (key == null)
        {
            throw new ArgumentException("Json property key cannot be null", nameof(key));
        }

        const string settinsgFileName = "appsettings.json";
        if (!File.Exists(settinsgFileName))
        {
            File.WriteAllText(settinsgFileName, EmptyJson);
        }
        var config = File.ReadAllText(settinsgFileName);

        var updatedConfigDict = UpdateJson(key, value, config);
        var updatedJson = JsonSerializer.Serialize(updatedConfigDict, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(settinsgFileName, updatedJson);
    }
    private static Dictionary<string, object> UpdateJson(string key, object value, string jsonSegment)
    {
        const char keySeparator = ':';

        var config = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonSegment);
        var keyParts = key.Split(keySeparator);
        var isKeyNested = keyParts.Length > 1;
        if (isKeyNested)
        {
            var firstKeyPart = keyParts[0];
            var remainingKey = string.Join(keySeparator, keyParts.Skip(1));
            var newJsonSegment = config.ContainsKey(firstKeyPart) && config[firstKeyPart] != null
                ? config[firstKeyPart].ToString()
                : EmptyJson;
            config[firstKeyPart] = UpdateJson(remainingKey, value, newJsonSegment);
        }
        else
        {
            config[key] = value;
        }
        return config;
    }
    public static string XmlToJSON(string xml)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        return XmlToJSON(doc);
    }
    public static string XmlToJSON(XmlDocument xmlDoc)
    {
        StringBuilder sbJSON = new StringBuilder();
        sbJSON.Append("{ ");
        XmlToJSONnode(sbJSON, xmlDoc.DocumentElement, true);
        sbJSON.Append("}");
        return sbJSON.ToString();
    }
    private static void XmlToJSONnode(StringBuilder sbJSON, XmlElement node, bool showNodeName)
    {
        if (showNodeName)
            sbJSON.Append("\"" + SafeJSON(node.Name) + "\": ");
        sbJSON.Append("{");
        SortedList<string, object> childNodeNames = new SortedList<string, object>();
        if (node.Attributes != null)
            foreach (XmlAttribute attr in node.Attributes)
                StoreChildNode(childNodeNames, attr.Name, attr.InnerText);
        foreach (XmlNode cnode in node.ChildNodes)
        {
            if (cnode is XmlText)
                StoreChildNode(childNodeNames, "value", cnode.InnerText);
            else if (cnode is XmlElement)
                StoreChildNode(childNodeNames, cnode.Name, cnode);
        }
        foreach (string childname in childNodeNames.Keys)
        {
            List<object> alChild = (List<object>)childNodeNames[childname];
            if (alChild.Count == 1)
                OutputNode(childname, alChild[0], sbJSON, true);
            else
            {
                sbJSON.Append(" \"" + SafeJSON(childname) + "\": [ ");
                foreach (object Child in alChild)
                    OutputNode(childname, Child, sbJSON, false);
                sbJSON.Remove(sbJSON.Length - 2, 2);
                sbJSON.Append(" ], ");
            }
        }
        sbJSON.Remove(sbJSON.Length - 2, 2);
        sbJSON.Append(" }");
    }
    private static void StoreChildNode(SortedList<string, object> childNodeNames, string nodeName, object nodeValue)
    {
        if (nodeValue is XmlElement)
        {
            XmlNode cnode = (XmlNode)nodeValue;
            if (cnode.Attributes.Count == 0)
            {
                XmlNodeList children = cnode.ChildNodes;
                if (children.Count == 0)
                    nodeValue = null;
                else if (children.Count == 1 && children[0] is XmlText)
                    nodeValue = ((XmlText)children[0]).InnerText;
            }
        }
        List<object> ValuesAL;

        if (childNodeNames.ContainsKey(nodeName))
        {
            ValuesAL = (List<object>)childNodeNames[nodeName];
        }
        else
        {
            ValuesAL = new List<object>();
            childNodeNames[nodeName] = ValuesAL;
        }
        ValuesAL.Add(nodeValue);
    }
    private static void OutputNode(string childname, object alChild, StringBuilder sbJSON, bool showNodeName)
    {
        if (alChild == null)
        {
            if (showNodeName)
                sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
            sbJSON.Append("null");
        }
        else if (alChild is string)
        {
            if (showNodeName)
                sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
            string sChild = (string)alChild;
            sChild = sChild.Trim();
            sbJSON.Append("\"" + SafeJSON(sChild) + "\"");
        }
        else
            XmlToJSONnode(sbJSON, (XmlElement)alChild, showNodeName);
        sbJSON.Append(", ");
    }
    private static string SafeJSON(string sIn)
    {
        StringBuilder sbOut = new StringBuilder(sIn.Length);
        foreach (char ch in sIn)
        {
            if (char.IsControl(ch) || ch == '\'')
            {
                int ich = ch;
                sbOut.Append(@"\u" + ich.ToString("x4"));
                continue;
            }
            else if (ch == '\"' || ch == '\\' || ch == '/')
            {
                sbOut.Append('\\');
            }
            sbOut.Append(ch);
        }
        return sbOut.ToString();
    }
}
