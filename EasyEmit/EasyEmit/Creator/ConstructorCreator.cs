using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit.Creator
{
    public class ConstructorCreator : Metadata.MenberData
    {
        private MethodAttributes methodAttributes;
        private ConstructorBuilder constructorBuilder;
        private ConstructorInfo constructorInfo;
        public ConstructorInfo ConstructorInfo { get { return constructorInfo; } }
        private IEnumerable<Metadata.Metadata> parameters;
        private List<ParameterCreator> configurationParameter = new List<ParameterCreator>();
        private List<CustomAttributeBuilder> customAttributes = new List<CustomAttributeBuilder>();
        private ConstructorCreator(ConstructorInfo constructorInfo)
        {
            State = Metadata.State.Defined;
            this.constructorInfo = constructorInfo;
        }
        internal ConstructorCreator(MethodAttributes methodAttributes,IEnumerable<Metadata.Metadata> parameters)
        {
            this.methodAttributes = methodAttributes;
            this.parameters = parameters;
        }

        #region BaseDefinition
        /// <summary>
        /// Configure one parameter of the constructor
        /// </summary>
        /// <param name="position">Position of parameter in the contrusctor</param>
        /// <param name="parameterAttributes"></param>
        /// <param name="parameterName">Name of parameter</param>
        /// <returns></returns>
        public ParameterCreator ConfigureParameter(int position,ParameterAttributes parameterAttributes,string parameterName)
        {
            if (State == Metadata.State.NotDefined)
            {
                if (parameters.ElementAt(position - 1) == null)
                {
                    throw new Exception("This parameter doesnt exist");
                }
                if (configurationParameter.Count(cp => cp.Position == position) == 1)
                {
                    throw new Exception("This parameter is alreadty configure");
                }
                if (configurationParameter.Count(cp => cp.Name == parameterName) == 1)
                {
                    throw new Exception("An another parameter already have this name");
                }
                else
                {
                    ParameterCreator parameterCreator = new ParameterCreator(position, parameterAttributes, parameterName, parameters.ElementAt(position - 1));
                    configurationParameter.Add(parameterCreator);
                    return parameterCreator;
                }
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Suppress one parameter using position
        /// </summary>
        /// <param name="position">Position of parameter to suppress</param>
        public void SuppressConfigurationParameter(int position)
        {
            if (State == Metadata.State.NotDefined)
            {
                configurationParameter.RemoveAll(cp => cp.Position == position);
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The type has been partialy compile" : "The type has been compile");
            }
        }
        /// <summary>
        /// Suppress one parameter using name
        /// </summary>
        /// <param name="name">Name of parameter to suppress</param>
        public void SuppressConfigurationParameter(string name)
        {
            if (State == Metadata.State.NotDefined)
            {
                configurationParameter.RemoveAll(cp => cp.Name == name);
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
            if (parameters != null)
            {
                if (parameters.Any(p => p == null || p.State == Metadata.State.NotDefined))
                {
                    if (throwException)
                    {
                        throw new Exception(string.Format("The type {0} is null or not defined", parameters.First(p => p == null || p.State == Metadata.State.NotDefined).Name));
                    }
                    else { return false; }
                }
                foreach(ParameterCreator parameter in configurationParameter)
                {
                    parameter.Verification(throwException);
                }
            }
            return true;
        }
        internal void CompileBaseDefinition(TypeBuilder typeBuilder)
        {
            VerificationBaseDefinition(true);
            Type[] parameters = (this.parameters == null) ? Type.EmptyTypes : this.parameters.Select(m => (Type)m).ToArray();
            constructorBuilder = typeBuilder.DefineConstructor(methodAttributes, CallingConventions.Standard, parameters);
            foreach(ParameterCreator parameter in configurationParameter)
            {
                parameter.Compile(constructorBuilder);
            }
            foreach(CustomAttributeBuilder customAttribute in customAttributes)
            {
                constructorBuilder.SetCustomAttribute(customAttribute);
            }
            constructorInfo = constructorBuilder;
            State = Metadata.State.BaseDefinition;

        }
        internal void Compile(TypeBuilder typeBuilder)
        {
            if(State == Metadata.State.NotDefined)
            {
                CompileBaseDefinition(typeBuilder);
            }
            constructorBuilder.GetILGenerator().Emit(OpCodes.Ret);
            constructorInfo = constructorBuilder;
            State = Metadata.State.Defined;
        }
        #endregion

        public static implicit operator ConstructorCreator(ConstructorInfo constructorInfo)
        {
            return new ConstructorCreator(constructorInfo);
        }
    }
}