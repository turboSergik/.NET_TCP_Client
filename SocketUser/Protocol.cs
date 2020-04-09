using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SocketUser
{
    enum ContentType { PLAIN, BIN };
    
    enum PackageCompression { ZIP, GZIP, DEFLATE };

    struct Packet
    {
        ContentType Type;
        PackageCompression Compression;
        uint CurrentPacket;
        uint AllPackeges;
        byte[] Body;
    };

    class Protocol
    {
        Packet ParsePackage(byte[] collection)
        {

            return Packet{ };
        }        
    }
}
