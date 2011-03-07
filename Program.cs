using System;
using System.Collections.Generic;
using System.IO;

namespace XmlComplex
{
    class Program
    {
        [STAThread]
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

            var _ex = new XmlComplexer();
            _ex.Combine(_items, _baseFile, _exportFile);

            return 0;
        }
    }
}
