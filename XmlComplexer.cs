using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
namespace XmlComplex
{
    class XmlComplexer
    {

        public XmlDocument Combine(List<string> _items, string _baseFile)
        {
            var  _basedoc = new XmlDocument();
            _basedoc.Load(_baseFile);

            foreach (var _item in _items)
            {
                var _xml = new XmlDocument();
                _xml.Load(_item);
                proc(_basedoc.ChildNodes[1] as XmlElement, _xml.ChildNodes[1] as XmlElement);
            }
            return _basedoc;
        }

        public void Combine(List<string> _items, string _baseFile, string _exportFile)
        {
            var _basedoc = Combine(_items, _baseFile);
            _basedoc.Save(_exportFile);
        }

        void proc(XmlElement basedata, XmlElement importdata)
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
