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

        private TypeBuilder typeBuilder;

        public TypeAttributes TypeAttributes { get; private set; }

        public Metadata.Metadata Parent { get; private set; }

        private List<Metadata.Metadata> interfaces = new List<Metadata.Metadata>();

        private List<GenerericParameterCreator> GenericParameters = new List<GenerericParameterCreator>();

        private List<CustomAttributeBuilder> CustomAttributes = new List<CustomAttributeBuilder>();

        private List<ConstructorCreator> Constructors = new List<ConstructorCreator>();

        private List<FieldCreator> Fields = new List<FieldCreator>();

        private List<MethodCreator> Methods = new List<MethodCreator>();

        private List<PropertyCreator> Properties = new List<PropertyCreator>();

        private List<EventCreator> Events = new List<EventCreator>();

        internal TypeCreator(string name,ModuleBuilder moduleBuilder, TypeAttributes typeAttributes)
        {
            if(typeAttributes.HasFlag(TypeAttributes.Interface))
            {
                throw new Exception("This class is made to created Class use interfacesCreator for interface");
            }
            this.Name = name;
            this.moduleBuilder = moduleBuilder;
            this.State = Metadata.State.NotDefined;
            this.IsGenericParameter = false;
            this.TypeAttributes = typeAttributes;
            this.IsClass = true;
            this.Assembly = moduleBuilder.Assembly;
        }

        #region BaseDefinition
        public void SetParent(Metadata.Metadata parent)
        {
            if (State == Metadata.State.NotDefined)
            {
                if (Parent == null)
                {
                    if(!parent.IsClass)
                    {
                        throw new Exception(string.Format("The type {0} is not an class", parent.Name));
                    }
                    Parent = parent;
                }
                else
                {
                    throw new Exception("The parent has already set");
                }
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        public void AddInterface(Metadata.Metadata interfaces)
        {
            if(!interfaces.IsInterface)
            {
                throw new Exception(string.Format("The type {0} is not an interfaces", interfaces.Name));
            }
            this.interfaces.Add(interfaces);
        }
        public List<GenerericParameterCreator> SetGenericParameter(params string[] names)
        {
            List<GenerericParameterCreator> parameters = names.Select(n => new GenerericParameterCreator(n, Assembly)).ToList();
            GenericParameters = parameters;
            return parameters;
        }
        public void SetCustomAttribute(CustomAttributeBuilder customAttributeBuilder)
        {
            if(State == Metadata.State.NotDefined)
            {
                CustomAttributes.Add(customAttributeBuilder);
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        #endregion

        #region AllDefinition
        public ConstructorCreator AddConstructor(MethodAttributes methodAttributes,IEnumerable<Metadata.Metadata> parameters)
        {
            ConstructorCreator constructorCreator = new ConstructorCreator(methodAttributes, parameters);
            Constructors.Add(constructorCreator);
            return constructorCreator;
        }
        public FieldCreator AddField(string name, Metadata.Metadata returnType,FieldAttributes fieldAttributes)
        {
            if(!VerificationName(name))
            {
                throw new Exception("The name is already taken");
            }
            FieldCreator fieldCreator = new FieldCreator(name, returnType, fieldAttributes);
            Fields.Add(fieldCreator);
            return fieldCreator;
        }
        public MethodCreator AddMethod(string name,MethodAttributes methodAttributes,CallingConventions callingConventions)
        {
            if(!VerificationName(name))
            {
                throw new Exception("The name is already taken");
            }
            MethodCreator methodCreator = new MethodCreator(name, methodAttributes, callingConventions);
            Methods.Add(methodCreator);
            return methodCreator;
        }
        public PropertyCreator AddProperty(string name,Metadata.Metadata returnType,PropertyAttributes propertyAttributes)
        {
            if (!VerificationName(name))
            {
                throw new Exception("The name is already taken");
            }
            PropertyCreator property = new PropertyCreator(name, returnType, propertyAttributes);
            Properties.Add(property);
            return property;
        }
        #endregion

        #region Compilation
        public bool VerificationBaseDefinition(bool throwException)
        {
            if(Parent!=null && !Parent.IsClass)
            {
                if (throwException)
                {
                    throw new Exception(string.Format("The parent class {0} is not an class", Parent.Name));
                }
                else { return false; }
            }
            if(Parent!=null && Parent.State == Metadata.State.NotDefined)
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The parent class {0} is not defined", Parent.Name));
                }
                else { return false; }
            }
            if(interfaces.Any(i => !i.IsInterface))
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The type {0} is not an interface", interfaces.First(i => !i.IsInterface).Name));
                }
            }
            if(interfaces.Any(i => i.State == Metadata.State.NotDefined))
            {
                if (throwException)
                {
                    throw new Exception(string.Format("The interface {0} is not defined", interfaces.First(i => i.State == Metadata.State.NotDefined).Name));
                }
                else { return false; }
            }
            foreach(GenerericParameterCreator parameter in GenericParameters)
            {
                if(GenericParameters.Count(g => g.Name == parameter.Name)>1)
                {
                    throw new Exception("They are more 1 than instance of the genericParameter " + parameter.Name);
                }
                parameter.Verification(true);
            }
            return true;
        }
        public Type CompileBaseDefinition()
        {
            if(State == Metadata.State.NotDefined)
            {
                VerificationBaseDefinition(true);
                typeBuilder = moduleBuilder.DefineType(Name, TypeAttributes);
                if(Parent!=null)
                {
                    typeBuilder.SetParent(Parent);
                }
                foreach(Metadata.Metadata @interface in interfaces)
                {
                    typeBuilder.AddInterfaceImplementation(@interface);
                }
                if (GenericParameters.Count > 0)
                {
                    foreach (GenericTypeParameterBuilder genericBuilder in typeBuilder.DefineGenericParameters(GenericParameters.Select(g => g.Name).ToArray()))
                    {
                        GenerericParameterCreator generericParameterCreator = GenericParameters.Single(g => g.Name == genericBuilder.Name);
                        generericParameterCreator.Compile(genericBuilder);
                    }
                }
                foreach(CustomAttributeBuilder customAttribute in CustomAttributes)
                {
                    typeBuilder.SetCustomAttribute(customAttribute);
                }
                State = Metadata.State.BaseDefinition;
                type = typeBuilder;
                return type;
            }
            else
            {
                throw new Exception("You already start or finish compilation");
            }
        }
        
        public bool VerificationDefinition(bool throwException)
        {
            if(!VerificationBaseDefinition(throwException))
            {
                return false;
            }
            foreach(FieldCreator field in Fields)
            {
                if(!field.Verification(throwException))
                {
                    return false;
                }
            }
            foreach(MethodCreator method in Methods)
            {
                if(!method.VerificationBaseDefinition(throwException))
                {
                    return false;
                }
            }
            foreach(PropertyCreator property in Properties)
            {
                if(!property.VerificationBaseDefinition(throwException))
                {
                    return false;
                }
            }
            foreach (ConstructorCreator constructor in Constructors)
            {
                if (!constructor.VerificationBaseDefinition(throwException))
                {
                    return false;
                }
            }
            return true;
        }
        public Type CompileDefinition()
        {
            if(State != Metadata.State.Defined && State!= Metadata.State.AllDefiniton)
            {

                if(State == Metadata.State.NotDefined)
                {
                    CompileBaseDefinition();
                }
                VerificationDefinition(true);
                foreach (FieldCreator field in Fields)
                {
                    field.Compile(typeBuilder);
                }
                foreach(MethodCreator method in Methods)
                {
                    method.CompileBaseDefinitionDefinition(typeBuilder);
                }
                foreach(PropertyCreator property in Properties)
                {
                    property.CompileBaseDefinition(typeBuilder);
                }
                foreach(ConstructorCreator constructor in Constructors)
                {
                    constructor.CompileBaseDefinition(typeBuilder);
                }
                State = Metadata.State.AllDefiniton;
                type = typeBuilder;
                return typeBuilder;
            }
            else
            {
                throw new Exception("You already start or finish compilation");
            }
        }

        public Type Compile()
        {
            if(State != Metadata.State.Defined)
            {
               
                if(State != Metadata.State.AllDefiniton)
                {
                    CompileDefinition();
                }
                foreach(MethodCreator method in Methods)
                {
                    method.Compile(typeBuilder);
                }
                foreach(PropertyCreator property in Properties)
                {
                    property.Compile(typeBuilder);
                }
                foreach(ConstructorCreator constructor in Constructors)
                {
                    constructor.Compile(typeBuilder);
                }
                State = Metadata.State.Defined;
                type = typeBuilder.CreateType();
                return type;

            }
            else
            {
                throw new Exception("You already compile the type");
            }
        }


        #endregion

        private bool VerificationName(string name)
        {
            if(Fields.Any(f => f.Name == name))
            {
                return false;
            }
            if(Methods.Any(f => f.Name == name))
            {
                return false;
            }
            if(Properties.Any(f => f.Name == name))
            {
                return false;
            }
            if(Name == name)
            {
                return false;
            }
            return true;
        }
    }
}
