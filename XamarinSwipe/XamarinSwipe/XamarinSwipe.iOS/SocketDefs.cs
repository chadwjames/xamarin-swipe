using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace XamarinSwipe.iOS
{
    // From `mcs/class/System/System.Net.NetworkInformation/MacOsNetworkInterfaceMarshal.cs`
    // The original C struct is in `/usr/include/ifaddrs.h`
    struct ifaddrs
    {
        public IntPtr ifa_next;
        public string ifa_name;
        public uint ifa_flags;
        public IntPtr ifa_addr;
        public IntPtr ifa_netmask;
        public IntPtr ifa_dstaddr;
        public IntPtr ifa_data;
    }

    // The original C struct is in `/usr/include/netinet/in.h`
    struct sockaddr_in
    {
        public byte sin_len;
        public byte sin_family;
        public ushort sin_port;
        public uint sin_addr;
    }

    // The original C struct is in `/usr/include/sys/socket.h`
    struct sockaddr
    {
        public byte sa_len;
        public byte sa_family;
    }

    // The original C struct is in `/usr/include/net/if_dl.h`
    struct sockaddr_dl
    {
        public byte sdl_len;
        public byte sdl_family;
        public ushort sdl_index;
        public byte sdl_type;
        public byte sdl_nlen;
        public byte sdl_alen;
        public byte sdl_slen;
        public byte[] sdl_data;

        internal void Read(IntPtr ptr)
        {
            sdl_len = Marshal.ReadByte(ptr, 0);
            sdl_family = Marshal.ReadByte(ptr, 1);
            sdl_index = (ushort)Marshal.ReadInt16(ptr, 2);
            sdl_type = Marshal.ReadByte(ptr, 4);
            sdl_nlen = Marshal.ReadByte(ptr, 5);
            sdl_alen = Marshal.ReadByte(ptr, 6);
            sdl_slen = Marshal.ReadByte(ptr, 7);
            sdl_data = new byte[Math.Max(12, sdl_len - 8)];
            Marshal.Copy(new IntPtr(ptr.ToInt64() + 8), sdl_data, 0, sdl_data.Length);
        }
    }

    // The corresponding C defines are in `/usr/include/net/if.h`
    [Flags]
    enum IFF
    {
        /* interface is up */
        UP = 0x1,
        /* broadcast address valid */
        BROADCAST = 0x2,
        /* turn on debugging */
        DEBUG = 0x4,
        /* is a loopback net */
        LOOPBACK = 0x8,
        /* interface is point-to-point link */
        POINTOPOINT = 0x10,
        /* obsolete: avoid use of trailers */
        NOTRAILERS = 0x20,
        /* resources allocated */
        RUNNING = 0x40,
        /* no address resolution protocol */
        NOARP = 0x80,
        /* receive all packets */
        PROMISC = 0x100,
        /* receive all multicast packets */
        ALLMULTI = 0x200,
        /* transmission in progress */
        OACTIVE = 0x400,
        /* can't hear own transmissions */
        SIMPLEX = 0x800,
        /* per link layer defined bit */
        LINK0 = 0x1000,
        /* per link layer defined bit */
        LINK1 = 0x2000,
        /* per link layer defined bit */
        LINK2 = 0x4000,
        /* supports multicast */
        MULTICAST = 0x8000
    }

    public partial class SocketDefs
    {
        public SocketDefs()
        {
        }

        [DllImport("libc")]
        static extern int getifaddrs(out IntPtr ifap);

        [DllImport("libc")]
        static extern void freeifaddrs(IntPtr ifap);

        class NetInterfaceInfo
        {
            public IPAddress Netmask;
            public IPAddress Address;
            public byte[] MacAddress;
            public ushort Index;
            public byte Type;
        }

        const int AF_INET = 2;
        const int AF_INET6 = 30;
        const int AF_LINK = 18;
        // The code in this event handler is essentially a trimmed down version
        // of `MacOsNetworkInterface.ImplGetAllNetworkInterfaces()`, from:
        // `mcs/class/System/System.Net.NetworkInformation/NetworkInterface.cs`
        public string GetMacInfo(out string wifi, out string cell)
        {
            wifi = null;
            cell = null;

            var netInterfaceInfos = new Dictionary<string, NetInterfaceInfo>();

            IntPtr ifap;
            if (getifaddrs(out ifap) != 0)
                throw new SystemException("getifaddrs() failed");

            try
            {
                IntPtr next = ifap;
                while (next != IntPtr.Zero)
                {
                    ifaddrs addr = (ifaddrs)Marshal.PtrToStructure(next, typeof(ifaddrs));

                    if (addr.ifa_addr != IntPtr.Zero && (((IFF)addr.ifa_flags & IFF.UP) != 0) && ((((IFF)addr.ifa_flags) & IFF.LOOPBACK) == 0))
                    {
                        byte sa_family = ((sockaddr)Marshal.PtrToStructure(addr.ifa_addr, typeof(sockaddr))).sa_family;

                        NetInterfaceInfo info;

                        if (!(netInterfaceInfos.TryGetValue(addr.ifa_name, out info)))
                        {
                            info = new NetInterfaceInfo();
                            netInterfaceInfos.Add(addr.ifa_name, info);
                        }

                        if (sa_family == AF_INET)
                        {
                            info.Netmask = new IPAddress(((sockaddr_in)Marshal.PtrToStructure(addr.ifa_netmask, typeof(sockaddr_in))).sin_addr);
                            info.Address = new IPAddress(((sockaddr_in)Marshal.PtrToStructure(addr.ifa_addr, typeof(sockaddr_in))).sin_addr);

                            //wifi = this.GetIpAddressFromBytes(info.Address.GetAddressBytes());


                        }
                        else if (sa_family == AF_LINK)
                        {
                            sockaddr_dl sockaddrdl = new sockaddr_dl();
                            sockaddrdl.Read(addr.ifa_addr);

                            info.MacAddress = new byte[(int)sockaddrdl.sdl_alen];
                            Array.Copy(sockaddrdl.sdl_data, sockaddrdl.sdl_nlen, info.MacAddress, 0, Math.Min(info.MacAddress.Length, sockaddrdl.sdl_data.Length - sockaddrdl.sdl_nlen)); //Math.Min(info.MacAddress.Length, sockaddrdl.sdl_data.Length - sockaddrdl.sdl_nlen));
                            info.Index = sockaddrdl.sdl_index;
                            info.Type = sockaddrdl.sdl_type;


                        }
                    }
                    next = addr.ifa_next;
                }
            }
            finally
            {
                freeifaddrs(ifap);
            }

            var sb = new StringBuilder();
            foreach (var netInterfaceInfo in netInterfaceInfos)
            {
                if (netInterfaceInfo.Key == "pdp_ip0" && netInterfaceInfo.Value.Address != null)
                {
                    //cell = GetIpAddressFromBytes(info.MacAddress);
                    cell = GetIpAddressFromBytes(netInterfaceInfo.Value.Address.GetAddressBytes());
                }

                if (netInterfaceInfo.Key == "en0" && netInterfaceInfo.Value.Address != null)
                {
                    wifi = GetIpAddressFromBytes(netInterfaceInfo.Value.Address.GetAddressBytes());
                }

                sb.Append(String.Format("Interface: {0}, netmask: {1}, address: {2}, mac: {3}, index: {4}, type: {5}\n",
                    netInterfaceInfo.Key,
                    netInterfaceInfo.Value.Netmask,
                    netInterfaceInfo.Value.Address,
                    BitConverter.ToString(netInterfaceInfo.Value.MacAddress),
                    netInterfaceInfo.Value.Index,
                    netInterfaceInfo.Value.Type
                ));
            }

            return sb.ToString();
        }

        private string GetIpAddressFromBytes(byte[] ipBytes)
        {
            uint a1 = Convert.ToUInt32(ipBytes[0] * Math.Pow(256, 0));
            uint a2 = Convert.ToUInt32(ipBytes[1] * Math.Pow(256, 0));
            uint a3 = Convert.ToUInt32(ipBytes[2] * Math.Pow(256, 0));
            uint a4 = Convert.ToUInt32(ipBytes[3] * Math.Pow(256, 0));
            string ip = "";

            try
            {
                ip = String.Format("{0}.{1}.{2}.{3}", new object[] { a1, a2, a3, a4 });
            }
            catch
            {
            }

            return ip;
        }
    }
}


