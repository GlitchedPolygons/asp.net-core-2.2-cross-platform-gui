// Copyright (c) 2019, Raphael Beck. All rights reserved.
// Use of this source code is governed by the BSD 3-Clause license that can be found in the repository root directory's LICENSE file.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace CrossPlatformGUI.Utilities
{
    /// <summary>
    /// Utility for checking port availability and getting free port numbers.
    /// </summary>
    public static class PortUtility
    {
        /// <summary>
        /// Checks whether a specific port is available or not.
        /// </summary>
        /// <param name="port">The port number to check [1;65535]</param>
        /// <returns>Whether the specified port is available or not.</returns>
        public static bool IsPortAvailable(int port)
        {
            if (port < 1 || port >= 65535)
            {
                return false;
            }

            var occupiedPorts = GetOccupiedPorts(1);

            return !occupiedPorts.Item1.Contains(port) 
                && !occupiedPorts.Item2.Contains(port)
                && !occupiedPorts.Item3.Contains(port);
        }

        /// <summary>
        /// Iterates through all TCP/UDP connections/listeners
        /// from a defined starting port number and returns the first available port.
        /// </summary>
        /// <param name="startingPort">The port number from which to iterate upwards. Ports prior to this number are ignored, even if available.</param>
        /// <returns>An available port number.</returns>
        public static int GetFirstAvailablePort(int startingPort)
        {
            var occupiedPorts = GetOccupiedPorts(startingPort);

            try
            {
                // Find the first port number that is not occupied and return it.
                int port = Enumerable
                    .Range(startingPort, ushort.MaxValue)
                    .Where(i => !occupiedPorts.Item1.Contains(i))
                    .Where(i => !occupiedPorts.Item2.Contains(i))
                    .First(i => !occupiedPorts.Item3.Contains(i));

                return port;
            }
            catch (InvalidOperationException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                throw new ApplicationException("There are no free ports available on localhost; couldn't start application...");
            }
        }

        /// <summary>
        /// Gets all ports that are currently active/occupied from a specific starting port number.
        /// </summary>
        /// <param name="startingPort">The port number from which to iterate upwards. Ports prior to this number are ignored, even if occupied.</param>
        /// <returns>A <see cref="Tuple"/> containing all occupied TCP connection and TCP/UDP connection/listener port numbers.</returns>
        public static Tuple<IEnumerable<int>, IEnumerable<int>, IEnumerable<int>> GetOccupiedPorts(int startingPort)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();

            // Get all currently active TCP connections.
            IEnumerable<int> tcpConnectionPorts = properties.GetActiveTcpConnections()
                .Where(n => n.LocalEndPoint.Port >= startingPort)
                .Select(n => n.LocalEndPoint.Port);

            // Get active TCP listeners.
            IEnumerable<int> tcpListenerPorts = properties.GetActiveTcpListeners()
                .Where(n => n.Port >= startingPort)
                .Select(n => n.Port);

            // Get active UDP listeners.
            IEnumerable<int> udpListenerPorts = properties.GetActiveUdpListeners()
                .Where(n => n.Port >= startingPort)
                .Select(n => n.Port);

            return new Tuple<IEnumerable<int>, IEnumerable<int>, IEnumerable<int>>(tcpConnectionPorts, tcpListenerPorts, udpListenerPorts);
        }
    }
}
