using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit.Creator
{
    public class EventCreator : Metadata.Metadata
    {
        private EventAttributes eventAttributes;

        private Metadata.Metadata eventType;

        internal EventCreator(string name,EventAttributes eventAttributes,Metadata.Metadata eventType)
        {
            Name = name;
            this.eventAttributes = eventAttributes;
            this.eventType = eventType;
        }

        internal void CompileBaseDefinition(TypeBuilder typeBuilder)
        {
            typeBuilder.DefineEvent(Name, eventAttributes, eventType);

        }
    }
}
