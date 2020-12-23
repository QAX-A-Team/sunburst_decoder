using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Zcg.Tools.Misc
{
    class SunBrustDecoder
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) { Console.WriteLine("usage: sunbrust_decoder <dga_list.lst>"); return; }
            string k = "ph2eifo3n5utg1j8d94qrvbmk0sal76c";
            Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
            Dictionary<string, List<DateTime>> dic2 = new Dictionary<string, List<DateTime>>();
            Dictionary<string, string> dic3 = new Dictionary<string, string>();
            Dictionary<string, List<string>> dic4 = new Dictionary<string, List<string>>();
            foreach (string s in File.ReadAllLines(args[0]))
            {
                if (s.Length > 16)
                {
                    string s1 = s.Substring(0, 15);
                    byte[] data = base32_decode(k, s);
                    byte[] guid = null;
                    if ((data[0] & 0x80) == 0x80)
                    //step 1: first bit set
                    //OrionImprovementBusinessLayer+CryptoHelper.CreateSecureString with flag=true
                    //OrionImprovementBusinessLayer+CryptoHelper.GetPreviousString
                    //domain name fragment sender
                    {
                        char c = s[15];
                        //OrionImprovementBusinessLayer+CryptoHelper.CreateString decoder
                        int ncount = 0;//OrionImprovementBusinessLayer+CryptoHelper.nCount
                        if (c >= '0' && c <= '9')
                        {
                            ncount = c - '0';
                        }
                        else if (c >= 'a' && c <= 'z')
                        {
                            ncount = c - 'a' + 10;
                        }
                        else
                        {
                            Console.WriteLine("err parse ncount: " + s);
                        }
                        ncount = ncount - s[0];
                        while (ncount < 0) { ncount += 36; }
                        //OrionImprovementBusinessLayer.userId, created by 
                        //OrionImprovementBusinessLayer.GetOrCreateUserID
                        guid = guid_decode(k, s1);
                        string dk = BitConverter.ToString(guid).Replace("-", "");

                        //save all fragments
                        if (!dic.ContainsKey(dk))
                        {
                            dic[dk] = new string[36];
                        }
                        string[] d = dic[dk];
                        d[ncount] = s.Substring(16, s.Length - 16);
                    }
                    else
                    //step 2
                    //OrionImprovementBusinessLayer+CryptoHelper.CreateSecureString with flag=false
                    //OrionImprovementBusinessLayer+CryptoHelper.GetNextString 
                    //or OrionImprovementBusinessLayer+CryptoHelper.GetNextStringEx
                    {
                        DateTime dt;
                        if (parse_dga_time(s, out guid, out dt))
                        {
                            string dk = BitConverter.ToString(guid).Replace("-", "");
                            if (!dic2.ContainsKey(dk))
                            {
                                dic2[dk] = new List<DateTime>();
                            }
                            List<DateTime> ls = dic2[dk];
                            ls.Add(dt);
                        }
                        else
                        {
                            Console.WriteLine("err parse time: " + s);
                        }
                    }
                    if (guid != null)
                    {
                        string dk = BitConverter.ToString(guid).Replace("-", "");
                        if (!dic4.ContainsKey(dk))
                        {
                            dic4[dk] = new List<string>();
                        }
                        dic4[dk].Add(s);
                    }
                }
            }
            foreach (var kp in dic)
            {
                bool comp = false;
                string s = "";
                string r = null;
                string[] v = kp.Value;
                for (int i = 0; i < v.Length; i++)
                {
                    if (v[i] != null)
                    {
                        s += v[i];
                    }
                    else
                    {
                        break;
                    }
                }
                if (v[35] != null)
                {
                    s += v[35];
                    comp = true;
                }
                if (comp)
                {
                    if (s.StartsWith("00"))
                    {
                        r = Encoding.UTF8.GetString(base32_decode(k, s.Substring(2)));
                    }
                    else
                    {
                        r = change_char_map(s);
                    }
                }
                else
                {
                    r = "not completed request: " + s + ", ";
                    if (s.StartsWith("00"))
                    {
                        r += Encoding.UTF8.GetString(base32_decode(k, s.Substring(2)));
                    }
                    else
                    {
                        try { r += change_char_map(s); } catch { r += "char map err"; }
                    }
                }
                if (dic3.ContainsKey(kp.Key))
                {
                    dic3[kp.Key] = "," + dic3[kp.Key] + r;
                }
                else
                {
                    dic3[kp.Key] = r;
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (var kp in dic3)
            {
                sb.AppendFormat("{0},\"{1}\"\r\n", kp.Key, kp.Value);
            }
            File.WriteAllText("all_domains_dga.txt", sb.ToString());
            sb.Clear();
            List<DateTime> dts = new List<DateTime>();
            foreach (var kp in dic2)
            {
                sb.AppendFormat("{0},\"", kp.Key);
                foreach (DateTime dt in kp.Value)
                {
                    sb.Append(dt);
                    dts.Add(dt);
                    sb.Append(",");
                }
                sb.Append("\"\r\n");
            }
            File.WriteAllText("all_times_dga.txt", sb.ToString());
            sb.Clear();
            foreach (var kp in dic2)
            {
                if (dic.ContainsKey(kp.Key))
                {
                    sb.AppendFormat("{0},\"", kp.Key);
                    foreach (DateTime dt in kp.Value)
                    {
                        sb.Append(dt);
                        dts.Add(dt);
                        sb.Append(",");
                    }
                    sb.Append("\"\r\n");
                }
            }
            File.WriteAllText("all_times_contains_dga.txt", sb.ToString());
            dts.Sort();
            sb.Clear();
            foreach (DateTime dt in dts)
            {
                sb.Append(dt + "\r\n");
            }
            File.WriteAllText("all_times.txt", sb.ToString());
            sb.Clear();
            foreach (var kp in dic4)
            {
                sb.AppendFormat("{0},\"", kp.Key);
                foreach (string ss in kp.Value)
                {
                    sb.Append(ss);
                    sb.Append(",");
                }
                sb.Append("\"");
                sb.Append("\r\n");
            }
            File.WriteAllText("all_domain_mapping.txt", sb.ToString());
        }
        static string change_char_map(string s)
        {
            string k = "rq3gsalt6u1iyfzop572d49bnx8cvmkewhj";
            string k2 = "0_-.";
            string ret = "";
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '0')
                {
                    i++;
                    c = s[i];
                    if (k.IndexOf(c) != -1)
                    {
                        ret += k2[k.IndexOf(c) % 4];
                    }
                    else
                    {
                        return "err";
                    }
                }
                else if (k.IndexOf(c) != -1)
                {
                    ret += k[(k.IndexOf(c) - 4 + k.Length) % k.Length];
                }
                else
                {
                    return "err";
                }
            }
            return ret;
        }
        public static bool parse_dga_time(string s, out byte[] guid, out DateTime dt)
        {
            try
            {
                if (s.Length > 18 && s[16] == '0' && s[17] == '0') { dt = DateTime.Now; guid = null; return false; }
                byte[] data = base32_decode("ph2eifo3n5utg1j8d94qrvbmk0sal76c", s);
                byte[] data2 = new byte[data.Length - 1];
                for (int i = 1; i < data2.Length; i++)
                {
                    data2[i - 1] = (byte)(data[i] ^ data[0]);
                }
                data = data2;

                for (int i = 0; i < 8; i++)
                {
                    data[i] ^= data[8 + 2 - i % 2];
                }
                int i1 = data[8] & 0xf;
                int i2 = data[9];
                int i3 = data[10];
                int dti = (i1 << 16) | (i2 << 8) | i3;
                dti >>= 1;
                dt = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(dti * 30);
                if (dt > new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc) && dt < DateTime.UtcNow)
                {
                    guid = new byte[8];
                    for (int i = 0; i < 8; i++) { guid[i] = data[i]; }
                    return true;
                }
                guid = null;
                dt = DateTime.Now;
                return false;
            }
            catch { dt = DateTime.Now; guid = null; return false; }
        }

        public static byte[] guid_decode(string k, string s)
        {
            byte[] data = base32_decode(k, s);
            byte[] ret = new byte[8];
            for (int i = 1; i < 9; i++)
            {
                ret[i - 1] = (byte)(data[i] ^ data[0]);
            }
            return ret;
        }

        //copy-paste from https://stackoverflow.com/questions/641361/base32-decoding
        public static byte[] base32_decode(string ValidChars, string str)
        {
            int numBytes = str.Length * 5 / 8;
            byte[] bytes = new Byte[numBytes];

            int bit_buffer;
            int currentCharIndex;
            int bits_in_buffer;

            if (str.Length < 3)
            {
                bytes[0] = (byte)(ValidChars.IndexOf(str[0]) | ValidChars.IndexOf(str[1]) << 5);
                return bytes;
            }

            bit_buffer = (ValidChars.IndexOf(str[0]) | ValidChars.IndexOf(str[1]) << 5);
            bits_in_buffer = 10;
            currentCharIndex = 2;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)bit_buffer;
                bit_buffer >>= 8;
                bits_in_buffer -= 8;
                while (bits_in_buffer < 8 && currentCharIndex < str.Length)
                {
                    bit_buffer |= ValidChars.IndexOf(str[currentCharIndex++]) << bits_in_buffer;
                    bits_in_buffer += 5;
                }
            }
            return bytes;
        }

    }
}