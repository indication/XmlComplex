using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
namespace CombineAppConfig
{
    class Program
    {
        /// <summary>
        /// ConsoleApplication.exe ExportTargetFileName.xml BaseFileName.xml ExtendFilename.xml...
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            var _items = new List<string>();
            _items.AddRange(args);
            if (_items.Count < 2)
                return -1;
            bool findAll = true;
            var _exportFile = _items[0];
            _items.RemoveAt(0);
            foreach (var _item in _items)
            {
                if (!File.Exists(_item))
                {
                    Console.WriteLine(string.Format("{0} is not Found.", _item));
                    findAll = false;
                }
            }
            if (!findAll)
            {
                return -1;
            }

            var _baseFile = _items[0];
            _items.RemoveAt(0);

            XmlDocument _basedoc = new XmlDocument();
            _basedoc.Load(_baseFile);

            foreach (var _item in _items)
            {
                var _xml = new XmlDocument();
                _xml.Load(_item);
                proc(_basedoc.ChildNodes[1] as XmlElement, _xml.ChildNodes[1] as XmlElement);
            }
            
            //_basedoc.Save(_baseFile + "2.xml");
            //Console.Write(_basedoc.OuterXml);
            //Console.Read();
            return 0;
        }

        static void proc(XmlElement basedata, XmlElement importdata)
        {
            if (basedata == null || importdata == null)
            {
                return;
            }
            if (!isSameElement(basedata, importdata))
            {
                basedata.InnerXml += importdata.OuterXml;
                return;
            }
            foreach (var data in importdata.ChildNodes)
            {
                var element = data as XmlElement;
                if (element == null)
                    continue;
                bool notFound = true;
                XmlElement searchel;
                foreach (var search in basedata.ChildNodes)
                {
                    searchel = search as XmlElement;
                    if (searchel == null)
                        continue;
                    if (isSameElement(element,searchel))
                    { 
                        proc(searchel, element);
                        notFound = false;
                        break;
                    }
                }
                if (notFound)
                {
                    basedata.InnerXml += element.OuterXml;
                }
                    
            }
        }
        static bool isSameElement(XmlElement basedata, XmlElement importdata)
        {
            if (basedata.Name != importdata.Name)
                return false;
            foreach (XmlAttribute _attr in basedata.Attributes)
            {
                bool isSameAttr = false;
                foreach (XmlAttribute _check in importdata.Attributes)
                {
                    if (_attr.Name.Equals(_check.Name) &&
                        _attr.Value == _check.Value)
                    {
                        isSameAttr = true;
                        break;
                    }
                }
                if (!isSameAttr)
                {
                    return false;
                }
            }
            return true;
        }


    }
}
