using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit.Creator
{
    public class ParameterCreator:Metadata.MenberData
    {
        int position;
        public int Position { get { return position; } }
        private ParameterAttributes parameterAttributes;
        private Metadata.Metadata type;
        private object value;
        private List<CustomAttributeBuilder> CustomAttributes = new List<CustomAttributeBuilder>();

        public ParameterCreator(int position,ParameterAttributes parameterAttributes,string parameterName,Metadata.Metadata type)
        {
            this.position = position;
            this.parameterAttributes = parameterAttributes;
            Name = parameterName;
            this.type = type;
        }

        #region BaseDefinition

        public void SetDefaultValue(object value)
        {
            if(State == Metadata.State.NotDefined)
            {
                if(value.GetType().Name == type.Name)
                {
                    this.value = value;
                }
                else
                {
                    throw new Exception("The value not correspond to the type " + type.Name);
                }
            }
            else
            {
                throw new Exception("The parameter is already compile");
            }
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
        #endregion

        #region Compilation
        public bool Verification(bool throwException)
        {
            if(position ==0)
            {
                if(throwException)
                {
                    throw new Exception("Parameter position start at 1");
                }
                else { return false; }
            }
            if(string.IsNullOrEmpty(Name))
            {
                if(throwException)
                {
                    throw new Exception("Parameter name cant null or empty");
                }
                else { return false; }
            }
            if(value != null && type.Name != value.GetType().Name)
            {
                if(throwException)
                {
                    throw new Exception("The object type is different to parameter type");
                }
                else { return false; }
            }
            return true;
        }
        internal void Compile(ConstructorBuilder constructorBuilder)
        {
            Verification(true);
            ParameterBuilder parameterBuilder = constructorBuilder.DefineParameter(position, parameterAttributes, Name);
            foreach(CustomAttributeBuilder customAttribute in CustomAttributes)
            {
                parameterBuilder.SetCustomAttribute(customAttribute);
            }
            if (value != null)
            {
                parameterBuilder.SetConstant(value);
            }
        }
        internal void Compile(MethodBuilder methodBuilder)
        {
            Verification(true);
            ParameterBuilder parameterBuilder = methodBuilder.DefineParameter(position, parameterAttributes, Name);
            if(value!=null)
            {
                parameterBuilder.SetConstant(value);
            }
        }
        #endregion
    }
}
