using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace gHowl.Udp
{
    internal static class AdapterDiscoverer
    {
        public static int InternetAvailable(out List<string[]> details)
        {
            details = new List<string[]>(2);
            if (!NetworkInterface.GetIsNetworkAvailable())
                return 0;

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            int count = 0;
            foreach (NetworkInterface item in interfaces)
            {
                if (item.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    item.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    item.OperationalStatus == OperationalStatus.Up &&
                    !item.IsReceiveOnly)
                {
                    string[] detail = new string[10];
                    details.Add(detail);

                    detail[0] = item.Name;
                    detail[1] = item.Description;
                    detail[2] = item.NetworkInterfaceType.ToString();
                    detail[3] = Formatter.Instance.FormatBitsBase10(item.Speed) + "ps";
                    detail[4] = item.Speed.ToString();
                    detail[5] = item.GetPhysicalAddress().ToString();

                    UnicastIPAddressInformationCollection ips = item.GetIPProperties().UnicastAddresses;

                    if (item.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        for (int i = 0; i < ips.Count; i++)
                        {
                            if (ips[i].DuplicateAddressDetectionState == DuplicateAddressDetectionState.Preferred &&
                                ips[i].Address.AddressFamily == AddressFamily.InterNetwork
                                )
                            {
                                detail[6] = ips[i].Address.ToString();
                                break;
                            }
                        }
                    }
                    if (item.Supports(NetworkInterfaceComponent.IPv6))
                    {
                        for (int i = 0; i < ips.Count; i++)
                        {
                            if (ips[i].DuplicateAddressDetectionState == DuplicateAddressDetectionState.Preferred &&
                                ips[i].Address.AddressFamily == AddressFamily.InterNetworkV6
                                )
                            {
                                detail[7] = ips[i].Address.ToString();
                                break;
                            }
                        }
                    }

                    IPv4InterfaceStatistics s = item.GetIPv4Statistics();
                    detail[8] = s.BytesReceived.ToString();
                    detail[9] = s.BytesSent.ToString();

                    count++;
                }
            }
            return count;
        }
    }
}
