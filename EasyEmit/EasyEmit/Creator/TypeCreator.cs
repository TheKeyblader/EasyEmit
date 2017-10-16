using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace EasyEmit.Creator
{
    public sealed class TypeCreator : Metadata.Metadata
    {
        private ModuleBuilder moduleBuilder;

        public TypeAttributes TypeAttributes { get; private set; }

        public Metadata.Metadata Parent { get; private set; }

        private List<GenerericParameterCreator> genericParameter { get; set; }

        private List<FieldCreator> Fields = new List<FieldCreator>();

        private List<MethodCreator> Methods = new List<MethodCreator>();

        public TypeCreator(string name,ModuleBuilder moduleBuilder, TypeAttributes typeAttributes = TypeAttributes.Public)
        {
            this.Name = name;
            this.moduleBuilder = moduleBuilder;
            this.State = Metadata.State.NotDefined;
            this.IsGenericParameter = false;
            this.IsClass = true;
            this.TypeAttributes = typeAttributes;
            this.Assembly = moduleBuilder.Assembly;
        }

        public void SetParent(Metadata.Metadata parent)
        {
            if (State == Metadata.State.NotDefined)
            {
                if (Parent == null)
                {
                    Parent = parent;
                }
                else
                {
                    throw new Exception("The parent has already set");
                }
            }
            else
            {
                throw new Exception((State == Metadata.State.Creating) ? "The type has been partialy compile" : "The type has been compile");
            }
        }

        public List<GenerericParameterCreator> SetGenericParameter(params string[] names)
        {
            List<GenerericParameterCreator> parameters = names.Select(n => new GenerericParameterCreator(n, Assembly)).ToList();
            genericParameter = parameters;
            return parameters;
        }

        public FieldCreator AddFields(string name, Metadata.Metadata returnType,FieldAttributes fieldAttributes)
        {
            FieldCreator fieldCreator = new FieldCreator(name, returnType, fieldAttributes);
            Fields.Add(fieldCreator);
            return fieldCreator;
        }
        public MethodCreator AddMethod(string name,MethodAttributes methodAttributes,CallingConventions callingConventions)
        {
            MethodCreator methodCreator = new MethodCreator(name, methodAttributes, callingConventions);
            Methods.Add(methodCreator);
            return methodCreator;
        }
        public Type Compile()
        {
            if (State == Metadata.State.NotDefined)
            {
                if (Parent != null && (Parent.State== Metadata.State.NotDefined || !Parent.IsClass))
                {
                    throw new Exception("The parent is not defined or is not an class");
                }
                TypeBuilder typeBuilder = moduleBuilder.DefineType(Name, TypeAttributes);
                this.type = typeBuilder;
                this.State = Metadata.State.Creating;
                if (Parent != null)
                {
                    typeBuilder.SetParent(Parent);
                }
                foreach(FieldCreator field in Fields)
                {
                    field.Compile(typeBuilder);
                }
                foreach(MethodCreator method in Methods)
                {
                    method.Compile(typeBuilder);
                }
                Type type = typeBuilder.CreateType();
                State = Metadata.State.Defined;
                this.type = type;
                return type;
            }
            else
            {
                throw new Exception("The type has been already compile");
            }
        }
    }
}
