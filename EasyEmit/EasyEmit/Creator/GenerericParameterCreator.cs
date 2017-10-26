using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace EasyEmit.Creator
{
    public class GenericParameterCreator:Metadata.Metadata
    {

        private Metadata.Metadata typeConstraint;
        private List<Metadata.Metadata> interfaces = new List<Metadata.Metadata>();
        private List<CustomAttributeBuilder> customAttributes = new List<CustomAttributeBuilder>();

        internal GenericParameterCreator(Metadata.Metadata parent,string name)
        {
            Name = name;
            State = Metadata.State.NotDefined;
            IsClass = false;
            IsEnum = false;
            IsInterface = false;
            IsGenericParameter = true;
            DeclaringType = parent;
        }
        internal GenericParameterCreator(MethodCreator declaringMethod ,string name)
        {
            Name = name;
            State = Metadata.State.NotDefined;
            IsClass = false;
            IsEnum = false;
            IsInterface = false;
            IsGenericParameter = true;
            DeclaringMethod = declaringMethod;
        }
        #region BaseDefinition

        /// <summary>
        /// Set class constraint
        /// </summary>
        /// <param name="type">class used for constraint</param>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void SetBaseConstraint(Metadata.Metadata type)
        {
            if (State == Metadata.State.NotDefined)
            {
                if (!type.IsClass)
                {
                    throw new Exception(string.Format("The type '{0}' is not an class", type.Name));
                }
                this.typeConstraint = type;
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
        public void RemoveBaseConstraint()
        {
            if (State == Metadata.State.NotDefined)
            {
                typeConstraint = null;
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }

        /// <summary>
        /// Set interfaces constraint of the generic type
        /// </summary>
        /// <param name="interfaces">interfaces constraint to use</param>
        /// <exception cref="System.Exception">Throw when one of interfaces is not an interface</exception>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void SetInterfacesConstraint(params Metadata.Metadata[] interfaces)
        {
            if (State == Metadata.State.NotDefined)
            {
                if (interfaces.Any(i => !i.IsInterface))
                {
                    throw new Exception(string.Format("The type {0} is not an interface", interfaces.First(i => !i.IsInterface).Name));
                }
                this.interfaces = interfaces.ToList();
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Remove All Interfaces constraint
        /// </summary>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void RemoveAllInterfacesConstraint()
        {
            if (State == Metadata.State.NotDefined)
            {
                interfaces.Clear();
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }

        /// <summary>
        /// Add CustomAttribute
        /// </summary>
        /// <param name="customAttributeBuilder"></param>
        /// <exception cref="System.Exception">Throw when type has been already compile</exception>
        public void SetCustomAttribute(CustomAttributeBuilder customAttributeBuilder)
        {
            if (State == Metadata.State.NotDefined)
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
            if (State == Metadata.State.NotDefined)
            {
                customAttributes.Clear();
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
            if (typeConstraint != null && typeConstraint.State == Metadata.State.NotDefined)
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The type {0} is not defined", typeConstraint.Name));
                }
                else { return false; }
            }
            if(interfaces.Any(i => i.State == Metadata.State.NotDefined ))
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The interface {0} is not defined", interfaces.First(i => i.State == Metadata.State.NotDefined).Name));
                }
                else { return false; }
            }
            return true;
        }
        internal void CompileBaseDefinition(GenericTypeParameterBuilder genericTypeParameterBuilder)
        {
            VerificationBaseDefinition(true);
            State = Metadata.State.AllDefiniton;
            type = genericTypeParameterBuilder;
            if(type != null)
            {
                genericTypeParameterBuilder.SetBaseTypeConstraint(typeConstraint);
            }
            if(interfaces.Count()>0)
            {
                genericTypeParameterBuilder.SetInterfaceConstraints(interfaces.Select(t => (Type)t).ToArray());
            }
            foreach(CustomAttributeBuilder customAttribute in customAttributes)
            {
                genericTypeParameterBuilder.SetCustomAttribute(customAttribute);
            }
            State = Metadata.State.Defined;
            type = genericTypeParameterBuilder;
        }
        #endregion 
    }
}
