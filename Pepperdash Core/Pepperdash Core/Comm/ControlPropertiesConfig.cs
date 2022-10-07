﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace PepperDash.Core
{
    /// <summary>
    /// Config properties that indicate how to communicate with a device for control
    /// </summary>
    public class ControlPropertiesConfig
    {
        /// <summary>
        /// The method of control
        /// </summary>
        public eControlMethod Method { get; set; }

        /// <summary>
        /// The key of the device that contains the control port
        /// </summary>
        public string ControlPortDevKey { get; set; }

        /// <summary>
        /// The number of the control port on the device specified by ControlPortDevKey
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // In case "null" is present in config on this value
        public uint ControlPortNumber { get; set; }

        /// <summary>
        /// The name of the control port on the device specified by ControlPortDevKey
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // In case "null" is present in config on this value
        public string ControlPortName { get; set; }

        /// <summary>
        /// Properties for ethernet based communications
        /// </summary>
        public TcpSshPropertiesConfig TcpSshProperties { get; set; }

        /// <summary>
        /// The filename and path for the IR file
        /// </summary>
        public string IrFile { get; set; }

        /// <summary>
        /// The IpId of a Crestron device
        /// </summary>
        public string IpId { get; set; }

        /// <summary>
        /// Readonly uint representation of the IpId
        /// </summary>
        [JsonIgnore]
        public uint IpIdInt { get { return Convert.ToUInt32(IpId, 16); } }

        /// <summary>
        /// Char indicating end of line
        /// </summary>
        public char EndOfLineChar { get; set; }

        /// <summary>
        /// Defaults to Environment.NewLine;
        /// </summary>
        public string EndOfLineString { get; set; }

        /// <summary>
        /// Indicates 
        /// </summary>
        public string DeviceReadyResponsePattern { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControlPropertiesConfig()
        {
            EndOfLineString = CrestronEnvironment.NewLine;
        }
    }
}