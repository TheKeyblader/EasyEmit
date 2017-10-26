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
        #region BuildProperties
        private ModuleBuilder moduleBuilder;

        private TypeBuilder typeBuilder;
        #endregion

        #region BuildData
        public TypeAttributes TypeAttributes { get; private set; }

        public Metadata.Metadata Parent { get; private set; }

        private List<Metadata.Metadata> interfaces = new List<Metadata.Metadata>();

        private List<GenericParameterCreator> genericParameters = new List<GenericParameterCreator>();

        private List<CustomAttributeBuilder> customAttributes = new List<CustomAttributeBuilder>();

        private List<ConstructorCreator> constructors = new List<ConstructorCreator>();

        private List<FieldCreator> fields = new List<FieldCreator>();

        private List<MethodCreator> methods = new List<MethodCreator>();

        private List<PropertyCreator> properties = new List<PropertyCreator>();
        #endregion

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

        /// <summary>
        /// Set the parent of the new type
        /// </summary>
        /// <param name="parent">Parent type</param>
        /// <exception cref="System.Exception"> Throw when parent is not an class</exception>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void SetParent(Metadata.Metadata parent)
        {
            if (State == Metadata.State.NotDefined)
            {
                if(!parent.IsClass)
                {
                    throw new Exception(string.Format("The type {0} is not an class", parent.Name));
                }
                Parent = parent;
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Remove parent of type
        /// </summary>
        /// <exception cref="System.Exception">Throw when parent is already null</exception>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void RemoveParent()
        {
            if(State== Metadata.State.NotDefined)
            {
                if(Parent == null)
                {
                    throw new Exception("The type doesn't contains parent");
                }
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }

        /// <summary>
        /// Add Interfaces to type
        /// </summary>
        /// <param name="interfaces">interfaces to add</param>
        /// <exception cref="System.Exception">Throw when interfaces is not an interfaces</exception>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void AddInterface(Metadata.Metadata interfaces)
        {
            if (State == Metadata.State.NotDefined)
            {
                if (!interfaces.IsInterface)
                {
                    throw new Exception(string.Format("The type {0} is not an interfaces", interfaces.Name));
                }
                this.interfaces.Add(interfaces);
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Remove Interfaces to type
        /// </summary>
        /// <param name="interfaces">interfaces to add</param>
        /// <exception cref="System.Exception">Throw when interfaces is not an interfaces</exception>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void RemoveInterface(Metadata.Metadata interfaces)
        {
            if (State == Metadata.State.NotDefined)
            {
                if (!interfaces.IsInterface)
                {
                    throw new Exception(string.Format("The type {0} is not an interfaces", interfaces.Name));
                }
                this.interfaces.RemoveAll(i => i.Name == interfaces.Name);
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Enumerate all interfaces
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<Metadata.Metadata> GetInterfaces() => interfaces.GetEnumerator();

        /// <summary>
        ///     SetGenericType
        /// </summary>
        /// <param name="names">Names used for generic type</param>
        /// <returns>AllGenericType</returns>
        /// <exception cref="System.Exception">Throw when they names duplicate in the list</exception>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public List<GenericParameterCreator> SetGenericParameter(params string[] names)
        {
            if (State == Metadata.State.NotDefined)
            {
                foreach (string name in names)
                {
                    if (names.Count(n => n == name) > 1)
                    {
                        throw new Exception("Duplice names : " + name);
                    }
                }
                List<GenericParameterCreator> parameters = names.Select(n => new GenericParameterCreator(this,n)).ToList();
                genericParameters = parameters;
                return parameters;
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Not Implemented
        /// </summary>
        public void RemoveAllGenericParameter()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add CustomAttribute
        /// </summary>
        /// <param name="customAttributeBuilder"></param>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void SetCustomAttribute(CustomAttributeBuilder customAttributeBuilder)
        {
            if(State == Metadata.State.NotDefined)
            {
                customAttributes.Add(customAttributeBuilder);
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Remove all CustomAttribute
        /// </summary>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void RemoveAllCustomAttribute()
        {
            if(State == Metadata.State.NotDefined)
            {
                customAttributes.Clear();
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }

        #endregion

        #region AllDefinition

        /// <summary>
        /// Add Constructor to type
        /// </summary>
        /// <param name="methodAttributes"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public ConstructorCreator AddConstructor(MethodAttributes methodAttributes,IEnumerable<Metadata.Metadata> parameters)
        {
            if (State == Metadata.State.NotDefined || State == Metadata.State.BaseDefinition)
            {
                ConstructorCreator constructorCreator = new ConstructorCreator(methodAttributes, parameters);
                constructors.Add(constructorCreator);
                return constructorCreator;
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="parameters"></param>
        public void RemoveConstructor(IEnumerable<Metadata.Metadata> parameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add Field to type
        /// </summary>
        /// <param name="name">name of the field</param>
        /// <param name="returnType">return type of the field</param>
        /// <param name="fieldAttributes"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public FieldCreator AddField(string name, Metadata.Metadata returnType,FieldAttributes fieldAttributes)
        {
            if (State == Metadata.State.NotDefined || State == Metadata.State.BaseDefinition)
            {
                if (!VerificationName(name))
                {
                    throw new Exception("The name is already taken");
                }
                FieldCreator fieldCreator = new FieldCreator(this,name, returnType, fieldAttributes);
                fields.Add(fieldCreator);
                return fieldCreator;
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Remove an field of the type
        /// </summary>
        /// <param name="name">name of the field to remove</param>
        /// <exception cref="System.Exception">Throw when the field doesn't exist</exception>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void RemoveField(string name)
        {
            if(State == Metadata.State.NotDefined ||State == Metadata.State.BaseDefinition)
            {
                if(fields.Count(f => f.Name == name)>0)
                {
                    fields.RemoveAll(f => f.Name == name);
                }
                else
                {
                    throw new Exception(string.Format("The fields {0} doesn't exist", name));
                }
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }

        public MethodCreator AddMethod(string name,MethodAttributes methodAttributes,CallingConventions callingConventions)
        {
            if(!VerificationName(name))
            {
                throw new Exception("The name is already taken");
            }
            MethodCreator methodCreator = new MethodCreator(name, methodAttributes, callingConventions);
            methods.Add(methodCreator);
            return methodCreator;
        }
        /// <summary>
        /// Remove an method of the type
        /// </summary>
        /// <param name="name">name of the method to remove</param>
        /// <exception cref="System.Exception">Throw when the method doesn't exist</exception>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void RemoveMethod(string name)
        {
            if (State == Metadata.State.NotDefined || State == Metadata.State.BaseDefinition)
            {
                if (methods.Count(f => f.Name == name) > 0)
                {
                    methods.RemoveAll(f => f.Name == name);
                }
                else
                {
                    throw new Exception(string.Format("The method {0} doesn't exist", name));
                }
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }

        public PropertyCreator AddProperty(string name,Metadata.Metadata returnType,PropertyAttributes propertyAttributes)
        {
            if (!VerificationName(name))
            {
                throw new Exception("The name is already taken");
            }
            PropertyCreator property = new PropertyCreator(name, returnType, propertyAttributes);
            properties.Add(property);
            return property;
        }
        /// <summary>
        /// Remove an property of the type
        /// </summary>
        /// <param name="name">name of the property to remove</param>
        /// <exception cref="System.Exception">Throw when the property doesn't exist</exception>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void RemoveProperty(string name)
        {
            if (State == Metadata.State.NotDefined || State == Metadata.State.BaseDefinition)
            {
                if (properties.Count(f => f.Name == name) > 0)
                {
                    properties.RemoveAll(f => f.Name == name);
                }
                else
                {
                    throw new Exception(string.Format("The property {0} doesn't exist", name));
                }
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }

        #endregion

        #region Compilation
        public bool VerificationBaseDefinition(bool throwException)
        {
            if(Parent!=null && Parent.State == Metadata.State.NotDefined)
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The parent class {0} is not defined", Parent.Name));
                }
                else { return false; }
            }
            if(interfaces.Any(i => i.State == Metadata.State.NotDefined))
            {
                if (throwException)
                {
                    throw new Exception(string.Format("The interface {0} is not defined", interfaces.First(i => i.State == Metadata.State.NotDefined).Name));
                }
                else { return false; }
            }
            foreach(GenericParameterCreator parameter in genericParameters)
            {
                parameter.VerificationBaseDefinition(throwException);
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
                if (genericParameters.Count > 0)
                {
                    foreach (GenericTypeParameterBuilder genericBuilder in typeBuilder.DefineGenericParameters(genericParameters.Select(g => g.Name).ToArray()))
                    {
                        GenericParameterCreator generericParameterCreator = genericParameters.Single(g => g.Name == genericBuilder.Name);
                        generericParameterCreator.CompileBaseDefinition(genericBuilder);
                    }
                }
                foreach(CustomAttributeBuilder customAttribute in customAttributes)
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
            foreach(FieldCreator field in fields)
            {
                if(!field.VerificationBaseDefinition(throwException))
                {
                    return false;
                }
            }
            foreach(MethodCreator method in methods)
            {
                if(!method.VerificationBaseDefinition(throwException))
                {
                    return false;
                }
            }
            foreach(PropertyCreator property in properties)
            {
                if(!property.VerificationBaseDefinition(throwException))
                {
                    return false;
                }
            }
            foreach (ConstructorCreator constructor in constructors)
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
                if (State == Metadata.State.NotDefined)
                {
                    CompileBaseDefinition();
                }
                VerificationDefinition(true);
                foreach (FieldCreator field in fields)
                {
                    field.CompileBaseDefinition(typeBuilder);
                }
                foreach(MethodCreator method in methods)
                {
                    method.CompileBaseDefinitionDefinition(typeBuilder);
                }
                foreach(PropertyCreator property in properties)
                {
                    property.CompileBaseDefinition(typeBuilder);
                }
                foreach(ConstructorCreator constructor in constructors)
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
                foreach(MethodCreator method in methods)
                {
                    method.Compile(typeBuilder);
                }
                foreach(PropertyCreator property in properties)
                {
                    property.Compile(typeBuilder);
                }
                foreach(ConstructorCreator constructor in constructors)
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
            if(fields.Any(f => f.Name == name))
            {
                return false;
            }
            if(methods.Any(f => f.Name == name))
            {
                return false;
            }
            if(properties.Any(f => f.Name == name))
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
