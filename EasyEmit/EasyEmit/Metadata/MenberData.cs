using System;
using System.Collections.Generic;
using System.Text;

namespace EasyEmit.Metadata
{
    public class MenberData
    {
        public Metadata DeclaringType { get; protected set; }

        public EasyEmit.Creator.MethodCreator DeclaringMethod { get; protected set; }

        public string Name { get; protected set; }

        public State State { get; protected set; }
    }
}
