using System.Collections.Generic;
using AltV.Net.Data;
using EntityStreamer;
using Newtonsoft.Json;

namespace Server.Property
{
    public class PropertyObject
    {
        public string Name { get; set; }
        public string ObjectName { get; set; }
        public int Dimension { get; set; }
        public Position Position { get; set; }
        public Rotation Rotation { get; set; }
    }

    public class LoadPropertyObject
    {
        public int PropertyId { get; set; }
        public Prop DynamicObject { get; set; }

        public LoadPropertyObject()
        {
        }

        public LoadPropertyObject(int propertyId, Prop dynamicObject)
        {
            PropertyId = propertyId;
            DynamicObject = dynamicObject;
        }
    }
}