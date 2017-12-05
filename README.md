# XmlComplex
Merge xml document via console by .NET Framework
This application for merging app.config/dll.config (.NET Applications) or android language files.

## Useage

      XmlComplex.exe -o=exportfile.xml inputfile1.xml inputfile2.xml ...
      Options:
        -o=FILE [--output=FILE]
                Output file. Specify export file. It is able to set input file

        -e=ENCODNG [--encoding=ENCODING]
                Output encoding. default: utf8 (without BOM)
                UTF-8BOM/UTF-16BOM/UTF-16BEBOM/UTF-32BOM/UTF-32BEBOM is supported.

        -i=INDENTCHAR [--indent=INDENTCHAR]
                Output with indednt. default:    (double space)
        -i- [--no-indent]
                Output with no indent

        -n=LINECODE [--newline=LINECODE]
                Output line break charactor(s). default: CRLF
                LINECODE: CR|LF|CRLF

        -es [--encodings]
                Show supported encodings.

        -h --help
                Show this message


## LICENSE

WTFPL2

