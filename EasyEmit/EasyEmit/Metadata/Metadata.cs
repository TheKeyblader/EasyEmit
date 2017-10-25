using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyEmit.Metadata
{
    public enum State
    {
        NotDefined,
        BaseDefinition,
        AllDefiniton,
        Defined
    }
    public class Metadata : MenberData
    {

        public Assembly Assembly { get; protected set; }

        public bool IsGenericParameter { get; protected set; }

        public bool IsInterface { get; protected set; }

        public bool IsEnum { get; protected set; }

        public bool IsClass { get; protected set; }

        protected Type type;

        public Type Type { get
            {
                if(State != State.NotDefined)
                {
                    return type;
                }
                else
                {
                    return null;
                }
            } }

        public static implicit operator Metadata(Type type)
        {
            return new Metadata()
            {
                Name = type.Name,
                Assembly = type.Assembly,
                State = State.Defined,
                IsGenericParameter = type.IsGenericParameter,
                IsClass = type.IsClass,
                IsEnum = type.IsEnum,
                IsInterface = type.IsInterface,
                type = type
            };
        }
        public static implicit operator Type(Metadata metadata)
        {
            if(metadata == null)
            {
                return null;
            }
            else
            {
                return metadata.Type;
            }
        }

        internal Metadata()
        { }
        internal Metadata(string name,Assembly assembly)
        {
            Name = name;
            Assembly = assembly;
            State = State.NotDefined;
        }
    }
}
