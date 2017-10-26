using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace EasyEmit.Creator
{
    public class MethodCreator : Metadata.MenberData
    {
        private MethodAttributes methodAttributes;

        private CallingConventions callingConventions;

        private List<GenericParameterCreator> genericParameters = new List<GenericParameterCreator>();

        private Metadata.Metadata returnType;

        private List<Metadata.Metadata> parameters = new List<Metadata.Metadata>();

        private List<ParameterCreator> configurationParameter = new List<ParameterCreator>();

        private List<CustomAttributeBuilder> CustomAttributes = new List<CustomAttributeBuilder>();



        private MethodInfo methodInfo;

        public MethodInfo Method
        {
            get {
                return methodInfo;
            }
        }

        private MethodBuilder methodBuilder;

        internal MethodCreator(string name,MethodAttributes methodAttributes,CallingConventions callingConventions)
        {
            Name = name;
            State = Metadata.State.NotDefined;
            this.methodAttributes = methodAttributes;
            this.callingConventions = callingConventions;
        }
        private MethodCreator(MethodInfo methodInfo)
        {
            State = Metadata.State.Defined;
            this.methodInfo = methodInfo;
        }

        #region BaseDefinition
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
                List<GenericParameterCreator> parameters = names.Select(n => new GenericParameterCreator(this, n)).ToList();
                genericParameters = parameters;
                return parameters;
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The mmethod has been partialy compile" : "The method has been compile");
            }
        }

        public void SetReturnType(Metadata.Metadata returnType)
        {
            if (State == Metadata.State.NotDefined)
            {
                this.returnType = returnType;
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The method has been partialy compile" : "The method has been compile");
            }
        } 
        
        public void SetParameters(params Metadata.Metadata[] parameters)
        {
            if (State == Metadata.State.NotDefined)
            {
                if (parameters.Any(m => m == null))
                {
                    throw new ArgumentNullException();
                }
                this.parameters = parameters.ToList();
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The method has been partialy compile" : "The method has been compile");
            }
        }

        public ParameterCreator ConfigureParameter(int position, ParameterAttributes parameterAttributes, string parameterName)
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
                else
                {
                    ParameterCreator parameterCreator = new ParameterCreator(position, parameterAttributes, parameterName, parameters.ElementAt(position - 1));
                    configurationParameter.Add(parameterCreator);
                    return parameterCreator;
                }
            }
            else
            {
                throw new Exception((State == Metadata.State.AllDefiniton) ? "The method has been partialy compile" : "The method has been compile");
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

        #region Compile

        #endregion

        #region Compilation
        public bool VerificationBaseDefinition(bool throwException)
        {
            if(returnType != null && returnType.State == Metadata.State.NotDefined && returnType.DeclaringMethod != this)
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The returnType {0} for the method {1}", returnType.Name, Name));
                }
                else { return false; }
            }
            if(parameters.Any(p => p.State == Metadata.State.NotDefined && p.DeclaringMethod != this))
            {
                if(throwException)
                {
                    throw new Exception(string.Format("The parameters of type {0} for the method {1}", parameters.First(p => p.State == Metadata.State.NotDefined).Name, Name));
                }
                else { return false; }
            }
            if(parameters.Count >0)
            {
                foreach (ParameterCreator parameter in configurationParameter)
                {
                    parameter.Verification(throwException);
                }
            }
            foreach(GenericParameterCreator parameter in genericParameters)
            {
                parameter.VerificationBaseDefinition(throwException);
            }
            return true;
        }

        internal void CompileBaseDefinitionDefinition(TypeBuilder typeBuilder)
        {
            VerificationBaseDefinition(true);
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(Name, methodAttributes, callingConventions);
            if(genericParameters.Count > 0)
            {
                foreach(GenericTypeParameterBuilder genericBuilder in methodBuilder.DefineGenericParameters(genericParameters.Select(g => g.Name).ToArray()))
                {
                    GenericParameterCreator genericParameterCreator = genericParameters.Single(g => g.Name == genericBuilder.Name);
                    genericParameterCreator.CompileBaseDefinition(genericBuilder);
                }
            }
            methodBuilder.SetReturnType(returnType);
            if (parameters.Count > 0)
            {
                methodBuilder.SetParameters((from t in parameters select (Type)t).ToArray());
                foreach(ParameterCreator parameter in configurationParameter)
                {
                    parameter.Compile(methodBuilder);
                }
            }
            foreach(ParameterCreator parameter in configurationParameter)
            {
                parameter.Compile(methodBuilder);
            }
            foreach(CustomAttributeBuilder customAttribute in CustomAttributes)
            {
                methodBuilder.SetCustomAttribute(customAttribute);
            }
            State = Metadata.State.BaseDefinition;
            this.methodBuilder = methodBuilder;
            methodInfo = methodBuilder;
        }

        internal void Compile(TypeBuilder typeBuilder)
        {
            if(State == Metadata.State.NotDefined)
            {
                CompileBaseDefinitionDefinition(typeBuilder);
            }
            ILGenerator iLGenerator = methodBuilder.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ret);
            methodInfo = methodBuilder;
            State = Metadata.State.Defined;
        }
        #endregion

        public static implicit operator MethodCreator(MethodInfo methodInfo)
        {
            return new MethodCreator(methodInfo);
        }
    }
}
