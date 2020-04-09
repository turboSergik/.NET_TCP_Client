using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;

namespace SocketUser
{
    enum Command { LOGIN, TEXT, BIN, EXIT };

    enum Compression { ZIP, NONE };

    struct Packet
    {
        public Dictionary<string, string> Meta;
        public byte[] Response;
        public override string ToString()
        {
            string result = "";

            foreach (KeyValuePair<string, string> pair in this.Meta)
            {
                result += pair.Key;
                result += ": ";
                result += pair.Value;
                result += "\n";
            }
            result += "\n";
            result += Encoding.UTF8.GetString(this.Response);

            return result;
        }
    };

    /**
     * Format:
     * 
     * Command: login
     * Compression: gzip
     * 
     * edelwud
     * 
     */

    class Protocol
    {
        string[] AllowedHeaders = { "Command", "Compression" };

        public Packet ParsePacket(string buffer)
        {
            int split = buffer.IndexOf("\n\n") == -1 ? buffer.Length : buffer.IndexOf("\n\n") + 2;

            Regex metaParser = new Regex("([A-Za-z 0-9]+): +([A-Za-z 0-9]+)");
            Match match = metaParser.Match(buffer.Substring(0, split));

            Dictionary<string, string> meta = new Dictionary<string, string>();

            while (match.Success)
            {
                string header = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                meta.Add(header, value);
                match = match.NextMatch();
            }

            Packet packet = new Packet();
            packet.Meta = meta;
            packet.Response = Encoding.UTF8.GetBytes(buffer.Substring(split));
            return packet;
        }

        public Packet ConfigurePacket(Command command, Compression compression, byte[] message)
        {
            Dictionary<string, string> format = new Dictionary<string, string>();
            format.Add("Command", command.ToString());
            format.Add("Compression", compression.ToString());

            Packet packet = new Packet();
            packet.Meta = format;
            packet.Response = message;
            return packet;
        }


    }
}
