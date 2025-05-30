﻿using System;
using Flagship.Hit;

namespace Flagship.Model
{
    internal class HitCacheData
    {
        public string VisitorId { get; set; }

        public string AnonymousId { get; set; }

        public HitType Type { get; set; }

        public DateTime Time { get; set; }

        public object Content { get; set; }
    }

    internal class HitCacheDTOV1
    {
        public int Version { get; set; }

        public HitCacheData Data { get; set; }
    }
}
