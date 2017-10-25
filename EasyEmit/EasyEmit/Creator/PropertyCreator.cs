using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit.Creator
{
    public class PropertyCreator : Metadata.MenberData
    {
        private PropertyAttributes propertyAttributes;
        private Metadata.Metadata returnType;
        private PropertyBuilder propertyBuilder;
        private PropertyInfo propertyInfo;
        public PropertyInfo PropertyInfo { get
            {
                return propertyInfo;
            } }
        private List<CustomAttributeBuilder> CustomAttributes = new List<CustomAttributeBuilder>();

        internal PropertyCreator(string name,Metadata.Metadata returnType,PropertyAttributes propertyAttributes)
        {
            Name = name;
            this.returnType = returnType;
            this.propertyAttributes = propertyAttributes;
        }
        private PropertyCreator(PropertyInfo propertyInfo)
        {
            State = Metadata.State.Defined;
            this.propertyInfo = propertyInfo;
        }

        #region BaseDefinition

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

        #endregion

        #region Compilation
        public bool VerificationBaseDefinition(bool throwException)
        {
            if(returnType.State == Metadata.State.NotDefined)
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The return type {0} for the property {1}", returnType.Name, Name));
                }
                else { return false; }
            }
            return true;
        }
        internal void CompileBaseDefinition(TypeBuilder typeBuilder)
        {
            VerificationBaseDefinition(true);
            propertyBuilder = typeBuilder.DefineProperty(Name, propertyAttributes, returnType, null);
            foreach(CustomAttributeBuilder customAttribute in CustomAttributes)
            {
                propertyBuilder.SetCustomAttribute(customAttribute);
            }
            propertyInfo = propertyBuilder;
            State = Metadata.State.BaseDefinition;
        }
        public bool Verification(bool throwException)
        {
            if(!VerificationBaseDefinition(throwException))
            {
                return false;
            }
            return true;
        }
        internal void Compile(TypeBuilder typeBuilder)
        {
            Verification(true);
            if(State == Metadata.State.NotDefined)
            {
                CompileBaseDefinition(typeBuilder);
            }
            propertyInfo = propertyBuilder;
            State = Metadata.State.Defined;
        }
        #endregion

        public static explicit operator PropertyCreator(PropertyInfo  propertyInfo)
        {
            return new PropertyCreator(propertyInfo);
        }
    }
}
