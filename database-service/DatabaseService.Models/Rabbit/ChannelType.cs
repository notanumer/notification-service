﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseService.Models.Rabbit
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChannelType
    {
        Telegram,
        VK,
        Email,
    }
}
