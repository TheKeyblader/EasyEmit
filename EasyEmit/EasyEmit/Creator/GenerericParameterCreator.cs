using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace EasyEmit.Creator
{
    public class GenerericParameterCreator:Metadata.Metadata
    {

        private Metadata.Metadata typeConstraint;
        private List<Metadata.Metadata> interfaces = new List<Metadata.Metadata>();
        private List<CustomAttributeBuilder> CustomAttributes = new List<CustomAttributeBuilder>();

        internal GenerericParameterCreator(string name,Assembly assembly)
        {
            Name = name;
            State = Metadata.State.NotDefined;
            IsClass = false;
            IsEnum = false;
            IsInterface = false;
            IsGenericParameter = true;
            Assembly = assembly;
            
        }
        
        public void SetBaseConstraint(Metadata.Metadata type)
        {
            if(!type.IsClass)
            {
                throw new Exception(string.Format("The type '{0}' is not an class",type.Name));
            }
            this.typeConstraint = type;
        }

        public void SetInterfacesConstraint(params Metadata.Metadata[] interfaces)
        {
            if(interfaces.Any(i => !i.IsInterface))
            {
                throw new Exception(string.Format("The type {0} is not an interface", interfaces.First(i => !i.IsInterface).Name));
            }
            this.interfaces = interfaces.ToList();
        }
        public void SetCustomAttribute(CustomAttributeBuilder customAttributeBuilder)
        {
            if (State == Metadata.State.NotDefined)
            {
                CustomAttributes.Add(customAttributeBuilder);
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }

        public bool Verification(bool throwException)
        {
            if(typeConstraint != null && !typeConstraint.IsClass)
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The type {0} is not an class", typeConstraint.Name));
                }
                else { return false; }
            }
            if (typeConstraint != null && typeConstraint.State == Metadata.State.NotDefined)
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The type {0} is not defined", typeConstraint.Name));
                }
                else { return false; }
            }
            if(interfaces.Any(i => !i.IsInterface))
            {
                if (throwException)
                {
                    throw new Exception(string.Format("The type {0} is not an interface", interfaces.First(i => !i.IsInterface).Name));
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

        internal void Compile(GenericTypeParameterBuilder genericTypeParameterBuilder)
        {
            Verification(true);
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
            foreach(CustomAttributeBuilder customAttribute in CustomAttributes)
            {
                genericTypeParameterBuilder.SetCustomAttribute(customAttribute);
            }
            State = Metadata.State.Defined;
            type = genericTypeParameterBuilder;
        }
    }
}
