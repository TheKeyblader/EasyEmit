using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace EasyEmit.Creator
{
    public class FieldCreator : Metadata.MenberData
    {
        private FieldAttributes fieldAttributes;
        private Metadata.Metadata type;
        private FieldBuilder fieldBuilder;
        private FieldInfo fieldInfo;
        public FieldInfo FieldInfo { get { return fieldInfo; } }
        private List<CustomAttributeBuilder> customAttributes = new List<CustomAttributeBuilder>();

        internal FieldCreator(Metadata.Metadata declaringType,string name,Metadata.Metadata metadata,FieldAttributes fieldAttributes)
        {
            this.DeclaringType = declaringType;
            this.Name = name;
            this.type = metadata;
            this.State = Metadata.State.NotDefined;
            this.fieldAttributes = fieldAttributes;
        }
        private FieldCreator(FieldInfo fieldInfo)
        {
            State = Metadata.State.Defined;
            this.fieldInfo = fieldInfo;
        }

        #region CompileBaseDefinition
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
            if(type.State == Metadata.State.NotDefined)
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The return type {0} for the fields {1} is not compiled", type.Name, Name));
                }
                else { return false; }
            }
            return true;
        }
        internal void CompileBaseDefinition(TypeBuilder typeBuilder)
        {
            if(type.State == Metadata.State.NotDefined)
            {
                throw new Exception(string.Format("The return type {0} of the field {1} has not created or not started created",type.Name,Name));
            }
            fieldBuilder = typeBuilder.DefineField(Name,type, fieldAttributes);
            foreach(CustomAttributeBuilder customAttribute in customAttributes)
            {
                fieldBuilder.SetCustomAttribute(customAttribute);
            }
            fieldInfo = fieldBuilder;
            State = Metadata.State.Defined;
        }
        #endregion

        public static implicit operator FieldCreator(FieldInfo fieldInfo)
        {
            return new FieldCreator(fieldInfo);
        }
    }
}