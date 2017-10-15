using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace EasyEmit.Creator
{
    public sealed class EnumCreator : Metadata.Metadata
    {
        private ModuleBuilder moduleBuilder;

        private Dictionary<string, int> entrys;

        public TypeAttributes TypeAttributes { get; private set; }

        public EnumCreator(string name, ModuleBuilder moduleBuilder) : this(name, moduleBuilder, TypeAttributes.Public) { }

        public EnumCreator(string name,ModuleBuilder moduleBuilder,TypeAttributes typeAttributes)
        {
            this.Name = name;
            this.Assembly = moduleBuilder.Assembly;
            this.State = Metadata.State.NotDefined;
            this.IsEnum = true;
            this.IsGenericParameter = false;
            this.moduleBuilder = moduleBuilder;
            this.entrys = new Dictionary<string, int>();
            this.TypeAttributes = typeAttributes;
        }
        
        public void AddEntry(string key,int value)
        {
            if(entrys.ContainsKey(key))
            {
                throw new Exception(string.Format("The enum already contains {0}", key));
            }
            if(entrys.ContainsValue(value))
            {
                throw new Exception(string.Format("The enum already contains the value {0}", value));
            }
            entrys.Add(key, value);
        }
        public Type Compile()
        {
            if (State == Metadata.State.NotDefined)
            {
                EnumBuilder enumBuilder = moduleBuilder.DefineEnum(Name, this.TypeAttributes, typeof(int));
                foreach (KeyValuePair<string, int> entry in entrys)
                {
                    enumBuilder.DefineLiteral(entry.Key, entry.Value);
                }
                Type type = enumBuilder.CreateTypeInfo().AsType();
                State = Metadata.State.Defined;
                this.type = type;
                return type;
            }
            else
            {
                throw new Exception("The enum has been already compile");
            }
        }
    }
}
