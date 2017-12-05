using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
namespace XmlComplex
{
    /// <summary>
    /// Combine XML document helper
    /// </summary>
    class XmlComplexer
    {
        /// <summary>
        /// Combine XML document
        /// </summary>
        /// <param name="_items">Merge files</param>
        /// <param name="_baseFile">Base file</param>
        /// <returns>Merged XML document</returns>
        public XmlDocument Combine(string _baseFile, params string[] _items)
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
        /// <summary>
        /// Combine xml document
        /// </summary>
        /// <param name="basedata">Base XML document</param>
        /// <param name="importdata">Merge XML document</param>
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
            //Enumurate elements
            foreach (var element in importdata.ChildNodes.Cast<object>().Where(w=>w is XmlElement).Cast<XmlElement>().Where(w=>w!= null))
            {
                var sameelement = basedata.ChildNodes
                        .Cast<object>().Where(w => w is XmlElement).Cast<XmlElement>()
                        .Where(w => w != null)
                        .Where(searchel => isSameElement(element, searchel))
                        ;
                //Recursive call for merge elements
                foreach(var searchel in sameelement)
                    proc(searchel, element);
                
                if (!sameelement.Any())
                    basedata.InnerXml += element.OuterXml;

            }
        }
        /// <summary>
        /// Check XML element is same (all element attributes are equal)
        /// </summary>
        /// <param name="basedata">Base element</param>
        /// <param name="importdata">Target element</param>
        /// <returns>is same</returns>
        static bool isSameElement(XmlElement basedata, XmlElement importdata)
        {
            if (basedata.Name != importdata.Name)
                return false;
            return basedata.Attributes
                .Cast<XmlAttribute>()
                .All(_attr => 
                    importdata.Attributes
                        .Cast<XmlAttribute>()
                        .All(_check => 
                            _attr.Name.Equals(_check.Name) &&
                            _attr.Value == _check.Value
                            )
                         );
        }


    }
}
